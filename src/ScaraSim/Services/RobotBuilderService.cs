using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using ScaraSim.Kinematics;
using ScaraSim.Models;

namespace ScaraSim.Services;

public sealed class RobotVisualParts
{
    public required SceneObject Base { get; init; }
    public required SceneObject Column { get; init; }
    public required SceneObject Link1 { get; init; }
    public required SceneObject Link2 { get; init; }
    public required SceneObject Tool { get; init; }

    public IEnumerable<SceneObject> All()
    {
        yield return Base;
        yield return Column;
        yield return Link1;
        yield return Link2;
        yield return Tool;
    }
}

public sealed class RobotBuilderService
{
    private const double LinkThickness = 90.0;
    private const double LinkHeight = 70.0;
    private const double ColumnSize = 140.0;
    private const double ToolDiameter = 60.0;

    public RobotVisualParts BuildDefaultRobot(RobotDimensions dim)
    {
        var baseObj = MakeObject("Base",
            BoxMesh(360, 360, dim.BaseHeight, new Point3D(0, 0, dim.BaseHeight / 2)),
            Colors.DimGray);

        double columnCenterZ = dim.BaseHeight + dim.ColumnHeight / 2;
        var column = MakeObject("Column",
            BoxMesh(ColumnSize, ColumnSize, dim.ColumnHeight,
                new Point3D(0, 0, columnCenterZ)),
            Colors.SlateGray);

        var link1 = MakeObject("Link1",
            LinkMesh(dim.Link1Length),
            Colors.SteelBlue);

        var link2 = MakeObject("Link2",
            LinkMesh(dim.Link2Length),
            Colors.DarkOrange);

        var tool = MakeObject("Tool",
            ToolMesh(dim.ToolLength),
            Colors.Firebrick);

        return new RobotVisualParts
        {
            Base = baseObj,
            Column = column,
            Link1 = link1,
            Link2 = link2,
            Tool = tool,
        };
    }

    public void ApplyPose(RobotVisualParts parts, JointState joints, RobotDimensions dim)
    {
        var pose = ScaraForwardKinematics.Solve(joints, dim);

        parts.Base.Transform = Transform3D.Identity;
        parts.Column.Transform = Transform3D.Identity;

        double a1Deg = joints.Q1Deg + joints.Q3Deg;
        parts.Link1.Transform = Compose(
            rotationZDeg: a1Deg,
            translation: pose.Shoulder);

        double a2Deg = a1Deg + joints.Q4Deg;
        parts.Link2.Transform = Compose(
            rotationZDeg: a2Deg,
            translation: pose.Elbow);

        double toolYawDeg = a2Deg + joints.Q5Deg;
        parts.Tool.Transform = Compose(
            rotationZDeg: toolYawDeg,
            translation: pose.Wrist);
    }

    private static Transform3D Compose(double rotationZDeg, Point3D translation)
    {
        var group = new Transform3DGroup();
        group.Children.Add(new RotateTransform3D(
            new AxisAngleRotation3D(new Vector3D(0, 0, 1), rotationZDeg)));
        group.Children.Add(new TranslateTransform3D(
            translation.X, translation.Y, translation.Z));
        group.Freeze();
        return group;
    }

    private static SceneObject MakeObject(string name, MeshGeometry3D mesh, Color color)
    {
        var material = MaterialHelper.CreateMaterial(color);
        var model = new GeometryModel3D(mesh, material) { BackMaterial = material };
        model.Freeze();

        return new SceneObject
        {
            Name = name,
            Kind = SceneObjectKind.RobotLink,
            Model = model,
            LocalBounds = AABB.FromBounds(model.Bounds),
        };
    }

    private static MeshGeometry3D BoxMesh(double sx, double sy, double sz, Point3D center)
    {
        var b = new MeshBuilder(true, true);
        b.AddBox(center, sx, sy, sz);
        return b.ToMesh(true);
    }

    private static MeshGeometry3D LinkMesh(double length)
    {
        var b = new MeshBuilder(true, true);
        var center = new Point3D(length / 2, 0, 0);
        b.AddBox(center, length, LinkThickness, LinkHeight);
        b.AddSphere(new Point3D(0, 0, 0), LinkHeight * 0.6);
        b.AddSphere(new Point3D(length, 0, 0), LinkHeight * 0.6);
        return b.ToMesh(true);
    }

    private static MeshGeometry3D ToolMesh(double length)
    {
        double coneHeight = Math.Min(ToolDiameter, length * 0.5);
        double shaftEnd = length - coneHeight;

        var b = new MeshBuilder(true, true);
        b.AddCylinder(new Point3D(0, 0, 0), new Point3D(0, 0, -shaftEnd), ToolDiameter / 2, 24);
        b.AddCone(new Point3D(0, 0, -shaftEnd), new Vector3D(0, 0, -1),
            ToolDiameter / 2, 0, coneHeight, true, true, 24);
        return b.ToMesh(true);
    }
}
