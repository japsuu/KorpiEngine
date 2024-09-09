using KorpiEngine.AssetManagement;
using KorpiEngine.Entities;
using KorpiEngine.Mathematics;
using KorpiEngine.Utils;

namespace KorpiEngine.Animations;

// Taken and modified from Prowl's Animation.cs
// https://github.com/ProwlEngine/Prowl/blob/main/Prowl.Runtime/Components/Animation.cs.

public class Animation : EntityComponent
{
    public List<AssetRef<AnimationClip>> Clips { get; set; } = [];
    public AssetRef<AnimationClip> DefaultClip { get; set; }
    public bool PlayAutomatically { get; set; } = true;
    public float Speed { get; set; } = 1.0f;

    private readonly List<AnimationState> _states = [];
    private readonly Dictionary<string, AnimationState> _stateDictionary = new();
    private readonly List<Transform> _transforms = [];


    protected override void OnEnable()
    {
        // Assign DefaultClip to the first clip if it's not set
        if (!DefaultClip.IsAvailable && Clips.Count > 0)
            DefaultClip = Clips[0];

        foreach (AssetRef<AnimationClip> clip in Clips)
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
            ProcessBlendStates(blendNormalizer, transform, ref position, ref rotation, ref scale);

        // Process Additive states
        ProcessAdditiveStates(transform, ref position, ref rotation, ref scale);

        transform.LocalPosition = position;
        transform.LocalRotation = rotation;
        transform.LocalScale = scale;
    }


    private void ProcessBlendStates(double blendNormalizer, Transform transform, ref Vector3 position, ref Quaternion rotation, ref Vector3 scale)
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
                rotation = Mathematics.MathOps.Slerp(rotation, rot.Value, (float)normalizedWeight);

            Vector3? scl = state.EvaluateScale(transform, state.Time);
            if (scl.HasValue)
                scale = Mathematics.MathOps.Lerp(scale, scl.Value, (float)normalizedWeight);
        }
    }


    private void ProcessAdditiveStates(Transform transform, ref Vector3 position, ref Quaternion rotation, ref Vector3 scale)
    {
        foreach (AnimationState state in _states.Where(s => s.Enabled && s.Blend == AnimationState.BlendMode.Additive))
        {
            Vector3? pos = state.EvaluatePosition(transform, state.Time);
            if (pos.HasValue)
                position += pos.Value * (float)state.Weight;

            Quaternion? rot = state.EvaluateRotation(transform, state.Time);
            if (rot.HasValue)
                rotation *= Mathematics.MathOps.Slerp(Quaternion.Identity, rot.Value, (float)state.Weight);

            Vector3? scl = state.EvaluateScale(transform, state.Time);
            if (scl.HasValue)
                scale = Mathematics.MathOps.Lerp(scale, scale * scl.Value, (float)state.Weight);
        }
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
        state.Weight = state.Weight.MoveTowards(state.TargetWeight, state.MoveWeightSpeed * Time.DeltaTime);
    }


    public void Blend(string clipName, float targetWeight, float fadeLength = 0.3f)
    {
        if (_stateDictionary.TryGetValue(clipName, out AnimationState? state))
        {
            state.TargetWeight = targetWeight;
            state.MoveWeightSpeed = 1.0f / fadeLength;
        }
    }


    public void CrossFade(string clipName, float fadeLength = 0.3f)
    {
        // Set all target weights to 0, and assign movespeed according to fadeLength
        foreach (AnimationState state in _states)
        {
            state.TargetWeight = state.Name.Equals(clipName, StringComparison.OrdinalIgnoreCase) ? 1.0f : 0.0f;
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