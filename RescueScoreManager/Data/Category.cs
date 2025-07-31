using System.Xml.Linq;

using Newtonsoft.Json.Linq;

namespace RescueScoreManager.Data;

public class Category
{
    #region Attributes
    public int Id { get; set; }
    public string Name { get; set; }
    public string NameEnglish { get; set; }
    public int AgeMin { get; set; } = 0;
    public int AgeMax { get; set; } = 0;
    public ICollection<Athlete> Athletes { get; } = new List<Athlete>();//one-to-many relationship
    public ICollection<Race> Races { get; } = new List<Race>(); // Many to many relation
    public ICollection<MeetingElement> MeetingElements { get; } = new List<MeetingElement>(); // Many to many relation
    public ICollection<Round> Rounds { get; } = new List<Round>();//one-to-many relationship
    #endregion Attributes

    public Category()
    {

    }

    public Category(JToken data)
    {
        Id = data["Id"].Value<int>();
        Name = data["Nom"].Value<string>();
        NameEnglish = data["NomEn"].Value<string>();
        if (data["AgeMin"].Value<string>() != null)
        {
            AgeMin = data["AgeMin"].Value<int>();
        }
        if (data["AgeMax"].Value<string>() != null)
        {
            AgeMax = data["AgeMax"].Value<int>();
        }
    }

    public Category(XElement xElement)
    {
        Id = int.Parse(xElement.Attribute(Properties.Resources.Id_XMI).Value);
        Name = xElement.Attribute(Properties.Resources.Name_XMI).Value;
        NameEnglish = xElement.Attribute(Properties.Resources.NameEnglish_XMI).Value;
        AgeMin = int.Parse(xElement.Attribute(Properties.Resources.AgeMin_XMI).Value);
        AgeMax = int.Parse(xElement.Attribute(Properties.Resources.AgeMax_XMI).Value);
    }

    public XElement WriteXml()
    {
        return new XElement(Properties.Resources.Category_XMI,
                            new XAttribute(Properties.Resources.Id_XMI, Id),
                            new XAttribute(Properties.Resources.Name_XMI, Name),
                            new XAttribute(Properties.Resources.NameEnglish_XMI, Name),
                            new XAttribute(Properties.Resources.AgeMin_XMI, AgeMin),
                            new XAttribute(Properties.Resources.AgeMax_XMI, AgeMax)
                            );
    }
}
