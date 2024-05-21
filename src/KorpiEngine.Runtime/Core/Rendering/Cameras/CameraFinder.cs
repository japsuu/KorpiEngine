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
        
#warning Find a better way that does not require creating a new Entity every time. Maybe loop the BehaviourComponents and check if they are a CameraComponent?
        return query.MainCamEntity == Entity.Null ?
            null :
            Scripting.Entity.Wrap(query.MainCamEntity.Reference(), cameraScene).GetComponent<Camera>();
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