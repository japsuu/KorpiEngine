#version 420 core

// Inputs
in vec2 texCoord0;

// Uniforms
uniform vec4 u_Color;
layout (binding = 0) uniform sampler2D u_MainTexture;

// Outputs
out vec4 frag;

void main() {
    vec3 textureColor = texture(u_MainTexture, texCoord0).xyz;
    frag = /*textureColor * */u_Color;
}
