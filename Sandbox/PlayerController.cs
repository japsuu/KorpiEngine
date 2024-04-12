using KorpiEngine.Core;
using KorpiEngine.Core.InputManagement;
using KorpiEngine.Core.Scripting;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Sandbox;

internal class PlayerController : Behaviour, IDamageable
{
    protected override void OnStart()
    {
        Console.WriteLine("Hello from PlayerController!");
    }


    protected override void OnUpdate()
    {
        if (Input.KeyboardState.IsKeyDown(Keys.W))
            Entity.Transform.Translate(new Vector3(0f, 1f, 0f) * Time.DeltaTime);
            
        if (Input.KeyboardState.IsKeyDown(Keys.A))
            Entity.Transform.Translate(new Vector3(-1f, 0f, 0f) * Time.DeltaTime);
            
        if (Input.KeyboardState.IsKeyDown(Keys.S))
            Entity.Transform.Translate(new Vector3(0f, -1f, 0f) * Time.DeltaTime);
            
        if (Input.KeyboardState.IsKeyDown(Keys.D))
            Entity.Transform.Translate(new Vector3(1f, 0f, 0f) * Time.DeltaTime);
    }


    public void TakeDamage(int amount)
    {
        Console.WriteLine($"Player took {amount} damage!");
    }
}