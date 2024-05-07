﻿Shader "Default/Standard"

Properties
{
	// Material property declarations go here
	_MainTex("Albedo Map", TEXTURE_2D)
	_NormalTex("Normal Map", TEXTURE_2D)
	_EmissionTex("Emissive Map", TEXTURE_2D)
	_SurfaceTex("Surface Map x:AO y:Rough z:Metal", TEXTURE_2D)
	_OcclusionTex("Occlusion Map", TEXTURE_2D)

	_EmissiveColor("Emissive Color", COLOR)
	_EmissionIntensity("Emissive Intensity", FLOAT)
	_MainColor("Main Color", COLOR)
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
		out vec4 PosProjOld;
		
		uniform mat4 matModel;
		uniform mat4 matView;
		uniform mat4 mvp;
		uniform mat4 mvpOld;

		void main()
		{
			vec3 boneVertexPosition = vertexPosition;
			vec3 boneVertexNormal = vertexNormal;
			vec3 boneVertexTangent = vertexTangent;
			
#ifdef SKINNED    
			vec4 totalPosition = vec4(0.0);
			vec3 totalNormal = vec3(0.0);
			vec3 totalTangent = vec3(0.0);

			for (int i = 0; i < MAX_BONE_INFLUENCE; i++)
			{
			    int index = int(vertexBoneIndices[i]) - 1;
			    if (index < 0)
			        continue;

			    float weight = vertexBoneWeights[i];
			    mat4 boneTransform = boneTransforms[index] * bindPoses[index];

			    totalPosition += (boneTransform * vec4(vertexPosition, 1.0)) * weight;
			    totalNormal += (mat3(boneTransform) * vertexNormal) * weight;
			    totalTangent += (mat3(boneTransform) * vertexTangent) * weight;
			}

			boneVertexPosition = totalPosition.xyz;
			boneVertexNormal = normalize(totalNormal);
			boneVertexTangent = normalize(totalTangent);
#endif

		    /*
		    * Position and Normal are in view space
		    */
		 	vec4 viewPos = matView * matModel * vec4(boneVertexPosition, 1.0);
		    Pos = (matModel * vec4(boneVertexPosition, 1.0)).xyz;
		    FragPos = viewPos.xyz; 
		    TexCoords0 = vertexTexCoord0;
		    TexCoords1 = vertexTexCoord1;
		    VertColor = vertexColor;

			mat3 normalMatrix = transpose(inverse(mat3(matModel)));
			VertNormal = normalize(normalMatrix * boneVertexNormal);
			
			vec3 T = normalize(vec3(matModel * vec4(boneVertexTangent, 0.0)));
			vec3 B = normalize(vec3(matModel * vec4(cross(boneVertexNormal, boneVertexTangent), 0.0)));
			vec3 N = normalize(vec3(matModel * vec4(boneVertexNormal, 0.0)));
		    TBN = mat3(T, B, N);
		
		    PosProj = mvp * vec4(boneVertexPosition, 1.0);
		    PosProjOld = mvpOld * vec4(boneVertexPosition, 1.0);
		
		    gl_Position = PosProj;
		}
	}

	Fragment
	{
		#extension GL_ARB_derivative_control : enable

		layout (location = 0) out vec4 gAlbedoAO; // AlbedoR, AlbedoG, AlbedoB, Ambient Occlusion
		layout (location = 1) out vec4 gNormalMetallic; // NormalX, NormalY, NormalZ, Metallic
		layout (location = 2) out vec4 gPositionRoughness; // PositionX, PositionY, PositionZ, Roughness
		layout (location = 3) out vec3 gEmission; // EmissionR, EmissionG, EmissionB, 
		layout (location = 4) out vec2 gVelocity; // VelocityX, VelocityY
		layout (location = 5) out float gObjectID; // ObjectID

		in vec3 FragPos;
		in vec3 Pos;
		in vec2 TexCoords0;
		in vec2 TexCoords1;
		in vec3 VertNormal;
		in vec4 VertColor;
		in mat3 TBN;
		in vec4 PosProj;
		in vec4 PosProjOld;

		uniform int ObjectID;

		uniform mat4 matView;
		
		uniform vec2 Jitter;
		uniform vec2 PreviousJitter;
		uniform int Frame;
		uniform vec2 Resolution;
		
		uniform sampler2D NoiseTexture; // r:Blue g:White b:Voronoi

		uniform sampler2D _MainTex; // diffuse
		uniform sampler2D _NormalTex; // Normal
		uniform sampler2D _SurfaceTex; // AO, Roughness, Metallic
		uniform sampler2D _EmissionTex; // Emissive
		uniform vec4 _EmissiveColor; // Emissive color
		uniform vec4 _MainColor; // color
		uniform float _EmissionIntensity;

		// Interesting method someone gave me, Not sure if it works
		vec2 UnjitterTextureUV(vec2 uv, vec2 currentJitterInPixels)
		{
		    // Note: We negate the y because UV and screen space run in opposite directions
			return uv;
			// return uv - ddx(uv) * currentJitterInPixels.x + ddy(uv) * currentJitterInPixels.y;
		    // return uv - ddx_fine(uv) * currentJitterInPixels.x + ddy_fine(uv) * currentJitterInPixels.y;
		}

		float InterleavedGradientNoise(vec2 pixel, int frame) 
		{
		    pixel += (float(frame) * 5.588238f);
		    return fract(52.9829189f * fract(0.06711056f*float(pixel.x) + 0.00583715f*float(pixel.y)));  
		}
		
		void main()
		{
			vec2 uv = UnjitterTextureUV(TexCoords0, Jitter);
			//vec2 uv = TexCoords;

			vec4 alb = texture(_MainTex, uv).rgba; 
			float rng = InterleavedGradientNoise(gl_FragCoord.xy, Frame % 32);
			if(rng > alb.a * _MainColor.a) discard;
			alb.rgb *= VertColor.rgb;

			// AO, Roughness, Metallic
			vec3 surface = texture(_SurfaceTex, uv).rgb;
			// Albedo
			//gAlbedoAO = vec4(alb.xyz * _MainColor.rgb, ao);
			gAlbedoAO = vec4(pow(alb.xyz * _MainColor.rgb, vec3(2.2)), surface.r);
	
			// Position & Roughness
			gPositionRoughness = vec4(FragPos, surface.g);

			// Normal & Metallic
			vec3 normal = texture(_NormalTex, uv).rgb;
			normal = normal * 2.0 - 1.0;   
			normal = normalize(TBN * normal); 
			gNormalMetallic = vec4((matView * vec4(normal, 0)).rgb, surface.b);
			
			// Emission
			gEmission.rgb = (texture(_EmissionTex, uv).rgb + _EmissiveColor.rgb) * _EmissionIntensity;

			// Velocity
			vec2 a = (PosProj.xy / PosProj.w) - Jitter;
			vec2 b = (PosProjOld.xy / PosProjOld.w) - PreviousJitter;
			gVelocity.xy = (b - a) * 0.5;



			gObjectID = float(ObjectID);
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

		uniform mat4 mvp;
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
			
		    gl_Position = mvp * vec4(boneVertexPosition, 1.0);
		}
	}

	Fragment
	{
		layout (location = 0) out float fragmentdepth;
		
		uniform sampler2D _MainTex; // diffuse
		uniform vec4 _MainColor;
		uniform int Frame;

		in vec2 TexCoords;
		
		float InterleavedGradientNoise(vec2 pixel, int frame) 
		{
		    pixel += (float(frame) * 5.588238f);
		    return fract(52.9829189f * fract(0.06711056f*float(pixel.x) + 0.00583715f*float(pixel.y)));  
		}

		void main()
		{
			float alpha = texture(_MainTex, TexCoords).a; 
			float rng = InterleavedGradientNoise(gl_FragCoord.xy, Frame % 32);
			if(rng > alpha * _MainColor.a) discard;
		}
	}
}