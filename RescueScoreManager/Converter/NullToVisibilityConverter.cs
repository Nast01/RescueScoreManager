using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RescueScoreManager.Converter;

/// <summary>
/// Converts null values to Visibility
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isNull = value == null;
        bool inverse = parameter?.ToString()?.ToLowerInvariant() == "inverse";

        if (inverse)
        {
            return isNull ? Visibility.Visible : Visibility.Collapsed;
        }
        else
        {
            return isNull ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}