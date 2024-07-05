Shader "Default/SSR"

Properties
{
	_Resolution("resolution", FLOAT2)
	_GColor("g color", TEXTURE_2D)
	_GNormalMetallic("g normal metallic", TEXTURE_2D)
	_GPositionRoughness("g position roughness", TEXTURE_2D)
	_GDepth("g depth", TEXTURE_2D)
	
	_MatProjection("projection matrix", MATRIX_4X4)
	_MatProjectionInverse("inverse projection matrix", MATRIX_4X4)
	_Time("time", FLOAT)
	_Frame("frame", INT)
	_SSRThreshold("ssr threshold", FLOAT)
	_SSRSteps("ssr steps", INT)
	_SSRBisteps("ssr bisteps", INT)
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

		void main() 
		{
			gl_Position =vec4(vertexPosition, 1.0);
		}
	}

	Fragment
	{
		layout(location = 0) out vec4 OutputColor;
		
		uniform vec2 _Resolution;
		
		uniform sampler2D _GColor; // Depth
		uniform sampler2D _GNormalMetallic; // Depth
		uniform sampler2D _GPositionRoughness; // Depth
		uniform sampler2D _GDepth; // Depth
		
		uniform mat4 _MatProjection;
		uniform mat4 _MatProjectionInverse;

		uniform float _Time;
		uniform int _Frame;

		uniform float _SSRThreshold;
		uniform int _SSRSteps; // [16 20 24 28 32]
		uniform int _SSRBisteps; // [0 4 8 16]
		
		#include "Random"
		#include "Utilities"
		#include "PBR"

		// ----------------------------------------------------------------------------
		
		vec3 calculateSSR(vec3 viewPos, vec3 screenPos, vec3 gBMVNorm, float dither) {
			vec3 reflectedScreenPos = rayTrace(screenPos, viewPos, reflect(normalize(viewPos), gBMVNorm), dither, _SSRSteps, _SSRBisteps, _GDepth);
			if(reflectedScreenPos.z < 0.5) return vec3(0);
			return vec3(reflectedScreenPos.xy, 1);
		}

		void main()
		{
			vec2 texCoords = gl_FragCoord.xy / _Resolution;

			vec3 color = texture(_GColor, texCoords).xyz;
			OutputColor = vec4(color, 1.0);

			vec4 viewPosAndRough = texture(_GPositionRoughness, texCoords);
			float smoothness = 1.0 - viewPosAndRough.w;
			
			smoothness = smoothness * smoothness;

			if(smoothness > _SSRThreshold)
			{
				vec4 normalAndMetallic = texture(_GNormalMetallic, texCoords);
				vec3 normal = normalAndMetallic.xyz;
				float metallic = normalAndMetallic.w;
				
				// Per-Pixel Roughness, Works great but needs a Denioser/Blur, TAA Helps but overall looks better without this
				//vec3 perturbedNormal = normalize(vec3(RandNextF(), RandNextF(), RandNextF()) * 2.0 - 1.0);
				//normal = normalize(mix(normal, perturbedNormal, viewPosAndRough.w * 0.4));

				vec3 screenPos = getScreenPos(texCoords, _GDepth);
				vec3 viewPos = getViewFromScreenPos(screenPos);

				bool isMetal = metallic > 0.9;

				// Get fresnel
				vec3 F0 = vec3(0.04); 
				F0 = mix(F0, color, metallic);
				vec3 fresnel = FresnelSchlick(max(dot(normal, normalize(-viewPos)), 0.0), F0);
				
				float dither = fract(sin(dot(texCoords + vec2(_Time, _Time), vec2(12.9898,78.233))) * 43758.5453123);

				vec3 SSRCoord = calculateSSR(viewPos, screenPos, normalize(normal), dither);
				if(SSRCoord.z > 0.5)
				{
					OutputColor.rgb *= isMetal ? vec3(1.0 - smoothness) : 1.0 - fresnel;
					OutputColor.rgb += texture(_GColor, SSRCoord.xy).xyz * fresnel;
				}
			}
		}

	}
}