using System.Xml.Linq;

using DocumentFormat.OpenXml.Wordprocessing;

using Newtonsoft.Json.Linq;

using RescueScoreManager.Helper;

using static RescueScoreManager.Data.EnumRSM;

namespace RescueScoreManager.Data;

public class RaceFormatConfiguration
{
    public int Id { get; set; }
    public string Label { get; set; }
    public string FullLabel { get; set; }
    public Gender Gender { get; set; }
    public string GenderLabel => Gender.ToString();
    public List<Category> Categories { get; set; } = new List<Category>();
    public int Discipline { get; set; }
    public string DisciplineLabel { get; set; } = "";
    public List<RaceFormatDetail> RaceFormatDetails { get; set; } = new List<RaceFormatDetail>();
    public List<Race> Races { get; set; } = new List<Race>();

    public RaceFormatConfiguration()
    {
        // Parameterless constructor for manual creation
    }

    public RaceFormatConfiguration(JToken jData, List<Category> categories)
    {
        Id = jData["Id"].Value<int>();
        Label = jData["label"].Value<string>();
        FullLabel = jData["fullLabel"].Value<string>();
        Gender = JsonHelper.GetGenderFromJsonValue(jData["genreLabel"].Value<string>());
        Discipline = jData["Discipline"]["id"].Value<int>();
        DisciplineLabel = jData["Discipline"]["Nom"].Value<string>();

        JArray JCategories = jData["categories"] as JArray;
        foreach (var jCat in JCategories.Children())
        {
            Category cat = categories.Find(c => c.Id == jCat["IdCategorie"].Value<int>());
            AddCategory(cat);
            cat.RaceFormatConfigurations.Add(this);
        }

        JArray JRaceFormatDetails = jData["parties"] as JArray;
        foreach (var jRFD in JRaceFormatDetails.Children())
        {
            RaceFormatDetail raceFormatDetail = new RaceFormatDetail(jRFD, this);
            if(raceFormatDetail != null)
            {
                RaceFormatDetails.Add(raceFormatDetail);
            }
        }
    }
    public RaceFormatConfiguration(XElement xElement, List<Category> categories, List<Race> races)
    {

        Id = int.Parse(xElement.Attribute(Properties.Resources.Id_XMI).Value);
        Label = xElement.Attribute(Properties.Resources.Label_XMI).Value;
        FullLabel = xElement.Attribute(Properties.Resources.FullLabel_XMI).Value;
        Gender = (Gender)Enum.Parse(typeof(Gender), xElement.Attribute(Properties.Resources.Gender_XMI).Value);
        Discipline = int.Parse(xElement.Attribute(Properties.Resources.Discipline_XMI).Value);
        DisciplineLabel = xElement.Attribute(Properties.Resources.DisciplineLabel_XMI)?.Value ?? "";

        // Load categories
        if (xElement.Attribute(Properties.Resources.Categories_XMI) != null)
        {
            string[] catIds = xElement.Attribute(Properties.Resources.Categories_XMI).Value.Split(" ");
            foreach (string catId in catIds)
            {
                Category cat = categories.Find(c => c.Id == int.Parse(catId));
                if (cat != null)
                {
                    Categories.Add(cat);
                }
            }
        }

        // Load races
        if (xElement.Attribute(Properties.Resources.Races_XMI) != null)
        {
            string[] raceIds = xElement.Attribute(Properties.Resources.Races_XMI).Value.Split(" ");
            foreach (string raceId in raceIds)
            {
                if (!string.IsNullOrEmpty(raceId))
                {
                    Race race = races.Find(r => r.Id == int.Parse(raceId));
                    if (race != null)
                    {
                        Races.Add(race);
                    }
                }
            }
        }

        // Update DisciplineLabel from races if available
        UpdateDisciplineLabel();

        // Load RaceFormatDetails from child elements
        foreach (XElement rfdElement in xElement.Elements(Properties.Resources.RaceFormatDetail_XMI))
        {
            RaceFormatDetail raceFormatDetail = new RaceFormatDetail(rfdElement, races);
            raceFormatDetail.RaceFormatConfiguration = this;
            raceFormatDetail.UpdateRacesFromParent();
            RaceFormatDetails.Add(raceFormatDetail);
        }
    }

    #region Public Method
    public bool AddCategory(Category category)
    {
        bool success = false;

        if (!Categories.Contains(category))
        {
            Categories.Add(category);
            success = true;
        }

        return success;
    }
    
    public void UpdateDisciplineLabel()
    {
        if (Discipline > 0 && Races.Any())
        {
            Race? firstRaceWithSameDiscipline = Races.FirstOrDefault(r => r.Discipline == Discipline);
            if (firstRaceWithSameDiscipline != null)
            {
                DisciplineLabel = firstRaceWithSameDiscipline.Name;
            }
        }
    }
    
    public XElement WriteXml()
    {
        string catIds = string.Empty;
        foreach (Category category in Categories)
        {
            catIds += category.Id + " ";
        }
        catIds = catIds.Trim();

        string raceIds = string.Empty;
        foreach (Race race in Races)
        {
            raceIds += race.Id + " ";
        }
        raceIds = raceIds.Trim();

        XElement xElement = new XElement(Properties.Resources.RaceFormatConfiguration_XMI,
                            new XAttribute(Properties.Resources.Id_XMI, Id),
                            new XAttribute(Properties.Resources.Label_XMI, Label),
                            new XAttribute(Properties.Resources.FullLabel_XMI, FullLabel),
                            new XAttribute(Properties.Resources.Gender_XMI, Gender.ToString()),
                            new XAttribute(Properties.Resources.Discipline_XMI, Discipline),
                            new XAttribute(Properties.Resources.DisciplineLabel_XMI, DisciplineLabel),
                            new XAttribute(Properties.Resources.Categories_XMI, catIds),
                            new XAttribute(Properties.Resources.Races_XMI, raceIds)
                            );

        foreach (RaceFormatDetail rfd in RaceFormatDetails)
        {
            xElement.Add(rfd.WriteXml());
        }

        return xElement;
    }
    #endregion Public Method
}
