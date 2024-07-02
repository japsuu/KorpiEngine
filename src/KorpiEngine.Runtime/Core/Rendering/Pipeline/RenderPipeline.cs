namespace KorpiEngine.Core.Rendering.Pipeline;

public class RenderPipeline
{
    public readonly List<RenderTexture> UsedRenderTextures = [];
    public int Width { get; private set; }
    public int Height { get; private set; }

    private readonly RenderPassNode _rootNode;


    public RenderPipeline()
    {
        // Create nodes, setup connections, and add to list.
        PBRDeferredNode pbrDeferredNode = new();
        PostPBRDeferredNode postPbrDeferredNode = new();
        ProceduralSkyboxNode proceduralSkyboxNode = new();
        //var screenSpaceReflectionNode = new ScreenSpaceReflectionNode();
        DepthOfFieldNode depthOfFieldNode = new();
        BloomNode bloomNode = new();
        TonemappingNode toneMappingNode = new();
        TAANode taaNode = new();
        
        pbrDeferredNode.SetChild(postPbrDeferredNode);
        postPbrDeferredNode.SetChild(proceduralSkyboxNode);
        proceduralSkyboxNode.SetChild(taaNode);
        taaNode.SetChild(depthOfFieldNode);
        depthOfFieldNode.SetChild(bloomNode);
        bloomNode.SetChild(toneMappingNode);
        
        _rootNode = pbrDeferredNode;
    }
    
    
    public void Prepare(int width, int height)
    {
        Width = width;
        Height = height;

        _rootNode.Prepare(this);
    }

    
    public RenderTexture? Render()
    {
        RenderTexture? result = null;
        try
        {
            result = _rootNode.Evaluate(null);
        }
        catch (Exception e)
        {
            Application.Logger.Error($"[RenderPipeline] {e.Message}{Environment.NewLine}{e.StackTrace}");
        }

        foreach (RenderTexture rt in UsedRenderTextures)
            RenderTexture.ReleaseTemporaryRT(rt);
        
        UsedRenderTextures.Clear();
        
        return result;
    }
}