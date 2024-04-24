using KorpiEngine.Core;
using KorpiEngine.Core.Windowing;
using OpenTK.Mathematics;

namespace Sandbox;

internal static class Program
{
    private static void Main(string[] args)
    {
        Application.Run(
            new WindowingSettings(new Vector2i(1280, 720), "KorpiEngine Sandbox"),
            new CustomScene());
    }
}