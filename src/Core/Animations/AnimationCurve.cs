﻿using System.Collections;
using KorpiEngine.Tools.Serialization;

namespace KorpiEngine.Animations;

// MIT License - Copyright (C) The Mono.Xna Team
// Modified from: https://github.com/MonoGame/MonoGame/blob/develop/MonoGame.Framework/Curve.cs

/// <summary>
/// Defines how the <see cref="AnimationCurve"/> value is determined for position before first point or after the end point on the <see cref="AnimationCurve"/>.
/// </summary>
public enum CurveLoopType
{
    Constant,
    Cycle,
    CycleOffset,
    Oscillate,
    Linear
}

/// <summary> Defines the different tangent types to be calculated for <see cref="KeyFrame"/> points in a <see cref="AnimationCurve"/>. </summary>
public enum CurveTangent
{
    Flat,
    Linear,
    Smooth
}

public class AnimationCurve : ISerializable
{
    #region Public Properties

    /// <summary>
    /// Defines how to handle weighting values that are less than the first control point in the curve.
    /// </summary>
    public CurveLoopType PreLoop { get; set; }

    /// <summary>
    /// Defines how to handle weighting values that are greater than the last control point in the curve.
    /// </summary>
    public CurveLoopType PostLoop { get; set; }

    /// <summary>
    /// The collection of curve keys.
    /// </summary>
    public CurveKeyCollection Keys { get; private set; }

    #endregion


    #region Public Constructors

    /// <summary>
    /// Constructs a curve.
    /// </summary>
    public AnimationCurve()
    {
        Keys =
        [
            new KeyFrame(0, 0),
            new KeyFrame(1, 1)
        ];

        // Add Default keys
        SmoothTangents(CurveTangent.Smooth);
    }

    #endregion


    #region Public Methods

    /// <summary>
    /// Evaluate the value at a position of this <see cref="AnimationCurve"/>.
    /// </summary>
    /// <param name="position">The position on this <see cref="AnimationCurve"/>.</param>
    /// <returns>Value at the position on this <see cref="AnimationCurve"/>.</returns>
    public float Evaluate(float position)
    {
        if (Keys.Count == 0)
            return 0.0f;

        if (Keys.Count == 1)
            return Keys[0].Value;

        KeyFrame first = Keys[0];
        KeyFrame last = Keys[^1];

        if (position < first.Position)
        {
            return (float)HandlePreLoop(position, first, last);
        }

        return position <= last.Position ?
            (float)GetCurvePosition(position) :
            (float)HandlePostLoop(position, last, first);
    }


    private double HandlePreLoop(double position, KeyFrame first, KeyFrame last)
    {
        switch (PreLoop)
        {
            case CurveLoopType.Constant:
                // constant
                return first.Value;

            case CurveLoopType.Linear:
                // linear y = a*x +b with a tangent of last point
                return first.Value - first.TangentIn * (first.Position - position);

            case CurveLoopType.Cycle:
                // start -> end / start -> end
                int cycle = GetNumberOfCycle(position);
                double virtualPos = position - cycle * (last.Position - first.Position);
                return GetCurvePosition(virtualPos);

            case CurveLoopType.CycleOffset:
                // make the curve continue (with no step) so must up the curve each cycle of delta(value)
                cycle = GetNumberOfCycle(position);
                virtualPos = position - cycle * (last.Position - first.Position);
                return GetCurvePosition(virtualPos) + cycle * (last.Value - first.Value);

            case CurveLoopType.Oscillate:
                // go back on curve from end and target start 
                // start → end / end → start
                cycle = GetNumberOfCycle(position);
                if (Mathematics.MathOps.AlmostEquals(cycle % 2.0, 0)) //if pair
                    virtualPos = position - cycle * (last.Position - first.Position);
                else
                    virtualPos = last.Position - position + first.Position + cycle * (last.Position - first.Position);
                return GetCurvePosition(virtualPos);
            default:
                throw new InvalidOperationException("Invalid CurveLoopType.");
        }
    }


    private double HandlePostLoop(double position, KeyFrame last, KeyFrame first)
    {
        switch (PostLoop)
        {
            case CurveLoopType.Constant:
                // constant
                return last.Value;

            case CurveLoopType.Linear:
                // linear y = a*x +b with a tangent of last point
                return last.Value + first.TangentOut * (position - last.Position);

            case CurveLoopType.Cycle:
                // start -> end / start -> end
                int cycle = GetNumberOfCycle(position);
                double virtualPos = position - cycle * (last.Position - first.Position);
                return GetCurvePosition(virtualPos);

            case CurveLoopType.CycleOffset:
                // make the curve continue (with no step) so must up the curve each cycle of delta(value)
                cycle = GetNumberOfCycle(position);
                virtualPos = position - cycle * (last.Position - first.Position);
                return GetCurvePosition(virtualPos) + cycle * (last.Value - first.Value);

            case CurveLoopType.Oscillate:
                // go back on curve from end and target start 
                // start → end / end → start
                cycle = GetNumberOfCycle(position);
                if (Mathematics.MathOps.AlmostEquals(cycle % 2.0, 0)) //if pair
                    virtualPos = position - cycle * (last.Position - first.Position);
                else
                    virtualPos = last.Position - position + first.Position + cycle * (last.Position - first.Position);
                return GetCurvePosition(virtualPos);
            default:
                throw new InvalidOperationException("Invalid CurveLoopType.");
        }
    }


    /// <summary>
    /// Computes tangents for all keys in the collection.
    /// </summary>
    /// <param name="tangentType">The tangent type for both in and out.</param>
    public void SmoothTangents(CurveTangent tangentType)
    {
        SmoothTangents(tangentType, tangentType);
    }


    /// <summary>
    /// Computes tangents for all keys in the collection.
    /// </summary>
    /// <param name="tangentInType">The tangent in-type. <see cref="KeyFrame.TangentIn"/> for more details.</param>
    /// <param name="tangentOutType">The tangent out-type. <see cref="KeyFrame.TangentOut"/> for more details.</param>
    public void SmoothTangents(CurveTangent tangentInType, CurveTangent tangentOutType)
    {
        for (int i = 0; i < Keys.Count; ++i)
            SmoothTangent(i, tangentInType, tangentOutType);
    }


    /// <summary>
    /// Computes tangent for the specific key in the collection.
    /// </summary>
    /// <param name="keyIndex">The index of a key in the collection.</param>
    /// <param name="tangentType">The tangent type for both in and out.</param>
    public void SmoothTangent(int keyIndex, CurveTangent tangentType) => SmoothTangent(keyIndex, tangentType, tangentType);


    /// <summary>
    /// Computes tangent for the specific key in the collection.
    /// </summary>
    /// <param name="keyIndex">The index of key in the collection.</param>
    /// <param name="tangentInType">The tangent in-type. <see cref="KeyFrame.TangentIn"/> for more details.</param>
    /// <param name="tangentOutType">The tangent out-type. <see cref="KeyFrame.TangentOut"/> for more details.</param>
    public void SmoothTangent(int keyIndex, CurveTangent tangentInType, CurveTangent tangentOutType)
    {
        // See http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.curvetangent.aspx

        KeyFrame key = Keys[keyIndex];

        double p0,
            p,
            p1;
        p0 = p = p1 = key.Position;

        double v0,
            v,
            v1;
        v0 = v = v1 = key.Value;

        if (keyIndex > 0)
        {
            p0 = Keys[keyIndex - 1].Position;
            v0 = Keys[keyIndex - 1].Value;
        }

        if (keyIndex < Keys.Count - 1)
        {
            p1 = Keys[keyIndex + 1].Position;
            v1 = Keys[keyIndex + 1].Value;
        }

        switch (tangentInType)
        {
            case CurveTangent.Flat:
                key.TangentIn = 0;
                break;
            case CurveTangent.Linear:
                key.TangentIn = (float)(v - v0);
                break;
            case CurveTangent.Smooth:
                double pn = p1 - p0;
                if (Math.Abs(pn) < double.Epsilon)
                    key.TangentIn = 0;
                else
                    key.TangentIn = (float)((v1 - v0) * ((p - p0) / pn));
                break;
        }

        switch (tangentOutType)
        {
            case CurveTangent.Flat:
                key.TangentOut = 0;
                break;
            case CurveTangent.Linear:
                key.TangentOut = (float)(v1 - v);
                break;
            case CurveTangent.Smooth:
                double pn = p1 - p0;
                if (Math.Abs(pn) < double.Epsilon)
                    key.TangentOut = 0;
                else
                    key.TangentOut = (float)((v1 - v0) * ((p1 - p) / pn));
                break;
        }
    }

    #endregion


    #region Private Methods

    private int GetNumberOfCycle(double position)
    {
        double cycle = (position - Keys[0].Position) / (Keys[Keys.Count - 1].Position - Keys[0].Position);
        if (cycle < 0.0)
            cycle--;
        return (int)cycle;
    }


    private double GetCurvePosition(double position)
    {
        // only for position in curve
        KeyFrame prev = Keys[0];
        for (int i = 1; i < Keys.Count; ++i)
        {
            KeyFrame next = Keys[i];
            if (next.Position >= position)
            {
                if (prev.Continuity == CurveContinuity.Step)
                {
                    if (position >= 1.0)
                        return next.Value;
                    return prev.Value;
                }

                double t = (position - prev.Position) / (next.Position - prev.Position); //to have t in [0,1]
                double ts = t * t;
                double tss = ts * t;

                // After a lot of search on internet I have found all about spline function
                // and Bezier (phi'sss ancient) but finally use Hermite curve 
                // http://en.wikipedia.org/wiki/Cubic_Hermite_spline
                // P(t) = (2*t^3 - 3t^2 + 1)*P0 + (t^3 - 2t^2 + t)m0 + (-2t^3 + 3t^2)P1 + (t^3-t^2)m1
                // with P0.value = prev.value , m0 = prev.tangentOut, P1= next.value, m1 = next.TangentIn
                return (2 * tss - 3 * ts + 1.0) * prev.Value + (tss - 2 * ts + t) * prev.TangentOut + (3 * ts - 2 * tss) * next.Value +
                       (tss - ts) * next.TangentIn;
            }

            prev = next;
        }

        return 0f;
    }


    public SerializedProperty Serialize(Serializer.SerializationContext ctx)
    {
        SerializedProperty value = SerializedProperty.NewCompound();
        value.Add("PreLoop", new SerializedProperty((int)PreLoop));
        value.Add("PostLoop", new SerializedProperty((int)PostLoop));

        SerializedProperty keyList = SerializedProperty.NewList();
        foreach (KeyFrame key in Keys)
        {
            SerializedProperty keyProp = SerializedProperty.NewCompound();
            keyProp.Add("Position", new SerializedProperty(key.Position));
            keyProp.Add("Value", new SerializedProperty(key.Value));
            keyProp.Add("TangentIn", new SerializedProperty(key.TangentIn));
            keyProp.Add("TangentOut", new SerializedProperty(key.TangentOut));
            keyProp.Add("Continuity", new SerializedProperty((int)key.Continuity));
            keyList.ListAdd(keyProp);
        }

        value.Add("Keys", keyList);

        return value;
    }


    public void Deserialize(SerializedProperty value, Serializer.SerializationContext ctx)
    {
        PreLoop = (CurveLoopType)value.Get("PreLoop")!.IntValue;
        PostLoop = (CurveLoopType)value.Get("PostLoop")!.IntValue;

        List<SerializedProperty> keyList = value.Get("Keys")!.List;
        foreach (SerializedProperty key in keyList)
        {
            float position = key.Get("Position")!.FloatValue;
            KeyFrame curveKey = new(
                position, key.Get("Value")!.FloatValue, key.Get("TangentIn")!.FloatValue, key.Get("TangentOut")!.FloatValue,
                (CurveContinuity)key.Get("Continuity")!.IntValue);
            Keys.Add(curveKey);
        }
    }

    #endregion
}

/// <summary>
/// Defines the continuity of keys on a <see cref="AnimationCurve"/>.
/// </summary>
public enum CurveContinuity
{
    /// <summary> Interpolation can be used between this key and the next. </summary>
    Smooth,

    /// <summary> Interpolation cannot be used. A position between the two points returns this point. </summary>
    Step
}

public sealed class KeyFrame : IEquatable<KeyFrame>, IComparable<KeyFrame>
{
    #region Properties

    /// <summary> Gets or sets the indicator whether the segment between this point and the next point on the curve is discrete or continuous. </summary>
    public CurveContinuity Continuity { get; set; }

    /// <summary> Gets a position of the key on the curve. </summary>
    public float Position { get; }

    /// <summary> Gets or sets a tangent when approaching this point from the previous point on the curve. </summary>
    public float TangentIn { get; set; }

    /// <summary> Gets or sets a tangent when leaving this point to the next point on the curve. </summary>
    public float TangentOut { get; set; }

    /// <summary> Gets a value of this point. </summary>
    public float Value { get; set; }

    #endregion


    #region Constructors

    /// <summary> Creates a new instance of <see cref="KeyFrame"/> class with position: 0 and value: 0. </summary>
    public KeyFrame() : this(0, 0)
    {
        // This parameterless constructor is needed for correct serialization of CurveKeyCollection and CurveKey.
    }


    /// <summary> Creates a new instance of <see cref="KeyFrame"/> class. </summary>
    public KeyFrame(float position, float value, float tangentIn = 0, float tangentOut = 0, CurveContinuity continuity = CurveContinuity.Smooth)
    {
        Position = position;
        Value = value;
        TangentIn = tangentIn;
        TangentOut = tangentOut;
        Continuity = continuity;
    }

    #endregion


    public static bool operator !=(KeyFrame? value1, KeyFrame? value2) => !(value1 == value2);


    public static bool operator ==(KeyFrame? value1, KeyFrame? value2)
    {
        if (Equals(value1, null))
            return Equals(value2, null);

        if (Equals(value2, null))
            return Equals(value1, null);

        return Mathematics.MathOps.AlmostEquals(value1.Position, value2.Position)
               && Mathematics.MathOps.AlmostEquals(value1.Value, value2.Value)
               && Mathematics.MathOps.AlmostEquals(value1.TangentIn, value2.TangentIn)
               && Mathematics.MathOps.AlmostEquals(value1.TangentOut, value2.TangentOut)
               && value1.Continuity == value2.Continuity;
    }


    #region Inherited Methods

    public int CompareTo(KeyFrame? other) => Position.CompareTo(other?.Position);

    public bool Equals(KeyFrame? other) => this == other;

    public override bool Equals(object? obj) => obj is KeyFrame && Equals((KeyFrame)obj);


    public override int GetHashCode() =>
        Position.GetHashCode() ^ Value.GetHashCode() ^ TangentIn.GetHashCode() ^
        TangentOut.GetHashCode() ^ Continuity.GetHashCode();

    #endregion
}

/// <summary>
/// The collection of the <see cref="KeyFrame"/> elements and a part of the <see cref="AnimationCurve"/> class.
/// </summary>
public class CurveKeyCollection : ICollection<KeyFrame>
{
    #region Private Fields

    private readonly List<KeyFrame> _keys;

    #endregion


    #region Properties

    public KeyFrame this[int index]
    {
        get => _keys[index];
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (index >= _keys.Count)
                throw new IndexOutOfRangeException("The index is out of range.");

            if (Mathematics.MathOps.AlmostEquals(_keys[index].Position, value.Position))
            {
                _keys[index] = value;
            }
            else
            {
                _keys.RemoveAt(index);
                _keys.Add(value);
            }
        }
    }

    /// <summary> Returns the count of keys in this collection. </summary>
    public int Count => _keys.Count;

    /// <summary> Returns false because it is not a read-only collection. </summary>
    public bool IsReadOnly => false;

    #endregion


    #region Constructors

    /// <summary> Creates a new instance of <see cref="CurveKeyCollection"/> class. </summary>
    public CurveKeyCollection()
    {
        _keys = [];
    }

    #endregion


    IEnumerator IEnumerable.GetEnumerator() => _keys.GetEnumerator();


    /// <summary> Adds a key to this collection. </summary>
    /// <exception cref="ArgumentNullException">Throws if <paramref name="item"/> is null.</exception>
    /// <remarks>The new key would be added respectively to the position of that key and the position of other keys.</remarks>
    public void Add(KeyFrame item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (_keys.Count == 0)
        {
            _keys.Add(item);
            return;
        }

        for (int i = 0; i < _keys.Count; i++)
        {
            if (item.Position >= _keys[i].Position)
                continue;
            
            _keys.Insert(i, item);
            return;
        }

        _keys.Add(item);
    }


    /// <summary> Removes all keys from this collection. </summary>
    public void Clear() => _keys.Clear();


    /// <summary> Determines whether this collection contains a specific key. </summary>
    public bool Contains(KeyFrame item) => _keys.Contains(item);


    /// <summary> Copies the keys of this collection to an array, starting at the array index provided. </summary>
    public void CopyTo(KeyFrame[] array, int arrayIndex) => _keys.CopyTo(array, arrayIndex);


    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>An enumerator for the <see cref="CurveKeyCollection"/>.</returns>
    public IEnumerator<KeyFrame> GetEnumerator() => _keys.GetEnumerator();


    /// <summary> Finds an element in the collection and returns its index. </summary>
    /// <returns>Index of the element; or -1 if item is not found.</returns>
    public int IndexOf(KeyFrame item) => _keys.IndexOf(item);


    /// <summary> Removes element at the specified index.  </summary>
    public void RemoveAt(int index) => _keys.RemoveAt(index);


    /// <summary> Removes specific element. </summary>
    /// <returns><c>true</c> if item is successfully removed; <c>false</c> otherwise. This method also returns <c>false</c> if item was not found.</returns>
    public bool Remove(KeyFrame item) => _keys.Remove(item);
}