Shader "Default/Invalid"

Properties
{
	_MatModel("model matrix", MATRIX_4X4)
	_MatView("view matrix", MATRIX_4X4)
	_MatMVP("mvp matrix", MATRIX_4X4)
	_MatMVPOld("old mvp matrix", MATRIX_4X4)
	_Jitter("jitter", FLOAT2)
	_PreviousJitter("previous jitter", FLOAT2)
	_ObjectID("object id", INT)
}

Pass 0
{
	// Default Raster state

	Vertex
	{
		layout (location = 0) in vec3 vertexPosition;
#ifdef HAS_NORMALS
		layout (location = 3) in vec3 vertexNormal;
#else
		vec3 vertexNormal = vec3(0.0, 1.0, 0.0);
#endif

		out vec3 FragPos;
		out vec3 VertNormal;
		out vec4 PosProj;
		out vec4 PosProjOld;
		
		uniform mat4 _MatModel;
		uniform mat4 _MatView;
		uniform mat4 _MatMVP;
		uniform mat4 _MatMVPOld;

		void main()
		{
		 	vec4 viewPos = _MatView * _MatModel * vec4(vertexPosition, 1.0);
			VertNormal = (_MatView * vec4(vertexNormal, 0.0)).xyz;
		    FragPos = viewPos.xyz; 

		    PosProj = _MatMVP * vec4(vertexPosition, 1.0);
		    PosProjOld = _MatMVPOld * vec4(vertexPosition, 1.0);
		
		    gl_Position = PosProj;
		}
	}

	Fragment
	{
		layout (location = 0) out vec4 gAlbedoAO; // AlbedoR, AlbedoG, AlbedoB, Ambient Occlusion
		layout (location = 1) out vec4 gNormalMetallic; // NormalX, NormalY, NormalZ, Metallic
		layout (location = 2) out vec4 gPositionRoughness; // PositionX, PositionY, PositionZ, Roughness
		layout (location = 4) out vec2 gVelocity; // VelocityX, VelocityY
		layout (location = 5) out float gObjectID; // ObjectID

		in vec3 FragPos;
		in vec3 VertNormal;
		in vec4 PosProj;
		in vec4 PosProjOld;

		uniform vec2 _Jitter;
		uniform vec2 _PreviousJitter;
		uniform int _ObjectID;
		
		void main()
		{
			gAlbedoAO = vec4(1.0, 0.0, 1.0, 0.0);
			gPositionRoughness = vec4(FragPos, 0.5);
			gNormalMetallic = vec4(VertNormal, 0.5);

			// Velocity
			vec2 a = (PosProj.xy / PosProj.w) - _Jitter;
			vec2 b = (PosProjOld.xy / PosProjOld.w) - _PreviousJitter;
			gVelocity.xy = (b - a) * 0.5;

			gObjectID = float(_ObjectID);
		}
	}
}

			
ShadowPass 0
{
	Vertex
	{
		layout (location = 0) in vec3 vertexPosition;
		
		uniform mat4 _MatMVP;
		void main()
		{
		    gl_Position =  _MatMVP * vec4(vertexPosition, 1.0);
		}
	}

	Fragment
	{
		layout (location = 0) out float fragmentdepth;
		
		void main()
		{
		}
	}
}