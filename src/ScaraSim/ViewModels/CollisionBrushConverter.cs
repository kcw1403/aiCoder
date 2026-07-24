using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ScaraSim.ViewModels;

public sealed class CollisionBrushConverter : IValueConverter
{
    public static readonly CollisionBrushConverter Instance = new();

    private static readonly Brush Colliding =
        new SolidColorBrush(Color.FromRgb(0xC0, 0x39, 0x2B));

    private static readonly Brush Clear =
        new SolidColorBrush(Color.FromRgb(0x2E, 0x7D, 0x32));

    static CollisionBrushConverter()
    {
        Colliding.Freeze();
        Clear.Freeze();
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? Colliding : Clear;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
