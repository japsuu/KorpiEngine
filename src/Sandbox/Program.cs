using KorpiEngine;
using KorpiEngine.AssetManagement;
using KorpiEngine.Mathematics;
using KorpiEngine.OpenGL;
using KorpiEngine.Rendering;
using Sandbox.Scenes.PrimitiveExample;

namespace Sandbox;

internal static class Program
{
    private static void Main(string[] args)
    {
        Application.Run<PrimitiveExampleScene>(
            WindowingSettings.Windowed("KorpiEngine Sandbox", new Int2(1920, 1080)),
            new UncompressedAssetProvider(),
            new GLGraphicsContext());
    }
}