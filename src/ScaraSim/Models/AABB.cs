using System.Windows.Media.Media3D;

namespace ScaraSim.Models;

public readonly struct AABB
{
    public Point3D Min { get; }
    public Point3D Max { get; }

    public AABB(Point3D min, Point3D max)
    {
        Min = min;
        Max = max;
    }

    public bool IsEmpty => Max.X < Min.X || Max.Y < Min.Y || Max.Z < Min.Z;

    public static AABB FromRect3D(Rect3D r) =>
        new(r.Location, new Point3D(r.X + r.SizeX, r.Y + r.SizeY, r.Z + r.SizeZ));

    public static AABB FromBounds(Rect3D bounds) => FromRect3D(bounds);

    public bool Intersects(in AABB other)
    {
        if (IsEmpty || other.IsEmpty)
            return false;

        return Min.X <= other.Max.X && Max.X >= other.Min.X
            && Min.Y <= other.Max.Y && Max.Y >= other.Min.Y
            && Min.Z <= other.Max.Z && Max.Z >= other.Min.Z;
    }

    public AABB Expand(double margin) =>
        new(new Point3D(Min.X - margin, Min.Y - margin, Min.Z - margin),
            new Point3D(Max.X + margin, Max.Y + margin, Max.Z + margin));

    public AABB Transform(Matrix3D m)
    {
        Span<Point3D> corners = stackalloc Point3D[8]
        {
            new(Min.X, Min.Y, Min.Z),
            new(Max.X, Min.Y, Min.Z),
            new(Min.X, Max.Y, Min.Z),
            new(Max.X, Max.Y, Min.Z),
            new(Min.X, Min.Y, Max.Z),
            new(Max.X, Min.Y, Max.Z),
            new(Min.X, Max.Y, Max.Z),
            new(Max.X, Max.Y, Max.Z),
        };

        double minX = double.MaxValue, minY = double.MaxValue, minZ = double.MaxValue;
        double maxX = double.MinValue, maxY = double.MinValue, maxZ = double.MinValue;

        foreach (var corner in corners)
        {
            var p = m.Transform(corner);
            if (p.X < minX) minX = p.X;
            if (p.Y < minY) minY = p.Y;
            if (p.Z < minZ) minZ = p.Z;
            if (p.X > maxX) maxX = p.X;
            if (p.Y > maxY) maxY = p.Y;
            if (p.Z > maxZ) maxZ = p.Z;
        }

        return new AABB(new Point3D(minX, minY, minZ), new Point3D(maxX, maxY, maxZ));
    }
}
