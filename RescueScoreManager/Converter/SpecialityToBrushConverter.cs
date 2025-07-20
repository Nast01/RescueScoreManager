using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using RescueScoreManager.Data;

using static RescueScoreManager.Data.EnumRSM;

namespace RescueScoreManager.Converter;

public class SpecialityToBrushConverter : IValueConverter
{
    private static readonly Dictionary<Speciality, LinearGradientBrush> _brushes = new()
    {
        {
            Speciality.EauPlate,
            new LinearGradientBrush(
                new GradientStopCollection
                {
                    new(Color.FromRgb(52, 152, 219), 0),   // Light blue
                    new(Color.FromRgb(41, 128, 185), 1)    // Darker blue
                },
                new Point(0, 0), new Point(0, 1))
        },
        {
            Speciality.Cotier,
            new LinearGradientBrush(
                new GradientStopCollection
                {
                    new(Color.FromRgb(255, 235, 59), 0),   // Light yellow
                    new(Color.FromRgb(255, 193, 7), 1)     // Darker yellow
                },
                new Point(0, 0), new Point(0, 1))
        },
        {
            Speciality.Mixte,
            new LinearGradientBrush(
                new GradientStopCollection
                {
                     new(Color.FromRgb(46, 204, 113), 0),   // Light green
                    new(Color.FromRgb(39, 174, 96), 1)     // Darker green
                },
                new Point(0, 0), new Point(0, 1))
        }
    };

    private static readonly LinearGradientBrush _defaultBrush = new(
        new GradientStopCollection
        {
            new(Color.FromRgb(149, 165, 166), 0),   // Light gray
            new(Color.FromRgb(127, 140, 141), 1)    // Darker gray
        },
        new Point(0, 0), new Point(0, 1));

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Speciality speciality && _brushes.TryGetValue(speciality, out var brush))
        {
            return brush;
        }

        return _defaultBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
