#version 420 core

// Attributes
layout (location = 0) in vec3 a_Position;
layout (location = 1) in vec3 a_Normal;
layout (location = 2) in vec3 a_Tangent;
layout (location = 3) in vec4 a_Color;
layout (location = 4) in vec2 a_Uv0;
layout (location = 5) in vec2 a_Uv1;

// Uniforms
uniform mat4 u_ModelMatrix;
uniform mat4 u_ViewMatrix;
uniform mat4 u_ProjectionMatrix;

// Outputs
out vec2 texCoord0;

void main() {
    texCoord0 = a_Uv0;
    gl_Position = vec4(a_Position, 1.0) * u_ModelMatrix * u_ViewMatrix * u_ProjectionMatrix;
}