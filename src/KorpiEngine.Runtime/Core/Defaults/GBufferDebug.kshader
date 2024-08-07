Shader "Default/GBufferDebug"

Properties
{
	_GAlbedoAO("g diffuse", TEXTURE_2D)
	_GNormalMetallic("g normal metallic", TEXTURE_2D)
	_GPositionRoughness("g pos roughness", TEXTURE_2D)
	_GEmission("g emission", TEXTURE_2D)
	_GVelocity("g velocity", TEXTURE_2D)
	_GObjectID("g objectID", TEXTURE_2D)
	_GDepth("g depth", TEXTURE_2D)
	_GUnlit("g unlit", TEXTURE_2D)
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
		in vec2 TexCoords;

		uniform sampler2D _GAlbedoAO;
		uniform sampler2D _GNormalMetallic;
		uniform sampler2D _GPositionRoughness;
		uniform sampler2D _GEmission;
		uniform sampler2D _GVelocity;
		uniform sampler2D _GObjectID;
		uniform sampler2D _GDepth;
		uniform sampler2D _GUnlit;
		
		layout(location = 0) out vec4 OutputColor;
		
		void main()
		{
			vec4 color = vec4(1.0, 0.0, 1.0, 1.0);
			
			
#ifdef ALBEDO
			color = vec4(texture(_GAlbedoAO, TexCoords).rgb, 1.0);
#endif

			
#ifdef AO
			color = vec4(vec3(texture(_GAlbedoAO, TexCoords).a), 1.0);
#endif
			
			
#ifdef NORMAL
			vec3 normals = texture(_GNormalMetallic, TexCoords).rgb;	// Normal in View Space
			color = vec4(normals, 1.0);
#endif

			
#ifdef METALLIC
			color = vec4(vec3(texture(_GNormalMetallic, TexCoords).a), 1.0);
#endif

			
#ifdef POSITION
			color = vec4(texture(_GPositionRoughness, TexCoords).rgb, 1.0);
#endif

			
#ifdef ROUGHNESS
			color = vec4(vec3(texture(_GPositionRoughness, TexCoords).a), 1.0);
#endif

			
#ifdef EMISSION
			color = vec4(texture(_GEmission, TexCoords).rgb, 1.0);
#endif

			
#ifdef VELOCITY
			vec2 vel = texture(_GVelocity, TexCoords).rg;
			color = vec4(vel.x, 0.0, vel.y, 1.0);
#endif

			
#ifdef OBJECTID
			color = vec4(texture(_GObjectID, TexCoords).r, 0.0, 0.0, 1.0);
#endif

			
#ifdef DEPTH
			float depth = texture(_GDepth, TexCoords).r;
			float near = 0.1; 	//TODO: Pull from Camera
			float far = 100.0;	//TODO: Pull from Camera
			float ndc = depth * 2.0 - 1.0;
			float linearDepth = (2.0 * near * far) / (far + near - ndc * (far - near));
			float visual = linearDepth / far;
			color = vec4(vec3(visual), 1.0);
#endif
			
			
#ifdef UNLIT
			color = texture(_GUnlit, TexCoords);
#endif
		
			
#ifdef WIREFRAME
			// Just draw the ObjectID buffer
			color = vec4(texture(_GObjectID, TexCoords).r, 0.0, 0.0, 1.0);
#endif

			
			OutputColor = color;
		}
	}
}