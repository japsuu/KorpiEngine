Shader "Default/Invalid"

Properties
{
	_MatMVP("Model View Projection Matrix", MATRIX_4X4)
}

Pass 0
{
	// Default Raster state

	Vertex
	{
		layout (location = 0) in vec3 vertexPosition;

		uniform mat4 _MatMVP;

		void main()
		{
		    gl_Position = _MatMVP * vec4(vertexPosition, 1.0);
		}
	}

	Fragment
	{
		layout (location = 0) out vec4 fragColor;
		
		void main()
		{
			fragColor = vec4(1.0, 0.0, 1.0, 1.0);
		}
	}
}