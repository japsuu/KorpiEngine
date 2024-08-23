using KorpiEngine.Core;
using KorpiEngine.Core.API;
using KorpiEngine.Core.Windowing;
using Sandbox.Scenes.SponzaExample;

namespace Sandbox;

internal static class Program
{
    private static void Main(string[] args)
    {
        Application.Run(
            new WindowingSettings(new Vector2i(1920, 1080), "KorpiEngine Sandbox"),
            new SponzaExampleScene());
    }
}