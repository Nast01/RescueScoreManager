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
    public string DisciplineLabel { get; set; }
    public List<RaceFormatDetail> RaceFormatDetails { get; set; } = new List<RaceFormatDetail>();

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
    public RaceFormatConfiguration(XElement xElement, List<Category> categories)
    {

        Id = int.Parse(xElement.Attribute(Properties.Resources.Id_XMI).Value);
        Label = xElement.Attribute(Properties.Resources.Label_XMI).Value;
        FullLabel = xElement.Attribute(Properties.Resources.FullLabel_XMI).Value;
        Gender = (Gender)Enum.Parse(typeof(Gender), xElement.Attribute(Properties.Resources.Gender_XMI).Value);
        Discipline = int.Parse(xElement.Attribute(Properties.Resources.Discipline_XMI).Value);

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
    public XElement WriteXml()
    {
        string catIds = string.Empty;
        foreach (Category category in Categories)
        {
            catIds += category.Id + " ";
        }
        catIds = catIds.Trim();

        XElement xElement = new XElement(Properties.Resources.RaceFormatConfiguration_XMI,
                            new XAttribute(Properties.Resources.Id_XMI, Id),
                            new XAttribute(Properties.Resources.Label_XMI, Label),
                            new XAttribute(Properties.Resources.FullLabel_XMI, FullLabel),
                            new XAttribute(Properties.Resources.Gender_XMI, Gender.ToString()),
                            new XAttribute(Properties.Resources.Discipline_XMI, Discipline),
                            new XAttribute(Properties.Resources.Categories_XMI, catIds)
                            );

        foreach (RaceFormatDetail rfd in RaceFormatDetails)
        {
            xElement.Add(rfd.WriteXml());
        }

        return xElement;
    }
    #endregion Public Method
}
