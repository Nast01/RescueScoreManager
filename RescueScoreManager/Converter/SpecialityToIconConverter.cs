using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

using static RescueScoreManager.Data.EnumRSM;

namespace RescueScoreManager.Converter;

public class SpecialityToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Speciality speciality)
        {
            return "Swimming";
        }

        return speciality switch
        {
            Speciality.EauPlate => "Pool",            // Pool/Lake icon
            Speciality.Cotier => "Waves",             // Ocean/Beach waves icon  
            Speciality.Mixte => "Swimming",           // Swimming icon
            _ => "Swimming"                           // Default icon
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
