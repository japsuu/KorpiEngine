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
        var pbrDeferredNode = new PBRDeferredNode();
        var postPbrDeferredNode = new PostPBRDeferredNode();
        var proceduralSkyboxNode = new ProceduralSkyboxNode();
        //var screenSpaceReflectionNode = new ScreenSpaceReflectionNode();
        var depthOfFieldNode = new DepthOfFieldNode();
        var bloomNode = new BloomNode();
        var toneMappingNode = new TonemappingNode();
        var taaNode = new TAANode();
        
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