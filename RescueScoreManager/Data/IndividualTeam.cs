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
        
        Athlete = lics.Find(l => l.Id == jData["athletes"][0]["NumeroLicence"].Value<string>()) as Athlete;
        Athlete.Category = categories.Find(cat => cat.Id == jData["categorie"]["Id"].Value<int>());
        Athlete.Category.Athletes.Add(Athlete);
    }

    public IndividualTeam(XElement xElement,List<Athlete> lics)
    {
        Id = int.Parse(xElement.Attribute(Properties.ResourceFR.Id_XMI).Value);
        IsForfeit = bool.Parse(xElement.Attribute(Properties.ResourceFR.IsForfeit_XMI).Value);
        IsForfeitFinal = bool.Parse(xElement.Attribute(Properties.ResourceFR.IsForfeitFinal_XMI).Value);
        EntryTime = int.Parse(xElement.Attribute(Properties.ResourceFR.EntryTime_XMI).Value);
        Number = 1;

        string athId = xElement.Attribute(Properties.ResourceFR.Athletes_XMI).Value;
        Athlete = lics.Find(l => l.Id == athId);
    }

    #endregion Constructor
    #region Method
    public override XElement WriteXml()
    {
        return new XElement(Properties.ResourceFR.Team_XMI,
                                new XAttribute(Properties.ResourceFR.Id_XMI, Id),
                                new XAttribute(Properties.ResourceFR.IsForfeit_XMI, IsForfeit),
                                new XAttribute(Properties.ResourceFR.IsForfeitFinal_XMI, IsForfeitFinal),
                                new XAttribute(Properties.ResourceFR.EntryTime_XMI, EntryTime),
                                //new XAttribute(Properties.ResourceFR.IdApi_XMI, IdApi),
                                new XAttribute(Properties.ResourceFR.Athlete_XMI, Athlete.Id)
                            );
    }
    #endregion
}
