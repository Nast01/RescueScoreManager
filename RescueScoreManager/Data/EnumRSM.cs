using System.ComponentModel;

namespace RescueScoreManager.Data;

public class EnumRSM
{
    public static  string GetEnumDescription(Enum value)
    {
        // Get the Description attribute value for the enum value
        var fieldInfo = value.GetType().GetField(value.ToString());
        var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute));

        return attribute != null ? attribute.Description : value.ToString();
    }

    public enum Status
    {
        Valide,
        NonValide
    }

    public enum BeachType
    {
        Mer,
        Lac,
        Ocean,
    }

    public enum SwimType
    {
        Bassin_50m,
        Bassin_25m
    }

    public enum Speciality
    {
        Cotier,
        EauPlate,
        Mixte,
    }

    public enum RefereeLevel
    {
        A,
        B,
        C,
        ND,
    }

    public enum ChronoType
    {
        Manual,
        Electronic,
    }

    public enum Gender
    {
        [Description("Messieurs")]
        Man,
        [Description("Dames")]
        Woman,
        [Description("Mixte")]
        Mixte
    }

    public enum HeatType
    {
        Heats,
        QuarterFinal,
        SemiFinal,
        Final,
    }

    public enum ExcelType
    {
        STARTLIST
    }
}
