﻿#ifndef SHADER_UTILITIES
#define SHADER_UTILITIES
// ----------------------------------------------------------------------------
vec3 projectAndDivide(mat4 matrix, vec3 pos) {
    vec4 p = matrix * vec4(pos, 1.0);
    return p.xyz / p.w;
}
// ----------------------------------------------------------------------------
vec3 getScreenPos(vec2 tc, sampler2D depthSampler) {
	return vec3(tc, texture(depthSampler, tc).x);
}
// ----------------------------------------------------------------------------
vec3 getScreenFromViewPos(vec3 viewPos) {
	vec3 p = projectAndDivide(_MatProjection, viewPos);
	return p * 0.5 + 0.5;
}
// ----------------------------------------------------------------------------
vec3 getNDCFromScreenPos(vec3 screenPos) {
	return screenPos * 2.0 - 1.0;
}
// ----------------------------------------------------------------------------
vec3 getViewFromScreenPos(vec3 screenPos) {
	return projectAndDivide(_MatProjectionInverse, getNDCFromScreenPos(screenPos));
}
// ----------------------------------------------------------------------------
vec3 getViewPos(vec2 tc, sampler2D depthSampler) {
	return getViewFromScreenPos(getScreenPos(tc, depthSampler));
}
// ----------------------------------------------------------------------------
// ----------------------------------------------------------------------------
// ----------------------------------------------------------------------------
// ----------------------------------------------------------------------------


// ----------------------------------------------------------------------------
vec3 binaryRefine(vec3 screenPosRayDir, vec3 startPos, int refineSteps, sampler2D depthSampler)
{
	for(int i = 0; i < refineSteps; i++)
	{
		screenPosRayDir *= 0.5;
		startPos += texture(depthSampler, startPos.xy).x < startPos.z ? -screenPosRayDir : screenPosRayDir;
	}
	return startPos;
}
// ----------------------------------------------------------------------------
vec3 rayTrace(vec3 screenPos, vec3 viewPos, vec3 rayDir, float dither, int steps, int refineSteps, sampler2D depthSampler) 
{
	vec3 screenPosRayDir = normalize(getScreenFromViewPos(viewPos + rayDir) - screenPos) / steps;
	screenPos += screenPosRayDir * dither;

	for(int i = 0; i < steps; i++)
	{
		screenPos += screenPosRayDir;
		if(screenPos.x <= 0 || screenPos.y <= 0 || screenPos.x >= 1 || screenPos.y >= 1) 
			return vec3(0);
		float curDepth = texture(depthSampler, screenPos.xy).x;

		if(screenPos.z > curDepth) 
		{
			if(refineSteps == 0) 
				return vec3(screenPos.xy, curDepth != 1);
			return vec3(binaryRefine(screenPosRayDir, screenPos, refineSteps, depthSampler).xy, curDepth != 1);
		}
	}
	
	return vec3(0);
}
// ----------------------------------------------------------------------------
// ----------------------------------------------------------------------------
// ----------------------------------------------------------------------------
// ----------------------------------------------------------------------------
// ----------------------------------------------------------------------------
// ----------------------------------------------------------------------------
// ----------------------------------------------------------------------------
// ----------------------------------------------------------------------------
// ----------------------------------------------------------------------------
// ----------------------------------------------------------------------------
// ----------------------------------------------------------------------------
// ----------------------------------------------------------------------------
// ----------------------------------------------------------------------------
// ----------------------------------------------------------------------------
// ----------------------------------------------------------------------------
// ----------------------------------------------------------------------------
#endif