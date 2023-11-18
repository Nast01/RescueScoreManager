namespace RescueScoreManager.Model;

public class EnumRSM
{
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
        Man,
        Woman,
        Mixte
    }

    public enum APIType
    {
        Competition,
        Club,
    }
}
