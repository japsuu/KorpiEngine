﻿namespace KorpiEngine.Core.EntityModel;

/// <summary>
/// A system that influences a single entity.
/// </summary>
public interface IEntitySystem
{
    public SystemUpdateStage[] UpdateStages { get; }
    public bool IsSingleton { get; }
    
    public void TryRegisterComponent<T>(T comp) where T : EntityComponent;
    public void TryUnregisterComponent<T>(T c) where T : EntityComponent;

    public void OnRegister(Entity e);
    public void OnUnregister(Entity e);

    public void Update(SystemUpdateStage stage);
}

/// <inheritdoc />
public abstract class EntitySystem<T1> : IEntitySystem
    where T1 : EntityComponent
{
    public abstract SystemUpdateStage[] UpdateStages { get; }
    public abstract bool IsSingleton { get; }


    #region System register / unregister

    public void OnRegister(Entity e)
    {
        foreach (T1 c in e.GetComponents<T1>())
            TryRegisterComponent(c);
    }
    
    
    public void OnUnregister(Entity e)
    {
        foreach (T1 c in e.GetComponents<T1>())
            TryUnregisterComponent(c);
    }

    #endregion


    #region Component register / unregister

    public void TryRegisterComponent<T>(T c) where T : EntityComponent
    {
        if (c is not T1 comp)
            return;

        RegisterComponent(comp);
    }


    public void TryUnregisterComponent<T>(T c) where T : EntityComponent
    {
        if (c is not T1 comp)
            return;
        
        UnregisterComponent(comp);
    }

    #endregion

    
    protected abstract void RegisterComponent(T1 c);
    protected abstract void UnregisterComponent(T1 c);
    
    public abstract void Update(SystemUpdateStage stage);
}

/// <inheritdoc />
public abstract class EntitySystem<T1, T2> : IEntitySystem
    where T1 : EntityComponent
    where T2 : EntityComponent
{
    public abstract SystemUpdateStage[] UpdateStages { get; }
    public abstract bool IsSingleton { get; }


    #region System register / unregister

    public void OnRegister(Entity e)
    {
        foreach (T1 c in e.GetComponents<T1>())
            TryRegisterComponent(c);
        
        foreach (T2 c in e.GetComponents<T2>())
            TryRegisterComponent(c);
    }
    
    
    public void OnUnregister(Entity e)
    {
        foreach (T1 c in e.GetComponents<T1>())
            TryUnregisterComponent(c);
        
        foreach (T2 c in e.GetComponents<T2>())
            TryUnregisterComponent(c);
    }

    #endregion


    #region Component register / unregister

    public void TryRegisterComponent<T>(T c) where T : EntityComponent
    {
        switch (c)
        {
            case T1 comp:
                RegisterComponent(comp);
                return;
            case T2 comp:
                RegisterComponent(comp);
                return;
        }
    }


    public void TryUnregisterComponent<T>(T c) where T : EntityComponent
    {
        switch (c)
        {
            case T1 comp:
                UnregisterComponent(comp);
                return;
            case T2 comp:
                UnregisterComponent(comp);
                return;
        }
    }

    #endregion

    
    protected abstract void RegisterComponent(T1 c1);
    protected abstract void UnregisterComponent(T1 c1);
    protected abstract void RegisterComponent(T2 c1);
    protected abstract void UnregisterComponent(T2 c1);
    
    public abstract void Update(SystemUpdateStage stage);
}

/// <inheritdoc />
public abstract class EntitySystem<T1, T2, T3> : IEntitySystem
    where T1 : EntityComponent
    where T2 : EntityComponent
    where T3 : EntityComponent
{
    public abstract SystemUpdateStage[] UpdateStages { get; }
    public abstract bool IsSingleton { get; }


    #region System register / unregister

    public void OnRegister(Entity e)
    {
        foreach (T1 c in e.GetComponents<T1>())
            TryRegisterComponent(c);
        
        foreach (T2 c in e.GetComponents<T2>())
            TryRegisterComponent(c);
        
        foreach (T3 c in e.GetComponents<T3>())
            TryRegisterComponent(c);
    }
    
    
    public void OnUnregister(Entity e)
    {
        foreach (T1 c in e.GetComponents<T1>())
            TryUnregisterComponent(c);
        
        foreach (T2 c in e.GetComponents<T2>())
            TryUnregisterComponent(c);
        
        foreach (T3 c in e.GetComponents<T3>())
            TryUnregisterComponent(c);
    }

    #endregion


    #region Component register / unregister

    public void TryRegisterComponent<T>(T c) where T : EntityComponent
    {
        switch (c)
        {
            case T1 comp:
                RegisterComponent(comp);
                return;
            case T2 comp:
                RegisterComponent(comp);
                return;
            case T3 comp:
                RegisterComponent(comp);
                return;
        }
    }


    public void TryUnregisterComponent<T>(T c) where T : EntityComponent
    {
        switch (c)
        {
            case T1 comp:
                UnregisterComponent(comp);
                return;
            case T2 comp:
                UnregisterComponent(comp);
                return;
            case T3 comp:
                UnregisterComponent(comp);
                return;
        }
    }

    #endregion

    
    protected abstract void RegisterComponent(T1 c);
    protected abstract void UnregisterComponent(T1 c);
    protected abstract void RegisterComponent(T2 c);
    protected abstract void UnregisterComponent(T2 c);
    protected abstract void RegisterComponent(T3 c);
    protected abstract void UnregisterComponent(T3 c);
    
    public abstract void Update(SystemUpdateStage stage);
}

/// <inheritdoc />
public abstract class EntitySystem<T1, T2, T3, T4> : IEntitySystem
    where T1 : EntityComponent
    where T2 : EntityComponent
    where T3 : EntityComponent
    where T4 : EntityComponent
{
    public abstract SystemUpdateStage[] UpdateStages { get; }
    public abstract bool IsSingleton { get; }


    #region System register / unregister

    public void OnRegister(Entity e)
    {
        foreach (T1 c in e.GetComponents<T1>())
            TryRegisterComponent(c);
        
        foreach (T2 c in e.GetComponents<T2>())
            TryRegisterComponent(c);
        
        foreach (T3 c in e.GetComponents<T3>())
            TryRegisterComponent(c);
        
        foreach (T4 c in e.GetComponents<T4>())
            TryRegisterComponent(c);
    }
    
    
    public void OnUnregister(Entity e)
    {
        foreach (T1 c in e.GetComponents<T1>())
            TryUnregisterComponent(c);
        
        foreach (T2 c in e.GetComponents<T2>())
            TryUnregisterComponent(c);
        
        foreach (T3 c in e.GetComponents<T3>())
            TryUnregisterComponent(c);
        
        foreach (T4 c in e.GetComponents<T4>())
            TryUnregisterComponent(c);
    }

    #endregion


    #region Component register / unregister

    public void TryRegisterComponent<T>(T c) where T : EntityComponent
    {
        switch (c)
        {
            case T1 comp:
                RegisterComponent(comp);
                return;
            case T2 comp:
                RegisterComponent(comp);
                return;
            case T3 comp:
                RegisterComponent(comp);
                return;
            case T4 comp:
                RegisterComponent(comp);
                return;
        }
    }


    public void TryUnregisterComponent<T>(T c) where T : EntityComponent
    {
        switch (c)
        {
            case T1 comp:
                UnregisterComponent(comp);
                return;
            case T2 comp:
                UnregisterComponent(comp);
                return;
            case T3 comp:
                UnregisterComponent(comp);
                return;
            case T4 comp:
                UnregisterComponent(comp);
                return;
        }
    }

    #endregion

    
    protected abstract void RegisterComponent(T1 c);
    protected abstract void UnregisterComponent(T1 c);
    protected abstract void RegisterComponent(T2 c);
    protected abstract void UnregisterComponent(T2 c);
    protected abstract void RegisterComponent(T3 c);
    protected abstract void UnregisterComponent(T3 c);
    protected abstract void RegisterComponent(T4 c);
    protected abstract void UnregisterComponent(T4 c);
    
    public abstract void Update(SystemUpdateStage stage);
}


public class ExampleEntitySystem : EntitySystem<ExampleEntitySystem.ExampleComponent>
{
    public class ExampleComponent : EntityComponent
    {
        public int Updates { get; set; }
        public int FixedUpdates { get; set; }
    }
    
    public override SystemUpdateStage[] UpdateStages => [SystemUpdateStage.Update, SystemUpdateStage.FixedUpdate];
    public override bool IsSingleton => false;
    
    private readonly List<ExampleComponent> _components = [];
    

    protected override void RegisterComponent(ExampleComponent c)
    {
        _components.Add(c);
    }

    protected override void UnregisterComponent(ExampleComponent c)
    {
        _components.Remove(c);
    }

    public override void Update(SystemUpdateStage stage)
    {
        if (stage == SystemUpdateStage.Update)
            foreach (ExampleComponent c1 in _components)
                c1.Updates++;
        else if (stage == SystemUpdateStage.FixedUpdate)
            foreach (ExampleComponent c1 in _components)
                c1.FixedUpdates++;
    }
}