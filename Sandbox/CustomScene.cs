using KorpiEngine.Core;
using KorpiEngine.Core.InputManagement;
using KorpiEngine.Core.Rendering;
using KorpiEngine.Core.Rendering.Materials;
using KorpiEngine.Core.SceneManagement;
using KorpiEngine.Core.Scripting;
using KorpiEngine.Core.Scripting.Components;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Sandbox;

internal class CustomScene : Scene
{
    private Entity _blueBoxEntity = null!;  // Automatically moves and rotates
    private PlayerController _player = null!;   // Controlled by the player


    protected override void Load()
    {
        _blueBoxEntity = CreatePrimitive(PrimitiveType.Quad, "Blue Box");
        _blueBoxEntity.Transform.Position = new Vector3(0, 5, 0);
            
        StandardMaterial3D blueMaterial = (StandardMaterial3D)_blueBoxEntity.GetComponent<MeshRenderer>()!.Material!;
        blueMaterial.Color = Color.Blue;

        _player = Instantiate<PlayerController>("Player");
        _player.Transform.Position = new Vector3(0, 0, 0);
    }


    protected override void Update()
    {
        UpdateBlueBox();

        if (Input.KeyboardState.IsKeyPressed(Keys.Space))
        {
            _player.Entity.GetComponent<IDamageable>()!.TakeDamage(10);
        }
    }


    private void UpdateBlueBox()
    {
        // Rotate the entity
        const float rotSpeedY = 15f;
        const float rotSpeedZ = 30f;
        Vector3 eulerAngles = _blueBoxEntity.Transform.EulerAngles;
        Vector3 newEulerAngles = new Vector3(eulerAngles.X, eulerAngles.Y + rotSpeedY * Time.DeltaTime, eulerAngles.Z + rotSpeedZ * Time.DeltaTime);
        _blueBoxEntity.Transform.Rotate(newEulerAngles);
            
        // Move the entity
        const float moveSpeed = 0.1f;
        _blueBoxEntity.Transform.Translate(new Vector3(1f, 0f, 0f) * moveSpeed * Time.DeltaTime);
            
        Console.WriteLine($"Blue Box position: {_blueBoxEntity.Transform.EulerAngles:F2}");
    }
}