Shader "Default/Standard"

Properties
{
	u_MatModel("Model Matrix", MATRIX_4X4)
	u_MatView("View Matrix", MATRIX_4X4)
	u_MatMVP("Model View Projection Matrix", MATRIX_4X4)
	u_MainTex("Main Texture", TEXTURE_2D)
	u_NormalTex("Normal Texture", TEXTURE_2D)
	u_SurfaceTex("Surface Texture", TEXTURE_2D)
	u_EmissionTex("Emission Texture", TEXTURE_2D)
	u_EmissiveColor("Emissive Color", COLOR)
	u_MainColor("Main Color", COLOR)
	u_EmissionIntensity("Emission Intensity", FLOAT)
}

Pass 0
{
	// Default Raster state
	Blend Off

	Vertex
	{
		#include "VertexAttributes"

		out vec3 FragPos;
		out vec3 Pos;
		out vec2 TexCoords0;
		out vec2 TexCoords1;
		out vec3 VertNormal;
		out vec4 VertColor;
		out mat3 TBN;
		out vec4 PosProj;
		
		uniform mat4 u_MatModel;
		uniform mat4 u_MatView;
		uniform mat4 u_MatMVP;

		void main()
		{
		    /*
		    * Position and Normal are in view space
		    */
		 	vec4 viewPos = u_MatView * u_MatModel * vec4(vertexPosition, 1.0);
		    Pos = (u_MatModel * vec4(vertexPosition, 1.0)).xyz;
		    FragPos = viewPos.xyz; 
		    TexCoords0 = vertexTexCoord0;
		    TexCoords1 = vertexTexCoord1;
		    VertColor = vertexColor;

			mat3 normalMatrix = transpose(inverse(mat3(u_MatModel)));
			VertNormal = normalize(normalMatrix * vertexNormal);
			
			// Construct the TBN matrix, where the letters depict a Tangent, BiTangent and Normal vectors: https://learnopengl.com/Advanced-Lighting/Normal-Mapping
			vec3 T = normalize(vec3(u_MatModel * vec4(vertexTangent, 0.0)));
			vec3 B = normalize(vec3(u_MatModel * vec4(cross(vertexNormal, vertexTangent), 0.0)));
			vec3 N = normalize(vec3(u_MatModel * vec4(vertexNormal, 0.0)));
		    TBN = mat3(T, B, N);
		
		    PosProj = u_MatMVP * vec4(vertexPosition, 1.0);
		
		    gl_Position = PosProj;
		}
	}

	Fragment
	{
	    #extension GL_ARB_derivative_control : enable
	
	    in vec3 FragPos;
	    in vec3 Pos;
	    in vec2 TexCoords0;
	    in vec2 TexCoords1;
	    in vec3 VertNormal;
	    in vec4 VertColor;
	    in mat3 TBN;
	    in vec4 PosProj;
	
	    uniform mat4 u_MatView;
	    
	    uniform sampler2D u_MainTex; // diffuse
	    uniform sampler2D u_NormalTex; // Normal
	    uniform sampler2D u_SurfaceTex; // AO, Roughness, Metallic
	    uniform sampler2D u_EmissionTex; // Emissive
	    uniform vec4 u_EmissiveColor; // Emissive color
	    uniform vec4 u_MainColor; // color
	    uniform float u_EmissionIntensity;
	
	    out vec4 FragColor;
	
	    void main()
	    {
	        vec2 uv = TexCoords0;
	
	        vec4 alb = texture(u_MainTex, uv).rgba; 
	        alb.rgb *= VertColor.rgb;
	
	        // AO, Roughness, Metallic
	        vec3 surface = texture(u_SurfaceTex, uv).rgb;
	
	        // Normal & Metallic
	        vec3 normal = texture(u_NormalTex, uv).rgb;
	        normal = normal * 2.0 - 1.0;   
	        normal = normalize(TBN * normal); 
	
	        // Emission
	        vec3 emission = (texture(u_EmissionTex, uv).rgb + u_EmissiveColor.rgb) * u_EmissionIntensity;
	
	        // Combine all the components to get the final color
	        FragColor = vec4(alb.rgb * surface.r + emission, alb.a);
	    }
	}
}

			
ShadowPass 0
{
	CullFace Front

	Vertex
	{
		layout (location = 0) in vec3 vertexPosition;
#ifdef HAS_UV
		layout (location = 1) in vec2 vertexTexCoord0;
#else
		vec2 vertexTexCoord0 = vec2(0.0, 0.0);
#endif
#ifdef SKINNED
	#ifdef HAS_BONEINDICES
		layout (location = 6) in vec4 vertexBoneIndices;
	#else
		vec4 vertexBoneIndices = vec4(0, 0, 0, 0);
	#endif

	#ifdef HAS_BONEWEIGHTS
		layout (location = 7) in vec4 vertexBoneWeights;
	#else
		vec4 vertexBoneWeights = vec4(0.0, 0.0, 0.0, 0.0);
	#endif
		
		const int MAX_BONE_INFLUENCE = 4;
		const int MAX_BONES = 100;
		uniform mat4 bindPoses[MAX_BONES];
		uniform mat4 boneTransforms[MAX_BONES];
#endif
		
		out vec2 TexCoords;

		uniform mat4 u_MatMVP;
		void main()
		{
			vec3 boneVertexPosition = vertexPosition;
			
#ifdef SKINNED    
			vec4 totalPosition = vec4(0.0);

			for (int i = 0; i < MAX_BONE_INFLUENCE; i++)
			{
			    int index = int(vertexBoneIndices[i]) - 1;
			    if (index < 0)
			        continue;

			    float weight = vertexBoneWeights[i];
			    mat4 boneTransform = boneTransforms[index] * bindPoses[index];

			    totalPosition += (boneTransform * vec4(vertexPosition, 1.0)) * weight;
			}

			boneVertexPosition = totalPosition.xyz;
#endif

		    TexCoords = vertexTexCoord0;
			
		    gl_Position = u_MatMVP * vec4(boneVertexPosition, 1.0);
		}
	}

	Fragment
	{
		layout (location = 0) out float fragmentdepth;
		
		uniform sampler2D u_MainTex; // diffuse
		uniform vec4 u_MainColor;
		uniform int Frame;

		in vec2 TexCoords;
		
		float InterleavedGradientNoise(vec2 pixel, int frame) 
		{
		    pixel += (float(frame) * 5.588238f);
		    return fract(52.9829189f * fract(0.06711056f*float(pixel.x) + 0.00583715f*float(pixel.y)));  
		}

		void main()
		{
			float alpha = texture(u_MainTex, TexCoords).a; 
			float rng = InterleavedGradientNoise(gl_FragCoord.xy, Frame % 32);
			if(rng > alpha * u_MainColor.a) discard;
		}
	}
}