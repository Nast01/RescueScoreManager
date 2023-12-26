using System.Xml.Linq;

using Newtonsoft.Json.Linq;

using RescueScoreManager.Helper;

using static RescueScoreManager.Data.EnumRSM;

namespace RescueScoreManager.Data;

public partial class Referee : Licensee
{
    #region Attributes
    public RefereeLevel RefereeLevel { get; set; }
    public List<RefereeDate> RefereeAvailabilities { get; set; } = new List<RefereeDate>();

    //public Category Category { get; set; }
    #endregion Attributes

    #region Constructor
    public Referee()
    {

    }

    public Referee(JToken? data, DateTime beginDate)
    {
        Id = data["NumeroLicence"].Value<String>();
        LastName = data["Nom"].Value<String>();
        FirstName = data["Prenom"].Value<String>();
        BirthYear = data["Annee"].Value<int>();
        Gender = JsonHelper.GetGenderFromJsonValue(data["Sexe"].Value<String>());

        IsLicensee = data["isLicencie"].Value<bool>();
        IsGuest = data["isInvite"].Value<bool>();

        RefereeLevel = (RefereeLevel)Enum.Parse(typeof(RefereeLevel), data["NiveauMax"].Value<string>());
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
        Id = xElement.Attribute(Properties.ResourceFR.Id_XMI).Value;
        LastName = xElement.Attribute(Properties.ResourceFR.LastName_XMI).Value;
        FirstName = xElement.Attribute(Properties.ResourceFR.FirstName_XMI).Value;
        BirthYear = int.Parse(xElement.Attribute(Properties.ResourceFR.BirthYear_XMI).Value);
        Gender = (Gender)Enum.Parse(typeof(Gender), xElement.Attribute(Properties.ResourceFR.Gender_XMI).Value);
        IsLicensee = bool.Parse(xElement.Attribute(Properties.ResourceFR.IsLicensee_XMI).Value);
        IsGuest = bool.Parse(xElement.Attribute(Properties.ResourceFR.IsGuest_XMI).Value);
        Nationality = xElement.Attribute(Properties.ResourceFR.Nationality_XMI).Value;
        RefereeLevel = (RefereeLevel)Enum.Parse(typeof(RefereeLevel), xElement.Attribute(Properties.ResourceFR.Level_XMI).Value);

        string[] availabilities = xElement.Attribute(Properties.ResourceFR.Availabilities_XMI).Value.Split(" ");
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
        string availabilities = string.Empty;
        foreach (RefereeDate refereeDate in RefereeAvailabilities)
        {
            availabilities += refereeDate.Availability.ToShortDateString() + " ";
        }
        availabilities = availabilities.Trim();

        XElement xElement = new XElement(Properties.ResourceFR.Referee_XMI,
                                new XAttribute(Properties.ResourceFR.Id_XMI, Id),
                                new XAttribute(Properties.ResourceFR.LastName_XMI, LastName),
                                new XAttribute(Properties.ResourceFR.FirstName_XMI, FirstName),
                                new XAttribute(Properties.ResourceFR.BirthYear_XMI, BirthYear),
                                new XAttribute(Properties.ResourceFR.Gender_XMI, Gender.ToString()),
                                new XAttribute(Properties.ResourceFR.IsLicensee_XMI, IsLicensee),
                                new XAttribute(Properties.ResourceFR.IsGuest_XMI, IsGuest),
                                new XAttribute(Properties.ResourceFR.Nationality_XMI, Nationality),
                                new XAttribute(Properties.ResourceFR.Level_XMI, RefereeLevel),
                                new XAttribute(Properties.ResourceFR.Availabilities_XMI, availabilities)
                            );
        return xElement;
    }
    #endregion  
}
