using System.Windows.Media.Media3D;

namespace ScaraSim.Kinematics;

/// <summary>
/// Forward kinematics for a 5-axis RPRR Dual-Yaw SCARA arm.
///
/// Axis layout (all revolute axes are parallel to world +Z, SCARA-style):
///   q1 : base yaw           (revolute, rotates about column Z)
///   S2 : vertical carriage  (prismatic, slides along Z)
///   q3 : link-1 yaw         (revolute, rotates link1 of length L1 in the XY plane)
///   q4 : link-2 yaw         (revolute, rotates link2 of length L2) -- the second "dual" yaw
///   q5 : tool yaw           (revolute, spins the tool of length Ltool)
///
/// Because every revolute axis is vertical, the planar reach is governed by q1/q3/q4
/// and the height purely by S2; the tool extends downward by Ltool from the wrist.
/// </summary>
public static class ScaraForwardKinematics
{
    public static ChainPose Solve(JointState joints, RobotDimensions dim)
    {
        double q1 = DegToRad(joints.Q1Deg);
        double q3 = DegToRad(joints.Q3Deg);
        double q4 = DegToRad(joints.Q4Deg);
        double q5 = DegToRad(joints.Q5Deg);

        double s2 = Clamp(joints.S2Mm, dim.VerticalStrokeMin, dim.VerticalStrokeMax);

        var baseOrigin = new Point3D(0, 0, 0);

        double shoulderZ = dim.BaseHeight + dim.ColumnHeight - s2;
        var shoulder = new Point3D(0, 0, shoulderZ);

        double a1 = q1 + q3;
        var elbow = new Point3D(
            shoulder.X + dim.Link1Length * Math.Cos(a1),
            shoulder.Y + dim.Link1Length * Math.Sin(a1),
            shoulder.Z);

        double a2 = a1 + q4;
        var wrist = new Point3D(
            elbow.X + dim.Link2Length * Math.Cos(a2),
            elbow.Y + dim.Link2Length * Math.Sin(a2),
            elbow.Z);

        var toolTip = new Point3D(wrist.X, wrist.Y, wrist.Z - dim.ToolLength);

        double toolYaw = a2 + q5;
        var toolFrame = BuildToolFrame(wrist, toolYaw);

        return new ChainPose
        {
            BaseOrigin = baseOrigin,
            Shoulder = shoulder,
            Elbow = elbow,
            Wrist = wrist,
            ToolTip = toolTip,
            ToolFrame = toolFrame,
        };
    }

    private static Matrix3D BuildToolFrame(Point3D wrist, double yawRad)
    {
        double c = Math.Cos(yawRad);
        double s = Math.Sin(yawRad);

        return new Matrix3D(
            c, s, 0, 0,
            -s, c, 0, 0,
            0, 0, 1, 0,
            wrist.X, wrist.Y, wrist.Z, 1);
    }

    private static double DegToRad(double deg) => deg * Math.PI / 180.0;

    private static double Clamp(double value, double min, double max)
        => value < min ? min : value > max ? max : value;
}
