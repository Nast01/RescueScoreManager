using System.Xml.Linq;

using Microsoft.Office.Interop.Word;

using Newtonsoft.Json.Linq;

using RescueScoreManager.Helper;

using static RescueScoreManager.Data.EnumRSM;

namespace RescueScoreManager.Data;

public class RaceFormatDetail
{
    public int Id { get; set; }
    public int Order { get; set; }
    public string Label { get; set; }
    public string FullLabel { get; set; }
    public string LevelLabel { get; set; }
    public HeatLevel Level { get; set; }
    public int NumberOfRun { get; set; }
    public QualificationType QualificationMethod { get; set; }
    public string QualificationMethodLabel { get; set; }
    public int SpotsPerRace { get; set; }
    public int QualifyingSpots { get; set; }
    public RaceFormatConfiguration RaceFormatConfiguration { get; set; }

    public RaceFormatDetail(JToken jData, RaceFormatConfiguration raceFormatConfiguration)
    {
        Id = jData["id"].Value<int>();
        Order = jData["ordre"].Value<int>();
        Label = jData["label"].Value<string>();
        FullLabel = jData["fullLabel"].Value<string>();
        LevelLabel = jData["niveauLabel"].Value<string>();
        Level = JsonHelper.GetHeatLevelFromJsonValue(jData["niveau"].Value<string>());
        NumberOfRun = jData["nbCourses"].Value<int>();
        QualificationMethod = JsonHelper.GetQualificationTypeFromJsonValue(jData["logiqueQualification"].Value<string>());
        QualificationMethodLabel = jData["logiqueQualificationLabel"].Value<string>();
        SpotsPerRace = jData["nbPlaceParCourse"].Value<int>();
        QualifyingSpots = jData["nbPlaceQualificative"].Value<int>();

        RaceFormatConfiguration = raceFormatConfiguration;
        raceFormatConfiguration.RaceFormatDetails.Add(this);
    }

    public RaceFormatDetail(XElement xElement, RaceFormatConfiguration raceFormatConfiguration)
    {
        Id = int.Parse(xElement.Attribute(Properties.Resources.Id_XMI).Value);
        Order = int.Parse(xElement.Attribute(Properties.Resources.Order_XMI).Value);
        Label = xElement.Attribute(Properties.Resources.Label_XMI).Value;
        FullLabel = xElement.Attribute(Properties.Resources.FullLabel_XMI).Value;
        LevelLabel = xElement.Attribute(Properties.Resources.LevelLabel_XMI).Value;
        Level = (HeatLevel)Enum.Parse(typeof(HeatLevel), xElement.Attribute(Properties.Resources.Level_XMI).Value);
        NumberOfRun = int.Parse(xElement.Attribute(Properties.Resources.NumberOfRun_XMI).Value);
        QualificationMethod = (QualificationType)Enum.Parse(typeof(QualificationType), xElement.Attribute(Properties.Resources.QualificationMethod_XMI).Value);
        QualificationMethodLabel = xElement.Attribute(Properties.Resources.QualificationMethodLabel_XMI).Value;
        SpotsPerRace = int.Parse(xElement.Attribute(Properties.Resources.SpotsPerRace_XMI).Value);
        QualifyingSpots = int.Parse(xElement.Attribute(Properties.Resources.QualifyingSpots_XMI).Value);

        RaceFormatConfiguration = raceFormatConfiguration;
        RaceFormatConfiguration.RaceFormatDetails.Add(this);
    }

    #region Public Method
    public XElement WriteXml()
    {
        XElement xElement = new XElement(Properties.Resources.RaceFormatDetail_XMI,
                            new XAttribute(Properties.Resources.Id_XMI, Id),
                            new XAttribute(Properties.Resources.Order_XMI, Order),
                            new XAttribute(Properties.Resources.Label_XMI, Label),
                            new XAttribute(Properties.Resources.FullLabel_XMI, FullLabel),
                            new XAttribute(Properties.Resources.LevelLabel_XMI, LevelLabel.ToString()),
                            new XAttribute(Properties.Resources.NumberOfRun_XMI, NumberOfRun),
                            new XAttribute(Properties.Resources.QualificationMethod_XMI, QualificationMethod.ToString()),
                            new XAttribute(Properties.Resources.QualificationMethodLabel_XMI, QualificationMethodLabel),
                            new XAttribute(Properties.Resources.SpotsPerRace_XMI, SpotsPerRace),
                            new XAttribute(Properties.Resources.QualifyingSpots_XMI, QualifyingSpots)
                            );

        return xElement;
    }
    #endregion Public Method
}
