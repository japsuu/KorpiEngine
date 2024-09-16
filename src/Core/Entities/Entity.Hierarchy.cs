using KorpiEngine.Mathematics;

namespace KorpiEngine.Entities;

public sealed partial class Entity
{
    /// <returns>True if <paramref name="testChild"/> is a child of <paramref name="testParent"/> or the same transform, false otherwise.</returns>
    public static bool IsChildOrSameTransform(Entity? testChild, Entity testParent)
    {
        Entity? child = testChild;
        while (child != null)
        {
            if (child == testParent)
                return true;
            child = child.Parent;
        }

        return false;
    }


    public bool IsChildOf(Entity testParent)
    {
        if (InstanceID == testParent.InstanceID)
            return false;

        return IsChildOrSameTransform(this, testParent);
    }


    public bool SetParent(Entity? newParent, bool worldPositionStays = true)
    {
        if (newParent == Parent)
            return true;

        // Make sure that the new father is not a child of this transform.
        if (IsChildOrSameTransform(newParent, this))
            return false;

        // Save the old position in world space
        Vector3 worldPosition = new();
        Quaternion worldRotation = new();
        Matrix4x4 worldScale = new();

        if (worldPositionStays)
        {
            worldPosition = Transform.Position;
            worldRotation = Transform.Rotation;
            worldScale = Transform.GetWorldRotationAndScale();
        }

        if (newParent != Parent)
        {
            Parent?._childList.Remove(this);

            if (newParent != null)
                newParent._childList.Add(this);

            Parent = newParent;
        }

        if (worldPositionStays)
        {
            if (Parent != null)
            {
                Transform.LocalPosition = Parent.Transform.InverseTransformPoint(worldPosition);
                Transform.LocalRotation = (Parent.Transform.Rotation.Inverse() * worldRotation).NormalizeSafe();
            }
            else
            {
                Transform.LocalPosition = worldPosition;
                Transform.LocalRotation = worldRotation.NormalizeSafe();
            }

            Transform.LocalScale = Vector3.One;
            Matrix4x4 inverseRotationScale = Transform.GetWorldRotationAndScale().Inverse() * worldScale;
            Transform.LocalScale = new Vector3(inverseRotationScale.M11, inverseRotationScale.M22, inverseRotationScale.M33);
        }

        HierarchyStateChanged();

        return true;
    }


    private void HierarchyStateChanged()
    {
        bool newState = _isEnabled && IsParentEnabled;
        if (EnabledInHierarchy != newState)
        {
            EnabledInHierarchy = newState;
            foreach (EntityComponent component in GetComponents<EntityComponent>())
                component.HierarchyStateChanged();
        }

        foreach (Entity child in _childList)
            child.HierarchyStateChanged();
    }


    private void SetEnabled(bool state)
    {
        _isEnabled = state;
        HierarchyStateChanged();
    }
}