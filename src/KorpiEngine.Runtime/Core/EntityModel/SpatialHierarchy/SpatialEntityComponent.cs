using KorpiEngine.Core.API;

namespace KorpiEngine.Core.EntityModel.SpatialHierarchy;

/// <summary>
/// Provides a <see cref="Transform"/> for the inheriting component, allowing it to be positioned in 3D space.
/// Can also be attached to entities to provide a root transform.
/// </summary>
public class SpatialEntityComponent : EntityComponent
{
    private SpatialEntityComponent? _spatialParent;
    private readonly List<SpatialEntityComponent> _spatialChildren = [];

    public readonly Transform Transform = new();

    public string SocketID = string.Empty;
    
    public bool IsRootComponent => _spatialParent == null;
    public bool HasChildren => _spatialChildren.Count > 0;


    #region Parenting

    internal bool SetParent(SpatialEntityComponent? newParent, bool worldPositionStays = true)
    {
        if (newParent == _spatialParent)
            return true;

        // Make sure that the new parent is not a child of this component.
        if (IsChildOrSameComponent(newParent, this))
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

        if (newParent != _spatialParent)
        {
            _spatialParent?._spatialChildren.Remove(this);
            newParent?._spatialChildren.Add(this);

            _spatialParent = newParent;
            Transform.Parent = newParent?.Transform;
        }

        if (worldPositionStays)
        {
            if (_spatialParent != null)
            {
                Transform.LocalRotation = Quaternion.NormalizeSafe(Quaternion.Inverse(_spatialParent.Transform.Rotation) * worldRotation);
                Transform.LocalPosition = _spatialParent.Transform.InverseTransformPoint(worldPosition);
            }
            else
            {
                Transform.LocalPosition = worldPosition;
                Transform.LocalRotation = Quaternion.NormalizeSafe(worldRotation);
            }

            Transform.LocalScale = Vector3.One;
            Matrix4x4 inverseRs = Transform.GetWorldRotationAndScale().Invert() * worldScale;
            Transform.LocalScale = new Vector3(inverseRs[0, 0], inverseRs[1, 1], inverseRs[2, 2]);
        }

        // HierarchyStateChanged();

        return true;
    }


    private static bool IsChildOrSameComponent(SpatialEntityComponent? testChild, SpatialEntityComponent testParent)
    {
        SpatialEntityComponent? child = testChild;
        while (child != null)
        {
            if (child == testParent)
                return true;
            child = child._spatialParent;
        }

        return false;
    }

    #endregion


    internal SpatialEntityComponent? FindSpatialComponentWithSocket(string? socketID)
    {
        if (SocketID == socketID)
            return this;

        foreach (SpatialEntityComponent child in _spatialChildren)
        {
            SpatialEntityComponent? result = child.FindSpatialComponentWithSocket(socketID);
            if (result != null)
                return result;
        }

        return null;
    }
}