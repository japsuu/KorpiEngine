using KorpiEngine.Core;
using KorpiEngine.Core.InputManagement;
using KorpiEngine.Core.Rendering;
using KorpiEngine.Core.SceneManagement;
using KorpiEngine.Core.Scripting;
using KorpiEngine.Core.Scripting.Components;
using KorpiEngine.Core.Windowing;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Sandbox;

internal static class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Hello, Korpi!");
        
        using Game game = new CustomGame(new WindowingSettings(new Vector2i(1280, 720), "KorpiEngine Sandbox"));
        
        game.Run();
    }


    private class CustomGame : Game
    {
        public CustomGame(WindowingSettings settings) : base(settings) { }


        protected override void OnLoadContent()
        {
            base.OnLoadContent();
            
            Scene scene = new CustomScene();
            SceneManager.LoadScene(scene, SceneLoadMode.Single);
        }
    }
    
    
    private class CustomScene : Scene
    {
        private Entity _blueBoxEntity = null!;  // Automatically moves and rotates
        private PlayerController _player = null!;   // Controlled by the player


        protected override void Load()
        {
            _blueBoxEntity = CreatePrimitive(PrimitiveType.Quad, "Blue Box");
            _blueBoxEntity.Transform.Position = new Vector3(0, 5, 0);
            _blueBoxEntity.GetComponent<MeshRenderer>().Material.Color = new Color(0, 0, 1, 1);

            _player = Instantiate<PlayerController>("Player");
            _player.Transform.Position = new Vector3(0, 0, 0);
        }


        protected override void Update()
        {
            UpdateBlueBox();

            if (Input.KeyboardState.IsKeyPressed(Keys.Space))
            {
                _player.Entity.GetComponent<IDamageable>().TakeDamage(10);
            }
        }


        private void UpdateBlueBox()
        {
            // Rotate the entity
            const float rotSpeedY = 0.1f;
            const float rotSpeedZ = 0.2f;
            Vector3 eulerAngles = _blueBoxEntity.Transform.EulerAngles;
            _blueBoxEntity.Transform.EulerAngles = new Vector3(eulerAngles.X, eulerAngles.Y + rotSpeedY * Time.DeltaTime, eulerAngles.Z + rotSpeedZ * Time.DeltaTime);
            
            // Move the entity
            const float moveSpeed = 0.1f;
            _blueBoxEntity.Transform.Translate(new Vector3(1f, 0f, 0f) * moveSpeed * Time.DeltaTime);
            
            Console.WriteLine($"Blue Box position: {_blueBoxEntity.Transform.Position}");
        }
    }
    
    
    private class PlayerController : Behaviour, IDamageable
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
    
    
    private interface IDamageable
    {
        public void TakeDamage(int amount);
    }
}