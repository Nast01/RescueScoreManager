using Newtonsoft.Json.Linq;

using RescueScoreManager.Helper;
using RescueScoreManager.Helpers;

using System.Diagnostics;
using System.Globalization;
using System.Security.AccessControl;
using System.Xml.Linq;

using static RescueScoreManager.Data.EnumRSM;

namespace RescueScoreManager.Data;
public partial class Competition
{
    #region Attributes
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Status Status { get; set; } = Status.Valide;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime BeginDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime EntryLimitDate { get; set; }
    public BeachType BeachType { get; set; }
    public SwimType SwimType { get; set; }
    public Speciality Speciality { get; set; }
    public ChronoType ChronoType { get; set; }
    public bool IsEligibleToNationalRecord { get; set; }
    public int PriceByAthlete { get; set; }
    public int PriceByEntry { get; set; }
    public int PriceByClub { get; set; }
    public string Organizer { get; set; }
    public string OrganizerLogoUrl { get; set; }
    public string OrganizerCapUrl { get; set; }
    public int HeadRefereeId { get; set; }
    public CompetitionLevel Level { get; set; }
    public ICollection<Club> Clubs { get; } = new List<Club>();//one-to-many relationship
    public ICollection<Race> Races { get; } = new List<Race>();//one-to-many relationship
    public ICollection<Meeting> Meetings { get; } = new List<Meeting>();//one-to-many relationship

    #endregion Attributes

    public Competition()
    {
    }

    public Competition(XElement xElement)
    {
        Id = int.Parse(xElement.Attribute(Properties.Resources.Id_XMI).Value);
        Status = (Status)Enum.Parse(typeof(Status), xElement.Attribute(Properties.Resources.Status_XMI).Value);
        Level = (CompetitionLevel)Enum.Parse(typeof(CompetitionLevel), xElement.Attribute(Properties.Resources.CompetitionLevel_XMI).Value);
        Name = xElement.Attribute(Properties.Resources.Name_XMI).Value;
        Description = xElement.Attribute(Properties.Resources.Description_XMI).Value;
        Location = xElement.Attribute(Properties.Resources.Location_XMI).Value;
        BeginDate = DateTime.ParseExact(xElement.Attribute(Properties.Resources.BeginDate_XMI).Value, "dd/MM/yyyy", CultureInfo.InvariantCulture);
        EndDate = DateTime.ParseExact(xElement.Attribute(Properties.Resources.EndDate_XMI).Value, "dd/MM/yyyy", CultureInfo.InvariantCulture);
        EntryLimitDate = DateTime.ParseExact(xElement.Attribute(Properties.Resources.EntryLimitDate_XMI).Value, "dd/MM/yyyy", CultureInfo.InvariantCulture);
        if (string.IsNullOrEmpty(xElement.Attribute(Properties.Resources.BeachType_XMI).Value))
        {
            BeachType = BeachType.Lac;
        }
        else
        {
            BeachType = (BeachType)Enum.Parse(typeof(BeachType), xElement.Attribute(Properties.Resources.BeachType_XMI).Value);
        }

        if (string.IsNullOrEmpty(xElement.Attribute(Properties.Resources.SwimType_XMI).Value))
        {
            SwimType = SwimType.Bassin_25m;
        }
        else
        {
            SwimType = (SwimType)Enum.Parse(typeof(SwimType), xElement.Attribute(Properties.Resources.SwimType_XMI).Value);
        }

        if (string.IsNullOrEmpty(xElement.Attribute(Properties.Resources.Speciality_XMI).Value))
        {
            Speciality = Speciality.EauPlate;
        }
        else
        {
            Speciality = (Speciality)Enum.Parse(typeof(Speciality), xElement.Attribute(Properties.Resources.Speciality_XMI).Value);
        }

        if (string.IsNullOrEmpty(xElement.Attribute(Properties.Resources.ChronoType_XMI).Value))
        {
            ChronoType = ChronoType.Manual;
        }
        else
        {
            ChronoType = (ChronoType)Enum.Parse(typeof(ChronoType), xElement.Attribute(Properties.Resources.ChronoType_XMI).Value);
        }
        IsEligibleToNationalRecord = bool.Parse(xElement.Attribute(Properties.Resources.IsEligibleToNationalRecord_XMI).Value);
        PriceByAthlete = int.Parse(xElement.Attribute(Properties.Resources.PriceByAthlete_XMI).Value);
        PriceByEntry = int.Parse(xElement.Attribute(Properties.Resources.PriceByEntry_XMI).Value);
        PriceByClub = int.Parse(xElement.Attribute(Properties.Resources.PriceByClub_XMI).Value);
        Organizer = xElement.Attribute(Properties.Resources.Organizer_XMI).Value;
        OrganizerLogoUrl = xElement.Attribute(Properties.Resources.LogoUrl_XMI).Value;
        OrganizerCapUrl = xElement.Attribute(Properties.Resources.CapUrl_XMI).Value;
        HeadRefereeId = int.Parse(xElement.Attribute(Properties.Resources.HeadReferee_XMI).Value);
    }

    public Competition(JToken data)
    {
        Id = data["Id"].Value<int>();
        Status = (Status)Enum.Parse(typeof(Status), data["Statut"].Value<string>());
        Level = (CompetitionLevel)data["Niveau"].Value<int>();
        Name = TextHelper.CleanedText(TextHelper.RemoveDiacritics(data["Nom"].Value<string>())).Trim();
        Description = data["Description"].Value<string>();
        Location = data["Lieu"].Value<string>();
        PriceByAthlete = data["TarifParAthlete"].Value<int>();
        PriceByEntry = data["TarifParEngagement"].Value<int>();
        PriceByClub = data["TarifParClub"].Value<int>();

        DateTime beginDate;
        DateTime.TryParse(data["Debut"].Value<string>(), out beginDate);
        BeginDate = beginDate;
        DateTime endDate;
        DateTime.TryParse(data["Fin"].Value<string>(), out endDate);
        EndDate = endDate;
        DateTime entryLimitDate;
        DateTime.TryParse(data["DebutEngagement"].Value<string>(), out entryLimitDate);
        EntryLimitDate = entryLimitDate;

        Speciality = JsonHelper.GetSpecialityFromJsonValue(data["specialiteLabel"].Value<string>());
        BeachType = JsonHelper.GetBeachTypeFromJsonValue(data["specialiteLabel"].Value<string>(), data["water"].Value<string>());
        SwimType = JsonHelper.GetSwimTypeFromJsonValue(data["specialiteLabel"].Value<string>(), data["bassin"].Value<string>());
        ChronoType = JsonHelper.GetChronoTypeFromJsonValue(data["chronoLabel"].Value<string>());

        IsEligibleToNationalRecord = data["isEligibleToNationalRecord"].Value<bool>();

        Organizer = data["Organisme"]["NomOrga"].Value<string>();
        OrganizerLogoUrl = data["Organisme"]["logo"].Value<string>();
        OrganizerCapUrl = data["Organisme"]["bonnet"].Value<string>();

        HeadRefereeId = data["officielPrincipal"].HasValues == false ? 0 : data["officielPrincipal"]["Id"].Value<int>();
    }


    public XElement WriteXml()
    {
        XElement xElement = new XElement(Properties.Resources.Competition_XMI,
                                new XAttribute(Properties.Resources.Id_XMI, Id),
                                //new XAttribute(Properties.Resources.IdApi_XMI, IdApi),
                                new XAttribute(Properties.Resources.Name_XMI, Name),
                                new XAttribute(Properties.Resources.Status_XMI, Status.ToString()),
                                new XAttribute(Properties.Resources.CompetitionLevel_XMI, Level.ToString()),
                                new XAttribute(Properties.Resources.Description_XMI, Description.Replace("\\r\\n", Environment.NewLine)),
                                new XAttribute(Properties.Resources.Location_XMI, Location),
                                new XAttribute(Properties.Resources.BeginDate_XMI, BeginDate.ToShortDateString()),
                                new XAttribute(Properties.Resources.EndDate_XMI, EndDate.ToShortDateString()),
                                new XAttribute(Properties.Resources.EntryLimitDate_XMI, EntryLimitDate.ToShortDateString()),
                                new XAttribute(Properties.Resources.BeachType_XMI, BeachType.ToString()),
                                new XAttribute(Properties.Resources.SwimType_XMI, SwimType.ToString()),
                                new XAttribute(Properties.Resources.Speciality_XMI, Speciality.ToString()),
                                new XAttribute(Properties.Resources.ChronoType_XMI, ChronoType.ToString()),
                                new XAttribute(Properties.Resources.IsEligibleToNationalRecord_XMI, IsEligibleToNationalRecord),
                                new XAttribute(Properties.Resources.PriceByAthlete_XMI, PriceByAthlete),
                                new XAttribute(Properties.Resources.PriceByEntry_XMI, PriceByEntry),
                                new XAttribute(Properties.Resources.PriceByClub_XMI, PriceByClub),
                                new XAttribute(Properties.Resources.Organizer_XMI, Organizer),
                                new XAttribute(Properties.Resources.LogoUrl_XMI, OrganizerLogoUrl),
                                new XAttribute(Properties.Resources.CapUrl_XMI, OrganizerCapUrl),
                                new XAttribute(Properties.Resources.HeadReferee_XMI, HeadRefereeId.ToString())
                            );
        return xElement;
    }
}
