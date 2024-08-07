Shader "Default/AmbientLight"

Properties
{
	_MatViewInverse("inverse view matrix", MATRIX_4X4)
	_SkyColor("sky color", FLOAT4)
	_GroundColor("ground color", FLOAT4)
	_SkyIntensity("sky intensity", FLOAT)
	_GroundIntensity("ground intensity", FLOAT)
	_GAlbedoAO("g albedo & roughness", TEXTURE_2D)
	_GNormalMetallic("g normal and metallness", TEXTURE_2D)
	_GPositionRoughness("g depth", TEXTURE_2D)
}

Pass 0
{
	DepthTest Off
	DepthWrite Off
	// DepthMode Less
	Blend On
	BlendSrc SrcAlpha
	BlendDst One
	BlendMode Add
	Cull Off
	// Winding CW

	Vertex
	{
		in vec3 vertexPosition;
		in vec2 vertexTexCoord;
		
		out vec2 TexCoords;

		void main() 
		{
			gl_Position = vec4(vertexPosition, 1.0);
			TexCoords = vertexTexCoord;
		}
	}

	Fragment
	{
		layout(location = 0) out vec4 gLighting;
		
		uniform mat4 _MatViewInverse;
		
		in vec2 TexCoords;
		
		uniform vec4 _SkyColor;
		uniform vec4 _GroundColor;
		uniform float _SkyIntensity;
		uniform float _GroundIntensity;
		
		uniform sampler2D _GAlbedoAO;			// Albedo (r,g,b) & Ambient Occlusion (a)
		uniform sampler2D _GNormalMetallic;		// Normal (r,g,b) & Metalness (a)
		uniform sampler2D _GPositionRoughness;	// Position (r,g,b) & Roughness (a)
		
		void main()
		{
			// Test the view-space position for valid data.
			// If invalid (e.g. skybox, post-processing effects), discard the fragment.
			vec3 viewSpacePos = textureLod(_GPositionRoughness, TexCoords, 0).xyz;
			if(viewSpacePos == vec3(0, 0, 0))
				discard;

			// Get view-space normal
			vec3 viewSpaceNormal = textureLod(_GNormalMetallic, TexCoords, 0).xyz;
			
			// Transform the normal from view-space to world-space
   			vec3 worldSpaceNormal = normalize(_MatViewInverse * vec4(viewSpaceNormal, 1.0)).xyz;

   			// Obtain the local up vector in world space
   			vec3 upVector = vec3(0.0, 1.0, 0.0);

			// Calculate how much the normal is pointing upwards
			float normalUp = max(0.0, dot(worldSpaceNormal, upVector));

			// Interpolate between _SkyColor and GroundColor based on normalUp
			vec3 ambientColor = mix(_GroundColor.rgb * _GroundIntensity, _SkyColor.rgb * _SkyIntensity, normalUp);

			// Apply ambient lighting to the albedo
			//vec3 albedo = textureLod(_GAlbedoAO, TexCoords, 0).rgb;
			//gLighting = vec4(albedo * ambientColor, 1.0);
			gLighting = vec4(worldSpaceNormal,1);
		}
	}
}