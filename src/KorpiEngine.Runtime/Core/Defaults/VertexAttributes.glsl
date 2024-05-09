#ifndef SHADER_VERTEXATTRIBUTES
#define SHADER_VERTEXATTRIBUTES
layout (location = 0) in vec3 vertexPosition;

#ifdef HAS_UV
layout (location = 1) in vec2 vertexTexCoord0;
#else
vec2 vertexTexCoord0 = vec2(0.0, 0.0);
#endif

#ifdef HAS_UV2
layout (location = 2) in vec2 vertexTexCoord1;
#else
vec2 vertexTexCoord1 = vec2(0.0, 0.0);
#endif

#ifdef HAS_NORMALS
layout (location = 3) in vec3 vertexNormal;
#else
vec3 vertexNormal = vec3(0.0, 1.0, 0.0);
#endif

#ifdef HAS_COLORS
layout (location = 4) in vec4 vertexColor;
#else
vec4 vertexColor = vec4(1.0, 1.0, 1.0, 1.0);
#endif

#ifdef HAS_TANGENTS
layout (location = 5) in vec3 vertexTangent;
#else
vec3 vertexTangent = vec3(1.0, 0.0, 0.0);
#endif

#endif