namespace KorpiEngine.Core.Entities.Components;

public abstract class EntityComponent
{
    public bool IsEnabled { get; private set; }
    
    protected Entity Entity { get; private set; } = null!;
    
    
    protected EntityComponent() { }


    public void SetEntity(Entity entity)
    {
        Entity = entity;
    }


    public void Enable()
    {
        IsEnabled = true;
        OnEnable();
    }
    
    
    public void Update()
    {
        OnUpdate();
    }


    public void FixedUpdate()
    {
        OnFixedUpdate();
    }
    
    
    public void Draw()
    {
        OnDraw();
    }
    
    
    public void Disable()
    {
        IsEnabled = false;
        OnDisable();
    }
    
    
    public void Destroy()
    {
        OnDestroy();
    }


    protected virtual void OnEnable() { }
    
    protected virtual void OnUpdate() { }
    
    protected virtual void OnFixedUpdate() { }
    
    protected virtual void OnDraw() { }
    
    protected virtual void OnDisable() { }
    
    protected virtual void OnDestroy() { }
}