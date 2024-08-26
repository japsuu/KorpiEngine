Shader "Default/Depth"

Properties
{
	_MatMVP("mvp matrix", MATRIX_4X4)
}

Pass 0
{
	CullFace Front

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