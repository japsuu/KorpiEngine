
# Working With Shaders

Shaders are programs that run on the GPU and are used to render graphics. In Korpi Engine they are written using a custom variant of the GLSL language, called Korpi Shader Language (KSL).

The KSL files are compiled into GLSL, so you can use any GLSL code/syntax in your shaders.

## Shader Files

Shader files are text files with the `.ksl` extension. They can be [loaded at runtime](external-assets.md) using the `AssetManager` API.

## Shader Structure

A shader file consists of the following parts:
- Shader name
- Properties block
- Pass block(s)
  - Vertex block
  - Fragment block
- Optional ShadowPass block
  - Vertex block
  - Fragment block

Here is a simple example of a shader that renders a texture on a mesh:
```glsl
// Name of the shader
Shader "Example/Texture"

// A properties block defines the inputs to the shader.
// Remember to add all your uniforms here.
Properties
{
    _Texture0("main texture", TEXTURE_2D)
}

// First render pass
Pass 0
{
    // Rasterizer state definitions
    BlendSrc SrcAlpha
    BlendDst One

    // Vertex shader code with ordinary GLSL syntax
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

    // Fragment shader code with ordinary GLSL syntax
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
```

## Shader Properties

Properties are used to define the inputs to the shader. They are declared in the `Properties` block at the beginning of the shader file. Internally, they are used to check if a specific uniform is present in the shader, and to set its value.

The following property types are supported:
- `FLOAT`
- `FLOAT2`
- `FLOAT3`
- `FLOAT4`
- `COLOR`
- `INT`
- `INT2`
- `INT3`
- `INT4`
- `TEXTURE_2D`
- `MATRIX_4X4`
- `MATRIX_4X4_ARRAY`

### Property Defaults

Property default values are not yet supported. You can follow the progress of this feature in [issue #33](