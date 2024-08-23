Shader "Default/Basic"

Properties
{
	_Texture0("texture", TEXTURE_2D)
}

Pass 0
{
	BlendSrc SrcAlpha
	BlendDst One

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
		uniform sampler2D _Texture0;
		
		out vec4 finalColor;
		
		void main()
		{
		    finalColor = vec4(texture(_Texture0, TexCoords).xyz, 1.0);
		}
	}
}