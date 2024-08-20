using KorpiEngine.Core.EntityModel;
using KorpiEngine.Core.EntityModel.SpatialHierarchy;
using KorpiEngine.Core.Internal.AssetManagement;

namespace KorpiEngine.Core.API;

// Taken and modified from Prowl's Animation.cs
// https://github.com/ProwlEngine/Prowl/blob/main/Prowl.Runtime/Components/Animation.cs.

public class Animation : EntityComponent
{
    public List<ResourceRef<AnimationClip>> Clips { get; set; } = [];
    public ResourceRef<AnimationClip> DefaultClip { get; set; }
    public bool PlayAutomatically { get; set; } = true;
    public double Speed { get; set; } = 1.0;

    private readonly List<AnimationState> _states = [];
    private readonly Dictionary<string, AnimationState> _stateDictionary = new();
    private readonly List<Transform> _transforms = [];


    protected override void OnEnable()
    {
        // Assign DefaultClip to the first clip if it's not set
        if (!DefaultClip.IsAvailable && Clips.Count > 0)
            DefaultClip = Clips[0];

        foreach (ResourceRef<AnimationClip> clip in Clips)
            if (clip.IsAvailable)
                AddClip(clip.Res!);
        if (DefaultClip.IsAvailable)
        {
            AddClip(DefaultClip.Res!);
            if (PlayAutomatically)
                Play(DefaultClip.Res!.Name);
        }
    }


    protected override void OnUpdate()
    {
        foreach (AnimationState state in _states)
            UpdateTime(state);

        if (_states.Where(s => s.Enabled).Sum(s => s.Weight) <= 0)
            // Either all disabled or all weights are zero
            return;

        // Normalize weights for Blend states
        double totalBlendWeight = _states.Where(s => s.Enabled && s.Blend == AnimationState.BlendMode.Blend).Sum(s => s.Weight);
        double blendNormalizer = totalBlendWeight > 0 ? 1.0 / totalBlendWeight : 0;

        // Update all transforms
        foreach (Transform transform in _transforms)
            UpdateTransform(blendNormalizer, transform);
    }


    private void UpdateTransform(double blendNormalizer, Transform transform)
    {
        Vector3 position = Vector3.Zero;
        Quaternion rotation = Quaternion.Identity;
        Vector3 scale = Vector3.One;

        if (blendNormalizer > 0)
        {
            // Process Blend states
            foreach (AnimationState state in _states.Where(s => s.Enabled && s.Blend == AnimationState.BlendMode.Blend))
            {
                double normalizedWeight = state.Weight * blendNormalizer;

                Vector3? pos = state.EvaluatePosition(transform, state.Time);
                if (pos.HasValue)
                    position += pos.Value * (float)normalizedWeight;

                Quaternion? rot = state.EvaluateRotation(transform, state.Time);
                if (rot.HasValue)
                    rotation = Quaternion.Slerp(rotation, rot.Value, (float)normalizedWeight);

                Vector3? scl = state.EvaluateScale(transform, state.Time);
                if (scl.HasValue)
                    scale = Vector3.Lerp(scale, scl.Value, (float)normalizedWeight);
            }
        }

        // Process Additive states
        foreach (AnimationState state in _states.Where(s => s.Enabled && s.Blend == AnimationState.BlendMode.Additive))
        {
            Vector3? pos = state.EvaluatePosition(transform, state.Time);
            if (pos.HasValue)
                position += pos.Value * (float)state.Weight;

            Quaternion? rot = state.EvaluateRotation(transform, state.Time);
            if (rot.HasValue)
                rotation *= Quaternion.Slerp(Quaternion.Identity, rot.Value, (float)state.Weight);

            Vector3? scl = state.EvaluateScale(transform, state.Time);
            if (scl.HasValue)
                scale = Vector3.Lerp(scale, scale * scl.Value, (float)state.Weight);
        }

        transform.LocalPosition = position;
        transform.LocalRotation = rotation;
        transform.LocalScale = scale;
    }


    private void UpdateTime(AnimationState state)
    {
        if (state.Enabled)
        {
            state.Time += state.Speed * Speed * Time.DeltaTime;

            if (state.Time >= state.Length)
            {
                switch (state.Wrap)
                {
                    case WrapMode.Loop:
                        state.Time = 0.0f;
                        break;
                    case WrapMode.PingPong:
                        state.Speed = -state.Speed;
                        break;
                    case WrapMode.ClampForever:
                        state.Time = state.Length;
                        break;
                    case WrapMode.Once:
                        state.Time = 0;
                        state.Enabled = false;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(state), state.Wrap, null);
                }
            }
        }

        // Weight always update even if the state is disabled
        state.Weight = Mathd.MoveTowards(state.Weight, state.TargetWeight, state.MoveWeightSpeed * Time.DeltaTime);
    }


    public void Blend(string clipName, double targetWeight, double fadeLength = 0.3f)
    {
        if (_stateDictionary.TryGetValue(clipName, out AnimationState? state))
        {
            state.TargetWeight = targetWeight;
            state.MoveWeightSpeed = 1.0f / fadeLength;
        }
    }


    public void CrossFade(string clipName, double fadeLength = 0.3f)
    {
        // Set all target weights to 0, and assign movespeed according to fadeLength
        foreach (AnimationState state in _states)
        {
            state.TargetWeight = state.Name.Equals(clipName, StringComparison.OrdinalIgnoreCase) ? 1.0 : 0.0;
            state.MoveWeightSpeed = 1.0f / fadeLength;
        }
    }


    public void Play(string stateName)
    {
        if (_stateDictionary.TryGetValue(stateName, out AnimationState? state))
        {
            state.Enabled = true;
            state.Time = 0.0f;
        }
    }


    public void Stop(string stateName)
    {
        if (_stateDictionary.TryGetValue(stateName, out AnimationState? state))
        {
            state.Enabled = false;
            state.Time = 0.0f;
        }
    }


    public void StopAll()
    {
        foreach (AnimationState state in _states)
        {
            state.Enabled = false;
            state.Time = 0.0f;
        }
    }


    public void AddClip(AnimationClip clip)
    {
        if (_stateDictionary.ContainsKey(clip.Name))
            return;
        _states.Add(new AnimationState(clip.Name, clip));
        _stateDictionary[clip.Name] = _states[^1];

        // Find all bone names used by the clip
        foreach (AnimationClip.AnimBone bone in clip.Bones)
        {
            Transform? t = Entity.Transform.DeepFind(bone.BoneName);
            if (t == null)
                continue;
            if (!_transforms.Contains(t))
                _transforms.Add(t);
        }
    }


    public void RemoveClip(string stateName)
    {
        if (!_stateDictionary.TryGetValue(stateName, out AnimationState? state))
            return;
        
        _states.Remove(state);
        _stateDictionary.Remove(stateName);
    }
}

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
    public double Speed { get; set; } = 1.0;
    public double Time { get; set; } = 0;
    public double Weight { get; set; } = 1.0;
    public double MoveWeightSpeed { get; set; } = 1.0;
    public double TargetWeight { get; set; } = 1.0;
    
    public double Length => Clip.Duration;
    public double NormalizedTime => Time / Length;

    public WrapMode Wrap { get; set; } = WrapMode.Loop;
    public BlendMode Blend { get; set; } = BlendMode.Blend;

    public HashSet<string> MixingTransforms { get; set; } = [];

    
    public AnimationState(string name, AnimationClip clip)
    {
        Name = name;
        Clip = clip;
    }


    public Vector3? EvaluatePosition(Transform target, double time)
    {
        // If MixingTransforms has elements, ensure target is in the list, its like a Whitelist for an animation clip
        if (MixingTransforms.Count > 0)

            // Ensure Target exists inside MixingTransforms
            if (!MixingTransforms.Contains(target.Entity.Name))
                return null;

        AnimationClip.AnimBone? bone = Clip.GetBone(target.Entity.Name);
        return bone?.EvaluatePositionAt(time);
    }


    public Quaternion? EvaluateRotation(Transform target, double time)
    {
        // If MixingTransforms has elements, ensure target clip exists inside the list
        if (MixingTransforms.Count > 0 && !MixingTransforms.Contains(target.Entity.Name))
            return null;

        AnimationClip.AnimBone? bone = Clip.GetBone(target.Entity.Name);
        return bone?.EvaluateRotationAt(time);
    }


    public Vector3? EvaluateScale(Transform target, double time)
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