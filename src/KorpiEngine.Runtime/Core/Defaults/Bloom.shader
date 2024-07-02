Shader "Default/Bloom"

Properties
{
	_Resolution("resolution", FLOAT2)
	_GColor("g color", TEXTURE_2D)
	_Radius("radius", FLOAT)
	_Threshold("threshold", FLOAT)
	_Alpha("alpha", FLOAT)
}

Pass 0
{
	DepthTest Off
	DepthWrite Off
	// DepthMode Less
	Blend On
	BlendSrc SrcAlpha
	BlendDst OneMinusSrcAlpha
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
		layout(location = 0) out vec4 OutputColor;
		
		in vec2 TexCoords;
		uniform vec2 _Resolution;
		
		uniform sampler2D _GColor;

		uniform float _Radius;
		uniform float _Threshold;
		uniform float _Alpha;
		
		// ----------------------------------------------------------------------------
		
		void main()
		{
			// Kawase Blur
			vec2 ps = (vec2(1.0, 1.0) / _Resolution) * _Radius;
			vec3 thres = vec3(_Threshold, _Threshold, _Threshold);
			vec3 zero = vec3(0.0, 0.0, 0.0);
			vec3 color = max(texture(_GColor, TexCoords).rgb - thres, zero);
            color += max(texture(_GColor, TexCoords + vec2(ps.x, ps.y)).rgb - thres, zero);
            color += max(texture(_GColor, TexCoords + vec2(ps.x, -ps.y)).rgb - thres, zero);
            color += max(texture(_GColor, TexCoords + vec2(-ps.x, ps.y)).rgb - thres, zero);
            color += max(texture(_GColor, TexCoords + vec2(-ps.x, -ps.y)).rgb - thres, zero);
			color /= 5.0;


			OutputColor = vec4(color, _Alpha);
		}

	}
}