using ScaraSim.Models;

namespace ScaraSim.Services;

public sealed record CollisionPair(string A, string B);

public sealed class CollisionResult
{
    public IReadOnlyList<CollisionPair> Pairs { get; init; } = Array.Empty<CollisionPair>();
    public bool HasCollision => Pairs.Count > 0;
}

/// <summary>
/// Approximate interference detection using axis-aligned bounding boxes.
/// Precise mesh-level collision is intentionally out of scope; this is a fast
/// broad-phase check suitable for visual feedback during jogging.
/// </summary>
public sealed class AabbCollisionService
{
    public double Margin { get; set; } = 0.0;

    public CollisionResult Check(IReadOnlyList<SceneObject> objects)
    {
        var pairs = new List<CollisionPair>();

        var active = objects.Where(o => o.CollisionEnabled).ToList();
        var boxes = active.Select(o => o.WorldBounds().Expand(Margin * 0.5)).ToArray();

        for (int i = 0; i < active.Count; i++)
        {
            for (int j = i + 1; j < active.Count; j++)
            {
                if (!ShouldTest(active[i], active[j]))
                    continue;

                if (boxes[i].Intersects(boxes[j]))
                    pairs.Add(new CollisionPair(active[i].Name, active[j].Name));
            }
        }

        return new CollisionResult { Pairs = pairs };
    }

    private static bool ShouldTest(SceneObject a, SceneObject b)
    {
        if (a.Kind == SceneObjectKind.RobotLink && b.Kind == SceneObjectKind.RobotLink)
            return AreNonAdjacentLinks(a.Name, b.Name);

        return true;
    }

    private static bool AreNonAdjacentLinks(string a, string b)
    {
        int ia = LinkIndex(a);
        int ib = LinkIndex(b);
        if (ia < 0 || ib < 0)
            return true;

        return Math.Abs(ia - ib) > 1;
    }

    private static int LinkIndex(string name) => name switch
    {
        "Base" => 0,
        "Column" => 1,
        "Link1" => 2,
        "Link2" => 3,
        "Tool" => 4,
        _ => -1,
    };
}
