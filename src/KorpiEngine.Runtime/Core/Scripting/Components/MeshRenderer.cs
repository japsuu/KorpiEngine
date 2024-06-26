﻿using KorpiEngine.Core.API.Rendering;
using KorpiEngine.Core.API.Rendering.Materials;
using KorpiEngine.Core.ECS;

namespace KorpiEngine.Core.Scripting.Components;

/// <summary>
/// Contains the mesh and material data for an <see cref="Entity"/>.
/// </summary>
public class MeshRenderer : Component
{
    internal override Type NativeComponentType => typeof(MeshRendererComponent);

    public Mesh? Mesh
    {
        get => Entity.GetNativeComponent<MeshRendererComponent>().Mesh;
        set => Entity.GetNativeComponent<MeshRendererComponent>().Mesh = value;
    }

    public Material? Material
    {
        get => Entity.GetNativeComponent<MeshRendererComponent>().Material;
        set => Entity.GetNativeComponent<MeshRendererComponent>().Material = value;
    }
}