using KorpiEngine.Core.EntityModel.Components;
using KorpiEngine.Core.Rendering;
using KorpiEngine.Core.Rendering.Cameras;

namespace KorpiEngine.Core.EntityModel.Systems.World;

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


    public override void Update(SystemUpdateStage stage)
    {
        switch (stage)
        {
            case SystemUpdateStage.PreRender:
            {
                // Construct ordered render queues
                foreach (CameraComponent c in _cameras)
                {
                    if (c.RenderTarget == CameraRenderTarget.Screen)
                        _renderQueueScreen.Enqueue(c, c.RenderPriority);
                    else
                        _renderQueueTexture.Enqueue(c, c.RenderPriority);
                }
                break;
            }
            case SystemUpdateStage.Render:
            {
                // Render all cameras that target a RenderTexture
                while (_renderQueueTexture.Count > 0)
                    RenderToTargetTexture(_renderQueueTexture.Dequeue());
                
                // Render all cameras that target the screen
                while (_renderQueueScreen.Count > 0)
                    RenderToScreen(_renderQueueScreen.Dequeue());
                
                Graphics.SetRenderingCamera(null);

                break;
            }
            case SystemUpdateStage.PostRender:
            {
                // Clear the render queues
                _renderQueueScreen.Clear();
                _renderQueueTexture.Clear();
                break;
            }
        }
    }


    private void RenderToScreen(CameraComponent camera)
    {
        // Clear the screen
        if (camera.ClearType == CameraClearType.SolidColor)
        {
            camera.ClearColor.Deconstruct(out float r, out float g, out float b, out float a);
            Graphics.Clear(r, g, b, a);
        }
            
        // Use the current view and projection matrices
        Graphics.SetRenderingCamera(camera);
        
        // Render all meshes
        foreach (MeshRendererComponent renderer in _meshRenderers)
        {
            if (renderer.Material == null)
                continue;
            
            // Render the mesh
            renderer.Render();
        }
    }


    private void RenderToTargetTexture(CameraComponent camera)
    {
        throw new NotImplementedException("RenderTexture rendering is not implemented yet.");
    }
}