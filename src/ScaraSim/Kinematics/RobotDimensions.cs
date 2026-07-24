using System.Windows.Media.Media3D;

namespace ScaraSim.Kinematics;

/// <summary>
/// Adjustable link dimensions for the RPRR Dual-Yaw SCARA robot, in millimetres.
/// Defaults: link1 = link2 = 440 mm, tool = 340 mm.
/// </summary>
public sealed class RobotDimensions
{
    public double Link1Length { get; set; } = 440.0;
    public double Link2Length { get; set; } = 440.0;
    public double ToolLength { get; set; } = 340.0;

    public double BaseHeight { get; set; } = 200.0;

    public double ColumnHeight { get; set; } = 600.0;

    public double VerticalStrokeMin { get; set; } = 0.0;
    public double VerticalStrokeMax { get; set; } = 400.0;

    public RobotDimensions Clone() => new()
    {
        Link1Length = Link1Length,
        Link2Length = Link2Length,
        ToolLength = ToolLength,
        BaseHeight = BaseHeight,
        ColumnHeight = ColumnHeight,
        VerticalStrokeMin = VerticalStrokeMin,
        VerticalStrokeMax = VerticalStrokeMax,
    };
}
