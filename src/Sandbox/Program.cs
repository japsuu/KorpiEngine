using KorpiEngine;
using KorpiEngine.Mathematics;
using KorpiEngine.Rendering;
using Sandbox.Scenes.SponzaExample;

namespace Sandbox;

internal static class Program
{
    private static void Main(string[] args)
    {
        Application.Run(
            new WindowingSettings(new Int2(1920, 1080), "KorpiEngine Sandbox"),
            new SponzaExampleScene());
    }
}