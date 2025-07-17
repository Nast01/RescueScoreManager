using System.ComponentModel;
using System.Xml.Linq;

using Newtonsoft.Json.Linq;

using RescueScoreManager.Helper;

namespace RescueScoreManager.Data;

public partial class IndividualTeam : Team
{
    public string AthleteId { get; set; } // Required foreign key property
    public Athlete Athlete { get; set; } = null!; // Required reference navigation to principal

    #region Constructor
    public IndividualTeam()
    {

    }

    public IndividualTeam(JToken jData, Race race, List<Licensee> lics,List<Category> categories)
    {
        Id = jData["Id"].Value<int>();
        EntryTime = jData["performance"].Value<int>();
        IsForfeit = false;
        IsForfeitFinal = false;
        Race = race;
        Number = race.NumberByTeam;

        string licNumber = jData["athletes"][0]["NumeroLicence"].Value<string>() == "INV" ? jData["athletes"][0]["idInvite"].Value<string>() : jData["athletes"][0]["NumeroLicence"].Value<string>();
        //Athlete = lics.Find(l => l.Id == licNumber) as Athlete;
        Category category = categories.Find(cat => cat.Id == jData["categorie"]["Id"].Value<int>());
        Athlete.Category = category; 
        Athlete.CategoryId = category.Id;
        if (category.Athletes.Contains(Athlete) == false)
            category.Athletes.Add(Athlete);

        Athlete.IndividualTeams.Add(this);
    }

    public IndividualTeam(XElement xElement,List<Athlete> lics)
    {
        Id = int.Parse(xElement.Attribute(Properties.Resources.Id_XMI).Value);
        IsForfeit = bool.Parse(xElement.Attribute(Properties.Resources.IsForfeit_XMI).Value);
        IsForfeitFinal = bool.Parse(xElement.Attribute(Properties.Resources.IsForfeitFinal_XMI).Value);
        EntryTime = int.Parse(xElement.Attribute(Properties.Resources.EntryTime_XMI).Value);
        Number = 1;

        AthleteId = xElement.Attribute(Properties.Resources.Athlete_XMI).Value;
        //Athlete = lics.Find(l => l.Id == AthleteId);
        Athlete.IndividualTeams.Add(this);   
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
                                new XAttribute(Properties.Resources.Athlete_XMI, Athlete.Id),
                                new XAttribute(Properties.Resources.Race_XMI, Race.Id)
                            );
    }
    #endregion
}
