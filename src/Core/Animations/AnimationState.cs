using KorpiEngine.Entities;

namespace KorpiEngine.Animations;

public class AnimationState
{
    public enum BlendMode
    {
        Blend,
        Additive
    }
    
    public string Name { get; set; }
    public AnimationClip Clip { get; set; }
    public bool Enabled { get; set; }
    public float Speed { get; set; } = 1.0f;
    public float Time { get; set; }
    public float Weight { get; set; } = 1.0f;
    public float MoveWeightSpeed { get; set; } = 1.0f;
    public float TargetWeight { get; set; } = 1.0f;
    
    public float Length => Clip.Duration;
    public float NormalizedTime => Time / Length;

    public WrapMode Wrap { get; set; } = WrapMode.Loop;
    public BlendMode Blend { get; set; } = BlendMode.Blend;

    public HashSet<string> MixingTransforms { get; set; } = [];

    
    public AnimationState(string name, AnimationClip clip)
    {
        Name = name;
        Clip = clip;
    }


    public Vector3? EvaluatePosition(Transform target, float time)
    {
        // If MixingTransforms has elements, ensure target is in the list, it's like an allowlist for an animation clip. Ensure target clip exists inside the list
        if (MixingTransforms.Count > 0 && !MixingTransforms.Contains(target.Entity.Name))
            return null;

        AnimationClip.AnimBone? bone = Clip.GetBone(target.Entity.Name);
        return bone?.EvaluatePositionAt(time);
    }


    public Quaternion? EvaluateRotation(Transform target, float time)
    {
        // If MixingTransforms has elements, ensure target clip exists inside the list
        if (MixingTransforms.Count > 0 && !MixingTransforms.Contains(target.Entity.Name))
            return null;

        AnimationClip.AnimBone? bone = Clip.GetBone(target.Entity.Name);
        return bone?.EvaluateRotationAt(time);
    }


    public Vector3? EvaluateScale(Transform target, float time)
    {
        // If MixingTransforms has elements, ensure target clip exists inside the list
        if (MixingTransforms.Count > 0 && !MixingTransforms.Contains(target.Entity.Name))
            return null;

        AnimationClip.AnimBone? bone = Clip.GetBone(target.Entity.Name);
        return bone?.EvaluateScaleAt(time);
    }


    public void AddMixingTransform(Transform transform, bool recursive)
    {
        MixingTransforms.Add(transform.Entity.Name);
        
        if (!recursive)
            return;
        
        foreach (Entity child in transform.Entity.Children)
            AddMixingTransform(child.Transform, true);
    }


    public void RemoveMixingTransform(Transform transform, bool recursive)
    {
        MixingTransforms.Remove(transform.Entity.Name);
        
        if (!recursive)
            return;
        
        foreach (Entity child in transform.Entity.Children)
            RemoveMixingTransform(child.Transform, true);
    }
}