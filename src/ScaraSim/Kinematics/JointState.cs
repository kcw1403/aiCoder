using System.Windows.Media.Media3D;

namespace ScaraSim.Kinematics;

/// <summary>
/// Joint values for the 5-axis RPRR Dual-Yaw SCARA robot.
/// q1, q3, q4, q5 are revolute joint angles in degrees.
/// S2 is the prismatic (vertical) joint displacement in millimetres.
/// </summary>
public readonly record struct JointState(
    double Q1Deg,
    double S2Mm,
    double Q3Deg,
    double Q4Deg,
    double Q5Deg)
{
    public static JointState Home => new(0, 0, 0, 0, 0);
}

/// <summary>
/// Cartesian pose of every kinematic frame origin along the chain,
/// expressed in the world coordinate system (millimetres).
/// </summary>
public sealed class ChainPose
{
    public required Point3D BaseOrigin { get; init; }
    public required Point3D Shoulder { get; init; }
    public required Point3D Elbow { get; init; }
    public required Point3D Wrist { get; init; }
    public required Point3D ToolTip { get; init; }
    public required Matrix3D ToolFrame { get; init; }
}
