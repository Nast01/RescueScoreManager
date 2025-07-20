using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

using RescueScoreManager.Services;

using static RescueScoreManager.Data.EnumRSM;

namespace RescueScoreManager.Converter;

public class SpecialityToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Speciality speciality)
        { 
            return ResourceManagerLocalizationService.Instance.GetString("Unknown"); 
        }

        return speciality switch
        {
            Speciality.EauPlate => ResourceManagerLocalizationService.Instance.GetString("Pool"),
            Speciality.Cotier => ResourceManagerLocalizationService.Instance.GetString("Beach"),
            _ => ResourceManagerLocalizationService.Instance.GetString("Mixte")
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
