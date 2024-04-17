using Arch.Core;
using Arch.Core.Extensions;
using KorpiEngine.Core.ECS;
using KorpiEngine.Core.SceneManagement;

namespace KorpiEngine.Core.Rendering.Cameras;

internal static class CameraFinder
{
    private static readonly QueryDescription FindMainCameraQueryDescription = new QueryDescription().WithAll<CameraComponent>();
    
    
    public static Camera? FindMainCamera()
    {
        MainCameraQueryExecutor query = new();
        Scene cameraScene = null!;
        
        foreach (Scene scene in SceneManager.CurrentlyLoadedScenes)
        {
            scene.World.InlineEntityQuery<MainCameraQueryExecutor, CameraComponent>(FindMainCameraQueryDescription, ref query);
            
            if (query.FoundCamera)
                cameraScene = scene;
        }
        
        return query.MainCamEntity == Entity.Null ?
            null :
            new Scripting.Entity(query.MainCamEntity.Reference(), cameraScene).GetComponent<Camera>();
    }
    
    
    private struct MainCameraQueryExecutor : IForEachWithEntity<CameraComponent>
    {
        private int _highestPriority;
        
        public Entity MainCamEntity;
        public bool FoundCamera;


        public MainCameraQueryExecutor()
        {
            _highestPriority = short.MinValue;
            MainCamEntity = Entity.Null;
            FoundCamera = false;
        }


        public void Update(Entity e, ref CameraComponent camera)
        {
            if (camera.RenderPriority <= _highestPriority)
            {
                FoundCamera = false;
                return;
            }
                
            _highestPriority = camera.RenderPriority;
            MainCamEntity = e;
            FoundCamera = true;
        }
    }
}