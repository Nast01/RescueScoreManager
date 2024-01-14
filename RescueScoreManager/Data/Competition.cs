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
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime BeginDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime EntryLimitDate { get; set; }
    public BeachType? BeachType { get; set; }
    public SwimType? SwimType { get; set; }
    public Speciality? Speciality { get; set; }
    public ChronoType? ChronoType { get; set; }
    public bool IsEligibleToNationalRecord { get; set; }
    public int PriceByAthlete { get; set; }
    public int PriceByEntry { get; set; }
    public int PriceByClub { get; set; }
    public string Organizer { get; set; }
    public ICollection<Club> Clubs { get; } = new List<Club>();//one-to-many relationship
    public ICollection<Race> Races { get; } = new List<Race>();//one-to-many relationship
    public ICollection<Meeting> Meetings { get; } = new List<Meeting>();//one-to-many relationship

    #endregion Attributes

    public Competition()
    {
    }

    public Competition(XElement xElement)
    {
        Id = int.Parse(xElement.Attribute(Properties.ResourceFR.Id_XMI).Value);
        //if (xElement.Attribute(Properties.ResourceFR.IdApi_XMI) != null)
        //{
        //    IdApi = int.Parse(xElement.Attribute(Properties.ResourceFR.IdApi_XMI).Value);
        //}
        //else
        //{
        //    IdApi = 0;
        //}
        Name = xElement.Attribute(Properties.ResourceFR.Name_XMI).Value;
        Description = xElement.Attribute(Properties.ResourceFR.Description_XMI).Value;
        Location = xElement.Attribute(Properties.ResourceFR.Location_XMI).Value;
        BeginDate = DateTime.ParseExact(xElement.Attribute(Properties.ResourceFR.BeginDate_XMI).Value, "dd/MM/yyyy", CultureInfo.InvariantCulture);
        EndDate = DateTime.ParseExact(xElement.Attribute(Properties.ResourceFR.EndDate_XMI).Value, "dd/MM/yyyy", CultureInfo.InvariantCulture);
        EntryLimitDate = DateTime.ParseExact(xElement.Attribute(Properties.ResourceFR.EntryLimitDate_XMI).Value, "dd/MM/yyyy", CultureInfo.InvariantCulture);
        if (string.IsNullOrEmpty(xElement.Attribute(Properties.ResourceFR.BeachType_XMI).Value))
        {
            BeachType = null;
        }
        else
        {
            BeachType = (BeachType)Enum.Parse(typeof(BeachType), xElement.Attribute(Properties.ResourceFR.BeachType_XMI).Value);
        }

        if (string.IsNullOrEmpty(xElement.Attribute(Properties.ResourceFR.SwimType_XMI).Value))
        {
            SwimType = null;
        }
        else
        {
            SwimType = (SwimType)Enum.Parse(typeof(SwimType), xElement.Attribute(Properties.ResourceFR.SwimType_XMI).Value);
        }

        if (string.IsNullOrEmpty(xElement.Attribute(Properties.ResourceFR.Speciality_XMI).Value))
        {
            Speciality = null;
        }
        else
        {
            Speciality = (Speciality)Enum.Parse(typeof(Speciality), xElement.Attribute(Properties.ResourceFR.Speciality_XMI).Value);
        }

        if (string.IsNullOrEmpty(xElement.Attribute(Properties.ResourceFR.ChronoType_XMI).Value))
        {
            ChronoType = null;
        }
        else
        {
            ChronoType = (ChronoType)Enum.Parse(typeof(ChronoType), xElement.Attribute(Properties.ResourceFR.ChronoType_XMI).Value);
        }
        IsEligibleToNationalRecord = bool.Parse(xElement.Attribute(Properties.ResourceFR.IsEligibleToNationalRecord_XMI).Value);
        PriceByAthlete = int.Parse(xElement.Attribute(Properties.ResourceFR.PriceByAthlete_XMI).Value);
        PriceByEntry = int.Parse(xElement.Attribute(Properties.ResourceFR.PriceByEntry_XMI).Value);
        PriceByClub = int.Parse(xElement.Attribute(Properties.ResourceFR.PriceByClub_XMI).Value);
        Organizer = xElement.Attribute(Properties.ResourceFR.Organizer_XMI).Value;
    }

    public Competition(JToken data)
    {
        this.Id = data["Id"].Value<int>();
        this.Name = TextHelper.CleanedText(TextHelper.RemoveDiacritics(data["Nom"].Value<string>())).Trim();
        this.Description = data["Description"].Value<string>();
        this.Location = data["Lieu"].Value<string>();
        this.PriceByAthlete = data["TarifParAthlete"].Value<int>();
        this.PriceByEntry = data["TarifParEngagement"].Value<int>();
        this.PriceByClub = data["TarifParClub"].Value<int>();

        DateTime beginDate;
        DateTime.TryParse(data["Debut"].Value<String>(), out beginDate);
        this.BeginDate = beginDate;
        DateTime endDate;
        DateTime.TryParse(data["Fin"].Value<String>(), out endDate);
        this.EndDate = endDate;
        DateTime entryLimitDate;
        DateTime.TryParse(data["DebutEngagement"].Value<String>(), out entryLimitDate);
        this.EntryLimitDate = entryLimitDate;

        this.Speciality = JsonHelper.GetSpecialityFromJsonValue(data["specialiteLabel"].Value<string>());
        this.BeachType = JsonHelper.GetBeachTypeFromJsonValue(data["specialiteLabel"].Value<string>(), data["water"].Value<string>());
        this.SwimType = JsonHelper.GetSwimTypeFromJsonValue(data["specialiteLabel"].Value<string>(), data["bassin"].Value<string>());
        this.ChronoType = JsonHelper.GetChronoTypeFromJsonValue(data["chronoLabel"].Value<string>());

        this.IsEligibleToNationalRecord = data["isEligibleToNationalRecord"].Value<bool>();

        this.Organizer = data["Organisme"]["NomOrga"].Value<string>();
    }


    public XElement WriteXml()
    {
        XElement xElement = new XElement(Properties.ResourceFR.Competition_XMI,
                                new XAttribute(Properties.ResourceFR.Id_XMI, Id),
                                //new XAttribute(Properties.ResourceFR.IdApi_XMI, IdApi),
                                new XAttribute(Properties.ResourceFR.Name_XMI, Name),
                                new XAttribute(Properties.ResourceFR.Description_XMI, Description.Replace("\\r\\n", Environment.NewLine)),
                                new XAttribute(Properties.ResourceFR.Location_XMI, Location),
                                new XAttribute(Properties.ResourceFR.BeginDate_XMI, BeginDate.ToShortDateString()),
                                new XAttribute(Properties.ResourceFR.EndDate_XMI, EndDate.ToShortDateString()),
                                new XAttribute(Properties.ResourceFR.EntryLimitDate_XMI, EntryLimitDate.ToShortDateString()),
                                new XAttribute(Properties.ResourceFR.BeachType_XMI, BeachType.ToString()),
                                new XAttribute(Properties.ResourceFR.SwimType_XMI, SwimType.ToString()),
                                new XAttribute(Properties.ResourceFR.Speciality_XMI, Speciality.ToString()),
                                new XAttribute(Properties.ResourceFR.ChronoType_XMI, ChronoType.ToString()),
                                new XAttribute(Properties.ResourceFR.IsEligibleToNationalRecord_XMI, IsEligibleToNationalRecord),
                                new XAttribute(Properties.ResourceFR.PriceByAthlete_XMI, PriceByAthlete),
                                new XAttribute(Properties.ResourceFR.PriceByEntry_XMI, PriceByEntry),
                                new XAttribute(Properties.ResourceFR.PriceByClub_XMI, PriceByClub),
                                new XAttribute(Properties.ResourceFR.Organizer_XMI, Organizer)
                            );
        return xElement;
    }
}
