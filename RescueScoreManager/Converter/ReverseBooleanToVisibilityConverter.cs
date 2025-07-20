using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RescueScoreManager.Converter;

public class ReverseBooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool boolean = false;
        if (value is bool) { boolean = (bool)value; }
        return boolean ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is Visibility && (Visibility)value == Visibility.Visible;
    }
}
