using System.Xml.Linq;

using Newtonsoft.Json.Linq;

using RescueScoreManager.Helper;

using static RescueScoreManager.Data.EnumRSM;

namespace RescueScoreManager.Data;

public partial class Referee : Licensee
{
    #region Attributes
    public RefereeLevel RefereeLevel { get; set; }
    public RefereeLevel MaxRefereeLevel { get; set; }
    public List<RefereeDate> RefereeAvailabilities { get; set; } = new List<RefereeDate>();
    public bool IsPrincipal {  get; set; }
    public string RefereeAvailabilitiesLabel
    {
        get
        {
            return string.Join(" ", RefereeAvailabilities.Select(ra => ra.Availability.ToShortDateString()));
        }
    }
    //public Category Category { get; set; }
    #endregion Attributes

    #region Constructor
    public Referee()
    {

    }

    public Referee(JToken? data, DateTime beginDate)
    {
        Id = data["Id"].Value<int>();
        LicenseeNumber = data["NumeroLicence"].Value<string>();
        LastName = data["Nom"].Value<string>();
        FirstName = data["Prenom"].Value<string>();
        BirthYear = data["Annee"].Value<int>();
        Gender = JsonHelper.GetGenderFromJsonValue(data["Sexe"].Value<string>());

        IsLicensee = data["isLicencie"].Value<bool>();
        IsGuest = data["isInvite"].Value<bool>();

        RefereeLevel = (RefereeLevel)Enum.Parse(typeof(RefereeLevel), data["Niveau"].Value<string>());
        MaxRefereeLevel = (RefereeLevel)Enum.Parse(typeof(RefereeLevel), data["NiveauMax"].Value<string>());
        IsPrincipal = data["Principal"].Value<bool>();

        Nationality = data["nationaliteCode"].Value<string>();
        Nationality = data["nationaliteLabel"].Value<string>();

        //ClubId
        //Club    
        


        JArray? days = data["Jours"] as JArray;
        if (days != null)
        {
            RefereeAvailabilities = new List<RefereeDate>();
            foreach (var day in days.Children())
            {
                int dayAvailable = day.Value<int>() - 1;//get the number of day to add to the beginDate of the competition
                RefereeDate refDate = new RefereeDate() { Availability = beginDate.AddDays(dayAvailable), Referee = this };
                RefereeAvailabilities.Add(refDate);
            }
        }
    }

    public Referee(XElement xElement)
    {
        Id = int.Parse(xElement.Attribute(Properties.Resources.Id_XMI).Value);
        LicenseeNumber = xElement.Attribute(Properties.Resources.LicenseeNumber_XMI).Value;
        LastName = xElement.Attribute(Properties.Resources.LastName_XMI).Value;
        FirstName = xElement.Attribute(Properties.Resources.FirstName_XMI).Value;
        BirthYear = int.Parse(xElement.Attribute(Properties.Resources.BirthYear_XMI).Value);
        Gender = (Gender)Enum.Parse(typeof(Gender), xElement.Attribute(Properties.Resources.Gender_XMI).Value);
        IsLicensee = bool.Parse(xElement.Attribute(Properties.Resources.IsLicensee_XMI).Value);
        IsGuest = bool.Parse(xElement.Attribute(Properties.Resources.IsGuest_XMI).Value);
        Nationality = xElement.Attribute(Properties.Resources.Nationality_XMI).Value;
        RefereeLevel = (RefereeLevel)Enum.Parse(typeof(RefereeLevel), xElement.Attribute(Properties.Resources.Level_XMI).Value);

        string[] availabilities = xElement.Attribute(Properties.Resources.Availabilities_XMI).Value.Split(" ");
        foreach (string date in availabilities)
        {
            RefereeDate refereeDate = new RefereeDate();
            refereeDate.Availability = DateTime.Parse(date);
            refereeDate.Referee = this;
            RefereeAvailabilities.Add(refereeDate);
        }

    }
    #endregion Constructor

    #region Methods
    public override XElement WriteXml()
    {
        return new XElement(Properties.Resources.Referee_XMI,
                                new XAttribute(Properties.Resources.Id_XMI, Id),
                                new XAttribute(Properties.Resources.LicenseeNumber_XMI, Id),
                                new XAttribute(Properties.Resources.LastName_XMI, LastName),
                                new XAttribute(Properties.Resources.FirstName_XMI, FirstName),
                                new XAttribute(Properties.Resources.BirthYear_XMI, BirthYear),
                                new XAttribute(Properties.Resources.Gender_XMI, Gender.ToString()),
                                new XAttribute(Properties.Resources.IsLicensee_XMI, IsLicensee),
                                new XAttribute(Properties.Resources.IsGuest_XMI, IsGuest),
                                new XAttribute(Properties.Resources.Nationality_XMI, Nationality),
                                new XAttribute(Properties.Resources.Level_XMI, RefereeLevel),
                                new XAttribute(Properties.Resources.MaxLevel_XMI, MaxRefereeLevel),
                                new XAttribute(Properties.Resources.Availabilities_XMI, RefereeAvailabilitiesLabel)
                            );
    }
    #endregion  
}
