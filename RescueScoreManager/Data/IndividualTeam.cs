using System.ComponentModel;
using System.Xml.Linq;

using Newtonsoft.Json.Linq;

using RescueScoreManager.Helper;

namespace RescueScoreManager.Data;

public partial class IndividualTeam : Team
{
    public int AthleteId { get; set; } // Required foreign key property
    public Athlete Athlete { get; set; } = null!; // Required reference navigation to principal

    #region Constructor
    public IndividualTeam()
    {

    }

    public IndividualTeam(JToken jData, Race race, List<Athlete> lics,List<Category> categories)
    {
        Id = jData["Id"].Value<int>();
        EntryTime = jData["performance"].Value<int>();
        IsForfeit = false;
        IsForfeitFinal = false;
        Race = race;
        Number = race.NumberByTeam;

        string licNumber = jData["athletes"]?[0]?["NumeroLicence"]?.Value<string>() == "INV" ? jData["athletes"]?[0]?["idInvite"]?.Value<string>() ?? string.Empty : jData["athletes"]?[0]?["NumeroLicence"]?.Value<string>() ?? string.Empty;
        Athlete = lics.Find(l => l.LicenseeNumber == licNumber) ?? throw new InvalidOperationException("Athlete not found");
        Athlete.IndividualTeams.Add(this);

        Category = categories.Find(cat => cat.Id == jData["categorie"]["Id"].Value<int>());

        Status = jData["Statut"].Value<int>();
        StatusLabel = jData["statutLabel"]?.Value<string>() ?? string.Empty;

        TeamLabel = Athlete.FullName!;
    }

    public IndividualTeam(XElement xElement,List<Athlete> lics,List<Race> races, List<Category> categories)
    {
        Id = int.Parse(xElement.Attribute(Properties.Resources.Id_XMI).Value);
        IsForfeit = bool.Parse(xElement.Attribute(Properties.Resources.IsForfeit_XMI).Value);
        IsForfeitFinal = bool.Parse(xElement.Attribute(Properties.Resources.IsForfeitFinal_XMI).Value);
        EntryTime = int.Parse(xElement.Attribute(Properties.Resources.EntryTime_XMI).Value);
        Number = 1;

        AthleteId = int.Parse(xElement.Attribute(Properties.Resources.Athlete_XMI)?.Value ?? "0");
        Athlete = lics.Find(l => l.Id == AthleteId) ?? throw new InvalidOperationException("Athlete not found");
        Athlete.IndividualTeams.Add(this);

        RaceId = int.Parse(xElement.Attribute(Properties.Resources.Race_XMI)?.Value ?? "0");
        Race = races.Find(r => r.Id == RaceId) ?? throw new InvalidOperationException("Race not found");
        Race.Teams.Add(this);

        int categoryId = int.Parse(xElement.Attribute(Properties.Resources.Category_XMI).Value);
        Category = categories.Find(c => c.Id == categoryId) ?? throw new InvalidOperationException("Category not found");

        Status = int.Parse(xElement.Attribute(Properties.Resources.Status_XMI).Value);
        StatusLabel = xElement.Attribute(Properties.Resources.StatusLabel_XMI).Value;

        TeamLabel = Athlete.FullName!;
    }

    #endregion Constructor
    #region Method
    public override XElement WriteXml()
    {
        return new XElement(Properties.Resources.IndividualTeam_XMI,
                                new XAttribute(Properties.Resources.Id_XMI, Id),
                                new XAttribute(Properties.Resources.IsForfeit_XMI, IsForfeit),
                                new XAttribute(Properties.Resources.IsForfeitFinal_XMI, IsForfeitFinal),
                                new XAttribute(Properties.Resources.EntryTime_XMI, EntryTime),
                                new XAttribute(Properties.Resources.Category_XMI, Category.Id),
                                new XAttribute(Properties.Resources.Athlete_XMI, Athlete.Id),
                                new XAttribute(Properties.Resources.Race_XMI, Race.Id),
                                new XAttribute(Properties.Resources.Number_XMI, Number),
                                new XAttribute(Properties.Resources.Status_XMI, Status),
                                new XAttribute(Properties.Resources.StatusLabel_XMI, StatusLabel)
                            );
    }
    #endregion
}
