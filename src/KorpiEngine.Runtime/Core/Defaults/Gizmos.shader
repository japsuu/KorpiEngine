Shader "Default/Gizmos"

Properties
{
	_MatMVP("mvp matrix", MATRIX_4X4)
}

Pass 0
{
	// Default Raster state
	Blend On
	BlendSrc SrcAlpha
	BlendDst One

	Vertex
	{
		layout (location = 0) in vec3 vertexPosition;
		layout (location = 1) in vec4 vertexColor;

		out vec4 VertColor;
		
		uniform mat4 _MatMVP;

		void main()
		{
		    VertColor = vertexColor;

		    gl_Position = _MatMVP * vec4(vertexPosition, 1.0);
		}
	}

	Fragment
	{
		in vec4 VertColor;

		out vec4 finalColor;

		void main()
		{
			finalColor = VertColor;
		}
	}
}