using System.Globalization;
using System.Windows.Data;

using RescueScoreManager.Services;

namespace RescueScoreManager.Converter;

/// <summary>
/// Value converter for formatting localized strings with parameters
/// </summary>
public class LocalizedFormatConverter : IMultiValueConverter
{
    private readonly ILocalizationService _localizationService;

    public LocalizedFormatConverter()
    {
        _localizationService = ResourceManagerLocalizationService.Instance;
    }

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is not string key || values == null)
        {
            return string.Empty;
        }

        return _localizationService.GetString(key, values);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
