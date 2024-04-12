using KorpiEngine.Core;
using KorpiEngine.Core.SceneManagement;
using KorpiEngine.Core.Windowing;

namespace Sandbox;

internal class CustomGame : Game
{
    public CustomGame(WindowingSettings settings) : base(settings) { }


    protected override void OnLoadContent()
    {
        base.OnLoadContent();
            
        Scene scene = new CustomScene();
        SceneManager.LoadScene(scene, SceneLoadMode.Single);
    }
}