using KorpiEngine.Core;
using KorpiEngine.Core.InputManagement;
using KorpiEngine.Core.Scripting;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Sandbox;

internal class PlayerController : Behaviour, IDamageable
{
    protected override void OnUpdate()
    {
        if (Input.KeyboardState.IsKeyDown(Keys.W))
            Move(new Vector3(0f, 1f, 0f));
            
        if (Input.KeyboardState.IsKeyDown(Keys.A))
            Move(new Vector3(-1f, 0f, 0f));
            
        if (Input.KeyboardState.IsKeyDown(Keys.S))
            Move(new Vector3(0f, -1f, 0f));
            
        if (Input.KeyboardState.IsKeyDown(Keys.D))
            Move(new Vector3(1f, 0f, 0f));
        
    }
    
    
    private void Move(Vector3 direction)
    {
        Entity.Transform.Translate(direction * Time.DeltaTime);
        Console.WriteLine($"Player moved to: {Entity.Transform.Position:F2}");
    }


    public void TakeDamage(int amount)
    {
        Console.WriteLine($"Player took {amount} damage!");
    }
    
    
    protected override void OnAwake()
    {
        Console.WriteLine("PlayerController.OnAwake()");
    }


    protected override void OnEnable()
    {
        Console.WriteLine("PlayerController.OnEnable()");
    }


    protected override void OnStart()
    {
        Console.WriteLine("PlayerController.OnStart()");
    }


    protected override void OnFixedUpdate()
    {
        
    }


    protected override void OnLateUpdate()
    {
        
    }


    protected override void OnDisable()
    {
        Console.WriteLine("PlayerController.OnDisable()");
    }


    protected override void OnDestroy()
    {
        Console.WriteLine("PlayerController.OnDestroy()");
    }
}