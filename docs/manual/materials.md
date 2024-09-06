
# Working With Materials

Materials are used to define the visual appearance of objects in the scene. They are composed of a shader, and a set of properties that are passed to the shader at runtime.

## Creating a Material

```csharp
// Creating a material requires a reference to the shader that will be used to render the object.
AssetRef<Shader> shaderRef = Shader.Find("Assets/Example.kshader");

// Create a new material using the shader reference, a name, and whether all textures should be set to default values (will be removed in the future).
Material material = new Material(shaderRef, "directional light material", false);

```

## Setting Material Properties

Material properties can be set using the `Material` API (see @KorpiEngine.Rendering.Materials.Material).

```csharp
material.SetTexture("_ExampleTexture", value);
material.SetVector("_ExampleVector", value);
material.SetColor("_ExampleColor", value);
material.SetFloat("_ExampleFloat", value);
```