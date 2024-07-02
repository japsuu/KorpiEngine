﻿Shader "Default/TAA"

Properties
{
	_Resolution("resolution", FLOAT2)
	_GColor("g color", TEXTURE_2D)
	_GHistory("g history", TEXTURE_2D)
	_GPositionRoughness("g position roughness", TEXTURE_2D)
	_GVelocity("g velocity", TEXTURE_2D)
	_GDepth("g depth", TEXTURE_2D)
	_MatProjection("projection matrix", MATRIX_4X4)
	_MatProjectionInverse("inverse projection matrix", MATRIX_4X4)
	_ClampRadius("clamp radius", INT)
	_Jitter("jitter", FLOAT2)
	_PreviousJitter("previous jitter", FLOAT2)
}

Pass 0
{
	DepthTest Off
	DepthWrite Off
	Blend Off
	Cull Off

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

		uniform sampler2D _GColor;
		uniform sampler2D _GHistory;
		uniform sampler2D _GPositionRoughness;
		uniform sampler2D _GVelocity;
		uniform sampler2D _GDepth;
		
		uniform mat4 _MatProjection;
		uniform mat4 _MatProjectionInverse;

		uniform int _ClampRadius;
		uniform vec2 _Jitter;
		uniform vec2 _PreviousJitter;

		#include "Utilities"

		// ----------------------------------------------------------------------------

		// from http://www.java-gaming.org/index.php?topic=35123.0
		vec4 cubic(float v){
		    vec4 n = vec4(1.0, 2.0, 3.0, 4.0) - v;
		    vec4 s = n * n * n;
		    float x = s.x;
		    float y = s.y - 4.0 * s.x;
		    float z = s.z - 4.0 * s.y + 6.0 * s.x;
		    float w = 6.0 - x - y - z;
		    return vec4(x, y, z, w) * (1.0/6.0);
		}
		
		vec4 textureBicubic(sampler2D sampler, vec2 texCoords){
		
		   vec2 texSize = textureSize(sampler, 0);
		   vec2 invTexSize = 1.0 / texSize;
		   
		   texCoords = texCoords * texSize - 0.5;
		
		   
		    vec2 fxy = fract(texCoords);
		    texCoords -= fxy;
		
		    vec4 xcubic = cubic(fxy.x);
		    vec4 ycubic = cubic(fxy.y);
		
		    vec4 c = texCoords.xxyy + vec2 (-0.5, +1.5).xyxy;
		    
		    vec4 s = vec4(xcubic.xz + xcubic.yw, ycubic.xz + ycubic.yw);
		    vec4 offset = c + vec4 (xcubic.yw, ycubic.yw) / s;
		    
		    offset *= invTexSize.xxyy;
		    
		    vec4 sample0 = texture(sampler, offset.xz);
		    vec4 sample1 = texture(sampler, offset.yz);
		    vec4 sample2 = texture(sampler, offset.xw);
		    vec4 sample3 = texture(sampler, offset.yw);
		
		    float sx = s.x / (s.x + s.y);
		    float sy = s.z / (s.z + s.w);
		
		    return mix(
		       mix(sample3, sample2, sx), mix(sample1, sample0, sx)
		    , sy);
		}

		// https://www.elopezr.com/temporal-aa-and-the-quest-for-the-holy-trail/
		vec3 AdjustHDRColor(vec3 color) {
			return vec3(color.x > 0.0 ? log(color.x) : -10.0, 
						color.y > 0.0 ? log(color.y) : -10.0, 
						color.z > 0.0 ? log(color.z) : -10.0); // Guard against nan
		}

		//note: see also http://www.decarpentier.nl/2d-catmull-rom-in-4-samples.
		vec4 sampleLevel0(sampler2D tex, vec2 uv)
		{
		    return texture(tex, uv, -10.0);
		}

		
		// note: entirely stolen from https://gist.github.com/TheRealMJP/c83b8c0f46b63f3a88a5986f4fa982b1
		//
		// Samples a texture with Catmull-Rom filtering, using 9 texture fetches instead of 16.
		// See http://vec3.ca/bicubic-filtering-in-fewer-taps/ for more details
		vec4 SampleTextureCatmullRom(sampler2D tex, vec2 uv, vec2 texSize)
		{
		    // We're going to sample a a 4x4 grid of texels surrounding the target UV coordinate. We'll do this by rounding
		    // down the sample location to get the exact center of our "starting" texel. The starting texel will be at
		    // location [1, 1] in the grid, where [0, 0] is the top left corner.
		    vec2 samplePos = uv * texSize;
		    vec2 texPos1 = floor(samplePos - 0.5) + 0.5;
		
		    // Compute the fractional offset from our starting texel to our original sample location, which we'll
		    // feed into the Catmull-Rom spline function to get our filter weights.
		    vec2 f = samplePos - texPos1;
		
		    // Compute the Catmull-Rom weights using the fractional offset that we calculated earlier.
		    // These equations are pre-expanded based on our knowledge of where the texels will be located,
		    // which lets us avoid having to evaluate a piece-wise function.
		    vec2 w0 = f * ( -0.5 + f * (1.0 - 0.5*f));
		    vec2 w1 = 1.0 + f * f * (-2.5 + 1.5*f);
		    vec2 w2 = f * ( 0.5 + f * (2.0 - 1.5*f) );
		    vec2 w3 = f * f * (-0.5 + 0.5 * f);
		    
		    // Work out weighting factors and sampling offsets that will let us use bilinear filtering to
		    // simultaneously evaluate the middle 2 samples from the 4x4 grid.
		    vec2 w12 = w1 + w2;
		    vec2 offset12 = w2 / w12;
		
		    // Compute the final UV coordinates we'll use for sampling the texture
		    vec2 texPos0 = texPos1 - vec2(1.0);
		    vec2 texPos3 = texPos1 + vec2(2.0);
		    vec2 texPos12 = texPos1 + offset12;
		
		    texPos0 /= texSize;
		    texPos3 /= texSize;
		    texPos12 /= texSize;
		
		    vec4 result = vec4(0.0);
		    result += sampleLevel0(tex, vec2(texPos0.x,  texPos0.y)) * w0.x * w0.y;
		    result += sampleLevel0(tex, vec2(texPos12.x, texPos0.y)) * w12.x * w0.y;
		    result += sampleLevel0(tex, vec2(texPos3.x,  texPos0.y)) * w3.x * w0.y;
								    
		    result += sampleLevel0(tex, vec2(texPos0.x,  texPos12.y)) * w0.x * w12.y;
		    result += sampleLevel0(tex, vec2(texPos12.x, texPos12.y)) * w12.x * w12.y;
		    result += sampleLevel0(tex, vec2(texPos3.x,  texPos12.y)) * w3.x * w12.y;
								    
		    result += sampleLevel0(tex, vec2(texPos0.x,  texPos3.y)) * w0.x * w3.y;
		    result += sampleLevel0(tex, vec2(texPos12.x, texPos3.y)) * w12.x * w3.y;
		    result += sampleLevel0(tex, vec2(texPos3.x,  texPos3.y)) * w3.x * w3.y;
		
		    return result;
		}


		vec2 GetVelocity(ivec2 pixelPos) {
			float closestDepth = 100.0;
			ivec2 closestUVOffset;
			for(int j = -_ClampRadius; j <= _ClampRadius; ++j) {
			    for(int i = -_ClampRadius; i <= _ClampRadius; ++i) {
			         float neighborDepth = texelFetch(_GDepth, pixelPos + ivec2(i, j), 0).x;
			         if(neighborDepth < closestDepth)
			         {
			             closestUVOffset = ivec2(i, j);
			             closestDepth = neighborDepth;
			         }
			    }
			}
			return texelFetch(_GVelocity, pixelPos + closestUVOffset, 0).xy;
		}

		// Source Unknown, Was passed around amongst some friends, apperantly its a better color clamp
		vec3 NeighborClip(vec3 c, vec3 cmin, vec3 cmax){
			vec3 p = 0.5 * (cmax + cmin);
		
			vec3 v = c - vec3(p);
			vec3 a = abs(v.xyz / (0.5 * (cmax - cmin) + 0.00000001));
			float ma = max(a.x, max(a.y, a.z));
		
			return ma > 1.0 ? vec3(p) + v / ma : c;
		}

		void main()
		{
			vec2 TexCoords = gl_FragCoord.xy / _Resolution.xy;

			vec2 pixel_size = vec2(1.0) / _Resolution;
			ivec2 pixelPos = ivec2(gl_FragCoord.xy);
			
			vec3 curr = AdjustHDRColor(texelFetch(_GColor, pixelPos, 0).xyz);
			vec3 nmin = curr;
			vec3 nmax = curr;   
			for (int x=-_ClampRadius; x<= _ClampRadius; x++) {
				for (int y=-_ClampRadius; y<= _ClampRadius; y++) {
					if(x == 0 && y == 0) continue;
					vec3 neighbor = AdjustHDRColor(texelFetch(_GColor, pixelPos + ivec2(x, y), 0).xyz);
					nmin = min(nmin, neighbor);
					nmax = max(nmax, neighbor);
				}
			}
			
			//vec2 velocity = texelFetch(_GVelocity, pixelPos, 0).xy;
			// Inflate Velocity Edge
			vec2 velocity = GetVelocity(pixelPos);

			vec2 histUv = TexCoords + velocity;
			
			// sample from history buffer, with neighbourhood clamping.  
			vec3 histSample = AdjustHDRColor(SampleTextureCatmullRom(_GHistory, histUv, _Resolution).xyz);
			//histSample = clamp(histSample, nmin, nmax);
			histSample = NeighborClip(histSample, nmin, nmax); // I think this method is better?
			
			// blend factor
			// 16 frames of jitter so match that for accumulation
			float blend = 1.0 / (16.0); 
			
			bvec2 a = greaterThan(histUv, vec2(1.0, 1.0));
			bvec2 b = lessThan(histUv, vec2(0.0, 0.0));
			// if history sample is outside screen, switch to aliased image as a fallback.
			blend = any(bvec2(any(a), any(b))) ? 1.0 : blend;
			
			vec3 curSample = curr;
			// finally, blend current and clamped history sample.
			OutputColor = vec4(mix(histSample, curSample, vec3(blend)), 1.0);

			OutputColor.rgb = exp(OutputColor.rgb); // Undo log transformation
		}

	}
}