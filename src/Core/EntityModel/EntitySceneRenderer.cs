using KorpiEngine.Core.Rendering.Cameras;

namespace KorpiEngine.Core.EntityModel;

internal sealed class EntitySceneRenderer
{
    private readonly List<Camera> _cameras = [];
    
    /// <summary>
    /// Cameras that will be rendered to the screen.
    /// </summary>
    private readonly PriorityQueue<Camera, short> _renderQueueScreen = new();
    
    /// <summary>
    /// Cameras that will be rendered to a RenderTexture.
    /// </summary>
    private readonly PriorityQueue<Camera, short> _renderQueueTexture = new();


    public void TryRegisterComponent<T>(T c)
    {
        switch (c)
        {
            case Camera camera:
                _cameras.Add(camera);
                break;
        }
    }


    public void TryUnregisterComponent<T>(T c)
    {
        switch (c)
        {
            case Camera camera:
                _cameras.Remove(camera);
                break;
        }
    }


    public void Render()
    {
        // Construct ordered render queues
        foreach (Camera c in _cameras)
        {
            if (!c.EnabledInHierarchy)
                continue;
                    
            if (c.TargetTexture.IsAvailable)
                _renderQueueTexture.Enqueue(c, c.RenderPriority);
            else
                _renderQueueScreen.Enqueue(c, c.RenderPriority);
        }
        
        // Render all cameras that target a RenderTexture
        while (_renderQueueTexture.Count > 0)
            _renderQueueTexture.Dequeue().Render();
                
        // Render all cameras that target the screen
        while (_renderQueueScreen.Count > 0)
            _renderQueueScreen.Dequeue().Render();
        
        // Clear the render queues
        _renderQueueScreen.Clear();
        _renderQueueTexture.Clear();
    }
}