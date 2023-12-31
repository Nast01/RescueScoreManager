using System.Globalization;
using System.Xml.Linq;

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

    //one-to-many relationship to Category
    public int CategoryId { get; set; } // Required foreign key property
    public Category Category { get; set; } = null!; // Required reference navigation to principal
    #endregion Attributes

    #region Constructor
    public Athlete()
    {
        IsForfeit = false;
        OrderNumber = 0;
    }

    public Athlete(JToken data)
    {
        Id = data["NumeroLicence"].Value<String>();
        FirstName = data["Prenom"].Value<String>();
        LastName = data["Nom"].Value<String>();
        BirthYear = data["Annee"].Value<int>();
        Gender = JsonHelper.GetGenderFromJsonValue(data["Sexe"].Value<String>());
        IsLicensee = data["isLicencie"].Value<bool>();
        IsGuest = data["isInvite"].Value<bool>();
        IsForfeit = false;
        OrderNumber = 0;
    }

    public Athlete(XElement xElement,List<Category> categories)
    {
        Id = xElement.Attribute(Properties.ResourceFR.Id_XMI).Value;
        LastName = xElement.Attribute(Properties.ResourceFR.LastName_XMI).Value;
        FirstName = xElement.Attribute(Properties.ResourceFR.FirstName_XMI).Value;
        BirthYear = int.Parse(xElement.Attribute(Properties.ResourceFR.BirthYear_XMI).Value);
        Gender = (Gender)Enum.Parse(typeof(Gender), xElement.Attribute(Properties.ResourceFR.Gender_XMI).Value);
        IsLicensee = bool.Parse(xElement.Attribute(Properties.ResourceFR.IsLicensee_XMI).Value);
        IsGuest = bool.Parse(xElement.Attribute(Properties.ResourceFR.IsGuest_XMI).Value);
        Nationality = xElement.Attribute(Properties.ResourceFR.Nationality_XMI).Value;
        IsForfeit = bool.Parse(xElement.Attribute(Properties.ResourceFR.IsForfeit_XMI).Value);
        OrderNumber = int.Parse(xElement.Attribute(Properties.ResourceFR.OrderNumber_XMI).Value);
        CategoryId = int.Parse(xElement.Attribute(Properties.ResourceFR.Category_XMI).Value);
        Category = categories.Find(cat => cat.Id == CategoryId);
    }
    #endregion Constructor

    public override XElement WriteXml()
    {
        XElement xElement = new XElement(Properties.ResourceFR.Athlete_XMI,
                                    new XAttribute(Properties.ResourceFR.Id_XMI, Id),
                                    new XAttribute(Properties.ResourceFR.LastName_XMI, LastName),
                                    new XAttribute(Properties.ResourceFR.FirstName_XMI, FirstName),
                                    new XAttribute(Properties.ResourceFR.BirthYear_XMI, BirthYear),
                                    new XAttribute(Properties.ResourceFR.Gender_XMI, Gender.ToString()),
                                    new XAttribute(Properties.ResourceFR.IsLicensee_XMI, IsLicensee),
                                    new XAttribute(Properties.ResourceFR.IsGuest_XMI, IsGuest),
                                    new XAttribute(Properties.ResourceFR.Nationality_XMI, Nationality),
                                    new XAttribute(Properties.ResourceFR.IsForfeit_XMI, IsForfeit),
                                    new XAttribute(Properties.ResourceFR.OrderNumber_XMI, OrderNumber),
                                    new XAttribute(Properties.ResourceFR.Category_XMI, Category.Id)
                                    //new XAttribute(Properties.ResourceFR.IdApi_XMI, IdApi)
                                );
        return xElement;
    }
}
