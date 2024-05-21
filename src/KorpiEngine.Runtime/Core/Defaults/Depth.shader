Shader "Default/Depth"

Properties
{
}

Pass 0
{
	CullFace Front

	Vertex
	{
		layout (location = 0) in vec3 vertexPosition;

		uniform mat4 mvp;
		void main()
		{
		    gl_Position =  mvp * vec4(vertexPosition, 1.0);
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