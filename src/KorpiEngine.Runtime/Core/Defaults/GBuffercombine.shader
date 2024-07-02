Shader "Default/GBuffer"

Properties
{
	_GAlbedoAO("g diffuse", TEXTURE_2D)
	_GLighting("g lighting", TEXTURE_2D)
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
			gl_Position =vec4(vertexPosition, 1.0);
			TexCoords = vertexTexCoord;
		}
	}

	Fragment
	{
		in vec2 TexCoords;

		uniform sampler2D _GAlbedoAO; // Diffuse
		uniform sampler2D _GLighting; // Lighting
		
		layout(location = 0) out vec4 OutputColor;
		
		void main()
		{
			vec4 albedoAO = texture(_GAlbedoAO, TexCoords);
			vec3 diffuseColor = albedoAO.rgb * 0.01;
			vec3 lightingColor = texture(_GLighting, TexCoords).rgb;
			// Apply AO onto the lightingColor
			// AO comes in as 0-1, 0 being no AO, 1 being full AO
			lightingColor *= (1.0 - albedoAO.w);
		
			vec3 color = diffuseColor + (lightingColor);

			OutputColor = vec4(color, 1.0);
		}
	}
}