namespace Sandbox;

internal class TestPlayer : Behaviour, IDamageable
{
    public void TakeDamage(int amount)
    {
        Console.WriteLine($"Player took {amount} damage!");
    }
    
    
    protected override void OnAwake()
    {
        Console.WriteLine("PlayerController.OnAwake()");
    }


    protected override void OnEnable()
    {
        Console.WriteLine("PlayerController.OnEnable()");
    }


    protected override void OnStart()
    {
        Console.WriteLine("PlayerController.OnStart()");
    }


    protected override void OnFixedUpdate()
    {
        
    }


    protected override void OnLateUpdate()
    {
        
    }


    protected override void OnDisable()
    {
        Console.WriteLine("PlayerController.OnDisable()");
    }


    protected override void OnDestroy()
    {
        Console.WriteLine("PlayerController.OnDestroy()");
    }
}