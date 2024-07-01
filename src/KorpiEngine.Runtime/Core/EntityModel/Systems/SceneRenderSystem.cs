using KorpiEngine.Core.EntityModel.Components;
using KorpiEngine.Core.Rendering;
using KorpiEngine.Core.Rendering.Cameras;

namespace KorpiEngine.Core.EntityModel.Systems;

public class SceneRenderSystem : SceneSystem
{
    private readonly List<CameraComponent> _cameras = [];
    private readonly List<MeshRendererComponent> _meshRenderers = [];
    
    /// <summary>
    /// Cameras that will be rendered to the screen.
    /// </summary>
    private readonly PriorityQueue<CameraComponent, short> _renderQueueScreen = new();
    
    /// <summary>
    /// Cameras that will be rendered to a RenderTexture.
    /// </summary>
    private readonly PriorityQueue<CameraComponent, short> _renderQueueTexture = new();
    
    
    public override void TryRegisterComponent<T>(T c)
    {
        switch (c)
        {
            case CameraComponent camera:
                _cameras.Add(camera);
                break;
            case MeshRendererComponent meshRenderer:
                _meshRenderers.Add(meshRenderer);
                break;
        }
    }


    public override void TryUnregisterComponent<T>(T c)
    {
        switch (c)
        {
            case CameraComponent camera:
                _cameras.Remove(camera);
                break;
            case MeshRendererComponent meshRenderer:
                _meshRenderers.Remove(meshRenderer);
                break;
        }
    }


    public override void Update(EntityUpdateStage stage)
    {
        switch (stage)
        {
            case EntityUpdateStage.PreRender:
            {
                // Construct ordered render queues
                foreach (CameraComponent c in _cameras)
                {
                    if (!c.EnabledInHierarchy)
                        continue;
                    
                    if (c.TargetTexture.IsAvailable)
                        _renderQueueTexture.Enqueue(c, c.RenderPriority);
                    else
                        _renderQueueScreen.Enqueue(c, c.RenderPriority);
                }
                break;
            }
            case EntityUpdateStage.Render:
            {
                // Render all cameras that target a RenderTexture
                while (_renderQueueTexture.Count > 0)
                    Render(_renderQueueTexture.Dequeue());
                
                // Render all cameras that target the screen
                while (_renderQueueScreen.Count > 0)
                    Render(_renderQueueScreen.Dequeue());
                
                break;
            }
            case EntityUpdateStage.PostRender:
            {
                // Clear the render queues
                _renderQueueScreen.Clear();
                _renderQueueTexture.Clear();
                break;
            }
        }
    }


    private void Render(CameraComponent camera)
    {
        // Use the current view and projection matrices
        Graphics.SetRenderingCamera(camera);
        
        // Clear the screen
        if (camera.ClearType == CameraClearType.SolidColor)
        {
            camera.ClearColor.Deconstruct(out float r, out float g, out float b, out float a);
            bool clearColor = camera.ClearFlags.HasFlag(CameraClearFlags.Color);
            bool clearDepth = camera.ClearFlags.HasFlag(CameraClearFlags.Depth);
            bool clearStencil = camera.ClearFlags.HasFlag(CameraClearFlags.Stencil);
            
            Graphics.Clear(r, g, b, a, clearColor, clearDepth, clearStencil);
        }
        
        // Render all meshes
        foreach (MeshRendererComponent renderer in _meshRenderers)
        {
            if (renderer.Material == null)
                continue;
            
            // Render the mesh
            renderer.Render();
        }
        
        Graphics.SetRenderingCamera(null);
    }
}