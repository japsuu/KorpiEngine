using KorpiEngine.Core.API.Rendering.Shaders;

namespace KorpiEngine.Core.API.AssetManagement;

[AssetImporter(".shader")]
public class ShaderImporter : AssetImporter
{
    public override EngineObject? Import(FileInfo assetPath) =>

        // Load the shader from the file
        new Shader();
}