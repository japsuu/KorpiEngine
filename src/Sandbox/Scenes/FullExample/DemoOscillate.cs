using KorpiEngine;
using KorpiEngine.Entities;
using KorpiEngine.Mathematics;
using KorpiEngine.Utils;
using MathOps = KorpiEngine.Mathematics.MathOps;
using Random = KorpiEngine.Mathematics.SharedRandom;

namespace Sandbox.Scenes.FullExample;

/// <summary>
/// This component makes the entity oscillate up and down.
/// </summary>
internal class DemoOscillate : EntityComponent
{
    private const float OSCILLATION_SPEED = 1f;
    private const float OSCILLATION_HEIGHT = 2f;

    private double _oscillationOffset;

    
    protected override void OnStart()
    {
        // Generate a random offset in the 0-1 range to make the oscillation unique for each entity
        _oscillationOffset = Random.Range(0f, 1f);
    }


    protected override void OnUpdate()
    {
        // Oscillate the entity up and down
        double time = Time.TotalTime + _oscillationOffset;
        float height = (float)MathOps.Sin(time * OSCILLATION_SPEED) * OSCILLATION_HEIGHT;
        Transform.Position = new Vector3(Transform.Position.X, height, Transform.Position.Z);
    }
}