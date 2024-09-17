using KorpiEngine;
using KorpiEngine.Mathematics;
using KorpiEngine.Rendering;
using Sandbox.Scenes.PrimitiveExample;
using Sandbox.Scenes.SponzaExample;

namespace Sandbox;

internal static class Program
{
    private static void Main(string[] args)
    {
        Application.Run<PrimitiveExampleScene>(
            new WindowingSettings(new Int2(1920, 1080), "KorpiEngine Sandbox"));
    }
}