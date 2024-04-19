using KorpiEngine.Core;
using KorpiEngine.Core.Logging;
using KorpiEngine.Core.Windowing;
using OpenTK.Mathematics;

namespace Sandbox;

internal static class Program
{
    private static void Main(string[] args)
    {
        using Game game = new CustomGame(new WindowingSettings(new Vector2i(1280, 720), "KorpiEngine Sandbox"));
        
        game.Run();
    }
}