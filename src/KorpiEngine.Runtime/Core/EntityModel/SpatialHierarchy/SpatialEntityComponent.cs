using KorpiEngine.Core.API;

namespace KorpiEngine.Core.EntityModel.SpatialHierarchy;

/// <summary>
/// Provides a <see cref="Transform"/> for the inheriting component, allowing it to be positioned in 3D space.
/// Can also be attached to entities to provide a root transform.
/// </summary>
public class SpatialEntityComponent : EntityComponent
{
    private Entity _entity = null!;
    private SpatialEntityComponent? _spatialParent;
    private readonly List<SpatialEntityComponent> _spatialChildren = [];
    
    public readonly Transform Transform = new();


    /// <summary>
    /// Binds the component to the given entity reference.
    /// </summary>
    internal void Bind(Entity entity) => _entity = entity;


    public bool IsRootComponent => _spatialParent == null;
    public bool IsLeafComponent => _spatialChildren.Count == 0;


    #region Parenting


    public bool SetParent(SpatialEntityComponent? newParent, bool worldPositionStays = true)
    {
        if (IsRootComponent)
            throw new NotImplementedException("TODO: Implement entity hierarchy/parenting. Currently only component hierarchy is supported.");  // What happens to the entity when the root component is parented to another entity?
        
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

        HierarchyStateChanged();

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


    private void HierarchyStateChanged()
    {
        //TODO: Is there any use in having spatial components have an enabled field? Should only entities be able to be enabled/disabled?
        bool newState = _entity.IsEnabled && Entity.IsParentEnabled();
        Entity.EnabledInHierarchy = newState;

        foreach (Transform child in Children)
            child.HierarchyStateChanged();
    }

    #endregion
}