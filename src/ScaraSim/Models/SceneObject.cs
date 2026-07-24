using System.Windows.Media.Media3D;

namespace ScaraSim.Models;

public enum SceneObjectKind
{
    RobotLink,
    Environment,
}

public sealed class SceneObject
{
    public required string Name { get; init; }
    public required SceneObjectKind Kind { get; init; }
    public required Model3D Model { get; init; }

    public Transform3D Transform { get; set; } = Transform3D.Identity;

    public AABB LocalBounds { get; init; }

    public bool CollisionEnabled { get; set; } = true;

    public AABB WorldBounds()
    {
        var m = Transform.Value;
        return LocalBounds.Transform(m);
    }
}
