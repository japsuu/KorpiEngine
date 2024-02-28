using Arch.Core;
using KorpiEngine.Core.Logging;

namespace KorpiEngine.Core.GameObjects;

/// <summary>
/// Manager for high-level <see cref="GameObject"/>s.
/// </summary>
internal class GameObjectManager
{
    internal static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(GameObjectManager));
    
    private readonly List<GameObject> _gameObjects = new();


    public void Update()
    {
        foreach (GameObject obj in _gameObjects)
        {
            obj.Update();
        }
    }


    public void FixedUpdate()
    {
        foreach (GameObject obj in _gameObjects)
        {
            obj.FixedUpdate();
        }
    }
    
    
    internal static GameObject CreateGameObject(Entity worldEntity, bool isEnabled)
    {
        GameObject gameObject = new(worldEntity, isEnabled);
        return gameObject;
    }


    internal void RegisterUpdates(GameObject obj)
    {
        _gameObjects.Add(obj);
    }


    internal void UnregisterUpdates(GameObject obj)
    {
        _gameObjects.Remove(obj);
    }
}