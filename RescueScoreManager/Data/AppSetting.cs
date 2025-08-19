using System.Xml.Linq;

namespace RescueScoreManager.Data;

public class AppSetting
{
    public int NumberOfLanes { get; set; }
    public bool IsRankingByTime { get; set; }
    public bool IsRankingByFinal { get; set; }
    public int NumberOfAthleteComputedByClub { get; set; }
    public int NumberOfRelayComputedByClub { get; set; }
    public bool HasAres { get; set; }
    public string ChronoPath { get; set; } = string.Empty;
    public bool IsRelayByHeatFinal { get; set; }
    public int NumberOfForeignAthleteInFinal { get; set; }
    public int NumberOfAthleteInFinal { get; set; }
    public int NumberMinOfAthleteInFinalByHeat { get; set; }
    public bool HasPointForeignAthlete { get; set; }

    public AppSetting()
    {
        NumberOfLanes = 8;
        IsRankingByTime = true;
        IsRankingByFinal = false;
        NumberOfAthleteComputedByClub = 3;
        NumberOfRelayComputedByClub = 1;
        HasAres = false;
        ChronoPath = string.Empty;
        IsRelayByHeatFinal = false;
        NumberOfForeignAthleteInFinal = 2;
        NumberOfAthleteInFinal = 8;
        NumberMinOfAthleteInFinalByHeat = 32;
        HasPointForeignAthlete = false;
    }

    public AppSetting(XElement xElement)
    {
        NumberOfLanes = int.Parse(xElement.Attribute(Properties.Resources.NumberOfLanes_XMI)?.Value ?? "8");
        IsRankingByTime = bool.Parse(xElement.Attribute(Properties.Resources.IsRankingByTime_XMI)?.Value ?? "true");
        IsRankingByFinal = bool.Parse(xElement.Attribute(Properties.Resources.IsRankingByFinal_XMI)?.Value ?? "false");
        NumberOfAthleteComputedByClub = int.Parse(xElement.Attribute(Properties.Resources.NumberOfAthleteComputedByClub_XMI)?.Value ?? "3");
        NumberOfRelayComputedByClub = int.Parse(xElement.Attribute(Properties.Resources.NumberOfRelayComputedByClub_XMI)?.Value ?? "1");
        HasAres = bool.Parse(xElement.Attribute(Properties.Resources.HasAres_XMI)?.Value ?? "false");
        ChronoPath = xElement.Attribute(Properties.Resources.ChronoPath_XMI)?.Value ?? string.Empty;
        IsRelayByHeatFinal = bool.Parse(xElement.Attribute(Properties.Resources.IsRelayByHeatFinal_XMI)?.Value ?? "false");
        NumberOfForeignAthleteInFinal = int.Parse(xElement.Attribute(Properties.Resources.NumberOfForeignAthleteInFinal_XMI)?.Value ?? "2");
        NumberOfAthleteInFinal = int.Parse(xElement.Attribute(Properties.Resources.NumberOfAthleteInFinal_XMI)?.Value ?? "8");
        NumberMinOfAthleteInFinalByHeat = int.Parse(xElement.Attribute(Properties.Resources.NumberMinOfAthleteInFinalByHeat_XMI)?.Value ?? "32");
        HasPointForeignAthlete = bool.Parse(xElement.Attribute(Properties.Resources.HasPointForeignAthlete_XMI)?.Value ?? "false");
    }

    public XElement WriteXml()
    {
        XElement xElement = new XElement(Properties.Resources.AppSetting_XMI,
            new XAttribute(Properties.Resources.NumberOfLanes_XMI, NumberOfLanes),
            new XAttribute(Properties.Resources.IsRankingByTime_XMI, IsRankingByTime),
            new XAttribute(Properties.Resources.IsRankingByFinal_XMI, IsRankingByFinal),
            new XAttribute(Properties.Resources.NumberOfAthleteComputedByClub_XMI, NumberOfAthleteComputedByClub),
            new XAttribute(Properties.Resources.NumberOfRelayComputedByClub_XMI, NumberOfRelayComputedByClub),
            new XAttribute(Properties.Resources.HasAres_XMI, HasAres),
            new XAttribute(Properties.Resources.ChronoPath_XMI, ChronoPath),
            new XAttribute(Properties.Resources.IsRelayByHeatFinal_XMI, IsRelayByHeatFinal),
            new XAttribute(Properties.Resources.NumberOfForeignAthleteInFinal_XMI, NumberOfForeignAthleteInFinal),
            new XAttribute(Properties.Resources.NumberOfAthleteInFinal_XMI, NumberOfAthleteInFinal),
            new XAttribute(Properties.Resources.NumberMinOfAthleteInFinalByHeat_XMI, NumberMinOfAthleteInFinalByHeat),
            new XAttribute(Properties.Resources.HasPointForeignAthlete_XMI, HasPointForeignAthlete)
        );

        return xElement;
    }
}
