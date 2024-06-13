using KorpiEngine.Core.API.Rendering;
using KorpiEngine.Core.API.Rendering.Materials;
using KorpiEngine.Core.Internal.AssetManagement;
using KorpiEngine.Core.Scripting.Components;

namespace KorpiEngine.Core.ECS;

public struct SkinnedMeshRendererComponent() : INativeComponent
{
    public AssetRef<Mesh> Mesh;
    public AssetRef<Material> Material;
    public Transform[] Bones = [];
    public System.Numerics.Matrix4x4[] BoneTransforms;
    //private Dictionary<int, Matrix4x4> prevMats = new();
}