using static RescueScoreManager.Data.EnumRSM;

namespace RescueScoreManager.Helper;

public static class JsonHelper
{
    public static Gender GetGenderFromJsonValue(string genderValue)
    {
        Gender gender = Gender.Mixte;
        if (genderValue.CompareTo("H") == 0)
        {
            gender = Gender.Man;
        }
        else if (genderValue.CompareTo("F") == 0)
        {
            gender = Gender.Woman;
        }

        return gender;
    }

    public static Speciality GetSpecialityFromJsonValue(string specialityValue)
    {
        Speciality speciality = Speciality.Mixte;
        if (String.Compare(specialityValue, "Eau-plate") == 0)
        {
            speciality = Speciality.EauPlate;
        }
        else if (String.Compare(specialityValue, "Côtier") == 0)
        {
            speciality = Speciality.Cotier;
        }
        else
        {
            speciality = Speciality.Mixte;
        }

        return speciality;
    }
    
    public static BeachType GetBeachTypeFromJsonValue(string specialityValue,string beachTypeValue)
    {
        BeachType beachType = BeachType.Lac;
        if (String.Compare(specialityValue, "Côtier") == 0
            || String.Compare(specialityValue, "Mixte") == 0)
        {
            if (String.Compare(beachTypeValue, "Lac") == 0)
            {
                beachType = BeachType.Lac;
            }
            else if (String.Compare(beachTypeValue, "Ocean") == 0)
            {
                beachType = BeachType.Ocean;
            }
            else
            {
                beachType = BeachType.Mer;
            }
        }

        return beachType;
    }

    public static SwimType GetSwimTypeFromJsonValue( string specialityValue, string swimTypeValue)
    {
        SwimType swimType = SwimType.Bassin_25m;
        if (String.Compare(specialityValue, "Eau-plate") == 0
            || String.Compare(specialityValue, "Mixte") == 0)
        {
            if (String.Compare(swimTypeValue, "bassin 25m") == 0)
            {
                swimType = SwimType.Bassin_25m;
            }
            else
            {
                swimType = SwimType.Bassin_50m;
            }
        }

        return swimType;
    }

    public static ChronoType GetChronoTypeFromJsonValue(string chronoValue)
    {
        ChronoType chronoType = ChronoType.Manual;
        if (String.Compare(chronoValue, "Manuel") == 0)
        {
            chronoType = ChronoType.Manual;
        }
        else
        {
            chronoType = ChronoType.Electronic;
        }

        return chronoType;
    }

}
