using KorpiEngine.Core.EntityModel.SpatialHierarchy;

namespace KorpiEngine.Core.EntityModel.Components;

public class BehaviourComponent : SpatialEntityComponent
{
    internal bool HasBeenStarted;
    
    
    /// <summary>
    /// Called when the object is created.
    /// </summary>
    public virtual void Awake()
    {
    }


    /// <summary>
    /// Called before the first Update.
    /// </summary>
    public virtual void Start()
    {
    }


    /// <summary>
    /// Called every frame.
    /// </summary>
    public virtual void Update()
    {
    }


    /// <summary>
    /// Called after Update.
    /// </summary>
    public virtual void LateUpdate()
    {
    }


    /// <summary>
    /// Called every fixed frame.
    /// </summary>
    public virtual void FixedUpdate()
    {
    }


    /// <summary>
    /// Called when the object is rendered.
    /// </summary>
    public virtual void OnRenderObject()
    {
    }


    /// <summary>
    /// Called when the component is destroyed.
    /// </summary>
    public virtual void OnDestroy()
    {
    }
}