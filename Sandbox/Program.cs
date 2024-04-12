using KorpiEngine.Core;
using KorpiEngine.Core.Windowing;
using OpenTK.Mathematics;

namespace Sandbox;

internal static class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Hello, Korpi!");
        
        using Game game = new CustomGame(new WindowingSettings(new Vector2i(1280, 720), "KorpiEngine Sandbox"));
        
        game.Run();
    }
}