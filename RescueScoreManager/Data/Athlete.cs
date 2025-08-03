using System.Globalization;
using System.Linq;
using System.Xml.Linq;

using CommunityToolkit.Mvvm.ComponentModel;

using Newtonsoft.Json.Linq;

using RescueScoreManager.Helper;

using static RescueScoreManager.Data.EnumRSM;

namespace RescueScoreManager.Data;

public partial class Athlete : Licensee
{
    #region Attributes
    public bool IsForfeit { get; set; }
    public int OrderNumber { get; set; }

    public ICollection<IndividualTeam> IndividualTeams { get; } = new List<IndividualTeam>();
    public ICollection<RelayTeam> RelayTeams { get; } = new List<RelayTeam>();
    /// <summary>
    /// Gets all teams (both Individual and Relay) that this athlete participates in.
    /// This is a computed property that concatenates IndividualTeams and RelayTeams.
    /// </summary>
    public IEnumerable<Team> Teams
    {
        get
        {
            return IndividualTeams.Cast<Team>().Concat(RelayTeams.Cast<Team>());
        }
    }

    /// <summary>
    /// Gets the total number of teams this athlete participates in.
    /// </summary>
    public int TotalTeamsNumber => IndividualTeams.Count + RelayTeams.Count;

    /// <summary>
    /// Checks if the athlete participates in any teams.
    /// </summary>
    public bool HasTeams => IndividualTeams.Any() || RelayTeams.Any();
    // Forfeit status indicators
    public int ForfeitedTeamsNumber => Teams.Count(t => t.IsForfeit);
    public int ActiveTeamsNumber => Teams.Count(t => !t.IsForfeit);

    public bool HasForfeitedTeams => ForfeitedTeamsNumber > 0;
    public bool HasAllTeamsForfeited => TotalTeamsNumber > 0 && ForfeitedTeamsNumber == TotalTeamsNumber;
    public bool HasPartialForfeit => ForfeitedTeamsNumber > 0 && ForfeitedTeamsNumber < TotalTeamsNumber;

    public string GetForfeitStatusText()
    {
            return TotalTeamsNumber switch
            {
                0 => "No teams",
                _ when HasAllTeamsForfeited => "All teams forfeited",
                _ when HasPartialForfeit => $"{ForfeitedTeamsNumber}/{TotalTeamsNumber} teams forfeited",
                _ => "All teams active"
            };
    }

    #endregion Attributes

    #region Constructor
    public Athlete()
    {
        IsForfeit = false;
        OrderNumber = 0;
    }

    public Athlete(JToken data)
    {
        Id = data["Id"].Value<int>();
        FirstName = data["Prenom"].Value<string>();
        LastName = data["Nom"].Value<string>();
        IsLicensee = data["isLicencie"].Value<bool>();
        IsGuest = data["isInvite"].Value<bool>();
        LicenseeNumber = data["NumeroLicence"].Value<string>();
        Gender = JsonHelper.GetGenderFromJsonValue(data["Sexe"].Value<string>());
        BirthYear = data["Annee"].Value<int>();
        Nationality = data["nationaliteLabel"].Value<string>();
        NationalityCode = data["nationaliteLabel"].Value<string>();
        IsValid = data["isValid"].Value<bool>();

        //Id = IsLicensee? data["NumeroLicence"].Value<String>() : data["idInvite"].Value<String>() ;
        IsForfeit = false;
        OrderNumber = 0;
    }

    public Athlete(XElement xElement)
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
        NationalityCode = xElement.Attribute(Properties.Resources.NationalityCode_XMI).Value;
        IsForfeit = bool.Parse(xElement.Attribute(Properties.Resources.IsForfeit_XMI).Value);
        OrderNumber = int.Parse(xElement.Attribute(Properties.Resources.OrderNumber_XMI).Value);
        IsValid = bool.Parse(xElement.Attribute(Properties.Resources.IsValid_XMI).Value);
    }
    #endregion Constructor

    public override XElement WriteXml()
    {
        XElement xElement = new XElement(Properties.Resources.Athlete_XMI,
                                    new XAttribute(Properties.Resources.Id_XMI, Id),
                                    new XAttribute(Properties.Resources.LicenseeNumber_XMI, LicenseeNumber),
                                    new XAttribute(Properties.Resources.LastName_XMI, LastName),
                                    new XAttribute(Properties.Resources.FirstName_XMI, FirstName),
                                    new XAttribute(Properties.Resources.BirthYear_XMI, BirthYear),
                                    new XAttribute(Properties.Resources.Gender_XMI, Gender.ToString()),
                                    new XAttribute(Properties.Resources.IsLicensee_XMI, IsLicensee),
                                    new XAttribute(Properties.Resources.IsGuest_XMI, IsGuest),
                                    new XAttribute(Properties.Resources.Nationality_XMI, Nationality),
                                    new XAttribute(Properties.Resources.NationalityCode_XMI, NationalityCode),
                                    new XAttribute(Properties.Resources.IsForfeit_XMI, IsForfeit),
                                    new XAttribute(Properties.Resources.OrderNumber_XMI, OrderNumber),
                                    new XAttribute(Properties.Resources.IsValid_XMI, IsValid)
                                //new XAttribute(Properties.Resources.IdApi_XMI, IdApi)
                                );
        return xElement;
    }
}
