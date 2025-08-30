using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

using Newtonsoft.Json.Linq;

using RescueScoreManager.Helper;
using RescueScoreManager.Services;

using static System.Runtime.InteropServices.JavaScript.JSType;
using static RescueScoreManager.Data.EnumRSM;

namespace RescueScoreManager.Data;

public class Race
{
    #region Attributes
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Label => Categories.Count == 1 ? $"{Name} {Categories.FirstOrDefault().Name} {EnumRSM.GetEnumDescription(Gender)}" : $"{Name} {EnumRSM.GetEnumDescription(Gender)}";

    public Gender Gender { get; set; }
    public string GenderLabel => Gender switch
    {
        Gender.Woman => ResourceManagerLocalizationService.Instance.GetString("Women") ?? "Women",
        Gender.Mixte => ResourceManagerLocalizationService.Instance.GetString("Mixte") ?? "Mixte",
        Gender.Men or _ => ResourceManagerLocalizationService.Instance.GetString("Men") ?? "Men"
    };
    public Speciality Speciality { get; set; }
    public int Discipline { get; set; }
    public string DisciplineLabel => $"{Name} {GenderLabel}";

    public int NumberByTeam { get; set; }
    public int? Distance { get; set; } = null;
    public int Interval { get; set; } = 9000;
    public string IntervalLabel => TimeHelper.ConvertCentisecondInString(Interval);
    public bool IsRelay { get; set; }
    public bool IsEligibleToNationalRecord { get; set; }

    public int MaxAthleteAllowed { get; set; }
    public bool CanExceedMaxAlthleteAllowed { get; set; }
    public bool IsFinalBAllowed { get; set; }

    //one-to-many relationship to Competition
    public int CompetitionId { get; set; } // Required foreign key property
    public Competition Competition { get; set; } = null!; // Required reference navigation to principal
    public ICollection<Category> Categories { get; } = new List<Category>();
    public ICollection<Team> Teams { get; } = new List<Team>();
    public ICollection<MeetingElement> MeetingElements { get; } = new List<MeetingElement>();
    #endregion Attributes

    #region Constructor
    public Race()
    {
    }

    public Race(JToken jData, List<Category> categories)
    {
        Id = jData["Id"].Value<int>();
        Discipline = jData["IdDiscipline"].Value<int>();
        Name = jData["discipline"]["Nom"].Value<string>();
        Gender = JsonHelper.GetGenderFromJsonValue(jData["Genre"].Value<string>());
        Speciality = JsonHelper.GetSpecialityFromJsonValue(jData["discipline"]["specialiteLabel"].Value<string>());
        NumberByTeam = jData["discipline"]["NbAthleteParEquipe"].Value<int>();
        IsEligibleToNationalRecord = jData["isEligibleToNationalRecord"].Value<bool>();
        IsRelay = NumberByTeam > 1;
        Distance = Speciality == Speciality.EauPlate ? jData["discipline"]["Distance"].Value<int>() : 0;

        //public static List<string> single16FinalRaces = new List<string>() { };//{ "E1-Beach Flags", "E2-Beach Flags" };//
        //public static List<string> double10FinalRaces = new List<string>() { "E1-Beach Flags", "E2-Beach Flags" }; //

        switch (Discipline)
        {
            // final A max 16 Exceeding
            case 1://R.Planche
            case 3://surfski
            case 4://Course - Nage - Course
            case 7://oceanwoman - oceanman
            case 13://paddle board
            case 27:// R Combiné
            case 34://Mini Combiné
            case 44://Relais Surfski
                MaxAthleteAllowed = 16;
                CanExceedMaxAlthleteAllowed = true;
                IsFinalBAllowed = false;
                break;
            // final A max 32 Exceeding
            case 5://Nage
            case 43://Relais Nage
                MaxAthleteAllowed = 32;
                CanExceedMaxAlthleteAllowed = true;
                IsFinalBAllowed = false;
                break;
            // final A B max 9 No Exceeding
            case 26://Relais Sauvetage tube
            case 41://Relais Sauvetage Planche
                MaxAthleteAllowed = 9;
                CanExceedMaxAlthleteAllowed = false;
                IsFinalBAllowed = true;
                break;
            // final A B max 10 Exceeding
            case 2://Relais Sprint
            case 9://90m Sprint
                MaxAthleteAllowed = 10;
                CanExceedMaxAlthleteAllowed = true;
                IsFinalBAllowed = true;
                break;
            // final A B max 10 Exceeding
            case 8://Beach Flags
                MaxAthleteAllowed = 16;
                CanExceedMaxAlthleteAllowed = true;
                IsFinalBAllowed = true;
                break;
            // final A max 40 Exceeding
            case 6://2km Sprint
            case 35://1km Sprint
                MaxAthleteAllowed = 40;
                CanExceedMaxAlthleteAllowed = true;
                IsFinalBAllowed = false;
                break;
            default:
                break;
        }

        JArray JCategories = jData["categories"] as JArray;
        foreach (var jCat in JCategories.Children())
        {
            Category cat = categories.Find(c => c.Id == jCat["Id"].Value<int>());
            AddCategory(cat);
            cat.Races.Add(this);
        }
    }
    public Race(XElement xElement, List<Category> categories)
    {
        Id = int.Parse(xElement.Attribute(Properties.Resources.Id_XMI).Value);
        Name = xElement.Attribute(Properties.Resources.Name_XMI).Value;
        Gender = (Gender)Enum.Parse(typeof(Gender), xElement.Attribute(Properties.Resources.Gender_XMI).Value);
        Speciality = (Speciality)Enum.Parse(typeof(Speciality), xElement.Attribute(Properties.Resources.Speciality_XMI).Value);
        Discipline = int.Parse(xElement.Attribute(Properties.Resources.Discipline_XMI).Value);
        NumberByTeam = int.Parse(xElement.Attribute(Properties.Resources.NumberByTeam_XMI).Value);
        Distance = int.Parse(xElement.Attribute(Properties.Resources.Distance_XMI).Value);
        Interval = int.Parse(xElement.Attribute(Properties.Resources.Interval_XMI).Value);
        IsRelay = bool.Parse(xElement.Attribute(Properties.Resources.IsRelay_XMI).Value);
        IsEligibleToNationalRecord = bool.Parse(xElement.Attribute(Properties.Resources.IsEligibleToNationalRecord_XMI).Value);

        MaxAthleteAllowed = int.Parse(xElement.Attribute(Properties.Resources.MaxAthleteAllowed_XMI).Value);
        CanExceedMaxAlthleteAllowed = bool.Parse(xElement.Attribute(Properties.Resources.CanExceedMaxAlthleteAllowed_XMI).Value);
        IsFinalBAllowed = bool.Parse(xElement.Attribute(Properties.Resources.IsFinalBAllowed_XMI).Value);

        string[] catIds = xElement.Attribute(Properties.Resources.Categories_XMI).Value.Split(" ");
        foreach (string catId in catIds)
        {
            Category cat = categories.Find(c => c.Id == int.Parse(catId));
            if (cat != null)
            {
                Categories.Add(cat);
            }
        }


        //ShortName = xElement.Attribute(Properties.Resources.ShortName_XMI).Value;
        //if (xElement.Attribute(Properties.Resources.AresStyleId_XMI) != null)
        //{
        //    AresStyleId = int.Parse(xElement.Attribute(Properties.Resources.AresStyleId_XMI).Value);
        //}
        //else
        //{
        //    AresStyleId = -1;
        //}
        //Categories = new Categories();
        //EntryTeams = new Teams();
        //MeetingElements = new MeetingElements();
    }

    #endregion Constructor

    #region Methods
    public XElement WriteXml()
    {
        string catIds = string.Empty;
        foreach (Category category in Categories)
        {
            catIds += category.Id + " ";
        }
        catIds = catIds.Trim();

        XElement xElement = new XElement(Properties.Resources.Race_XMI,
                            new XAttribute(Properties.Resources.Id_XMI, Id),
                            new XAttribute(Properties.Resources.Name_XMI, Name),
                            new XAttribute(Properties.Resources.Gender_XMI, Gender.ToString()),
                            new XAttribute(Properties.Resources.Speciality_XMI, Speciality.ToString()),
                            new XAttribute(Properties.Resources.Discipline_XMI, Discipline),
                            new XAttribute(Properties.Resources.NumberByTeam_XMI, NumberByTeam),
                            new XAttribute(Properties.Resources.Distance_XMI, Distance!),
                            new XAttribute(Properties.Resources.Interval_XMI, Interval),
                            new XAttribute(Properties.Resources.IsRelay_XMI, IsRelay),
                            new XAttribute(Properties.Resources.IsEligibleToNationalRecord_XMI, IsEligibleToNationalRecord),
                            new XAttribute(Properties.Resources.MaxAthleteAllowed_XMI, MaxAthleteAllowed),
                            new XAttribute(Properties.Resources.CanExceedMaxAlthleteAllowed_XMI, CanExceedMaxAlthleteAllowed),
                            new XAttribute(Properties.Resources.IsFinalBAllowed_XMI, IsFinalBAllowed),
                            new XAttribute(Properties.Resources.Categories_XMI, catIds)
                            //new XAttribute(Properties.Resources.MeetingElements_XMI, MeetingElements.GetXmiIds()),
                            //new XAttribute(Properties.Resources.AresStyleId_XMI, AresStyleId)
                            );

        foreach (Team team in Teams)
        {
            xElement.Add(team.WriteXml());
        }

        return xElement;
    }
    #region Export

    #endregion Export

    #region Categories Methods
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

    public bool AddCategories(List<Category> categories)
    {
        bool success = false;

        bool isNotContained = categories.Except(Categories).Any();

        if (categories.Count > 0 && isNotContained)
        {
            Categories.ToList<Category>().AddRange(categories);
            success = true;
        }
        return success;
    }

    public bool RemoveCategory(Category category)
    {
        bool success = false;
        if (category != null && Categories.Contains(category))
        {
            success = Categories.Remove(category);
        }
        return success;
    }

    public bool RemoveCategories(List<Category> categoriesTobeRemoved)
    {
        bool success = false;

        if (categoriesTobeRemoved.Count > 0)
        {
            int removedCategoriesTobeRemoved = Categories.ToList<Category>().RemoveAll(cat => categoriesTobeRemoved.Contains(cat));
            success = categoriesTobeRemoved.Count == removedCategoriesTobeRemoved;
        }
        return success;
    }
    #endregion Categories Methods
    #region Teams Methods

    public List<Team> GetAvailableTeams()
    {
        return Teams.ToList().FindAll(team => !team.IsForfeit);
    }
    public bool AddTeam(Team team)
    {
        bool success = false;

        if (!Teams.Contains(team))
        {
            Teams.Add(team);
            success = true;
        }

        return success;
    }

    public bool AddTeams(List<Team> teams)
    {
        bool success = false;

        bool isNotContained = teams.Except(Teams).Any();

        if (teams.Count > 0 && isNotContained)
        {
            Teams.ToList<Team>().AddRange(teams);
            success = true;
        }
        return success;
    }

    public bool RemoveTeam(Team team)
    {
        bool success = false;
        if (team != null && Teams.Contains(team))
        {
            success = Teams.Remove(team);
        }
        return success;
    }

    public bool RemoveTeams(List<Team> teamsTobeRemoved)
    {
        bool success = false;

        if (teamsTobeRemoved.Count > 0)
        {
            int removedTeams = Teams.ToList<Team>().RemoveAll(t => teamsTobeRemoved.Contains(t));
            success = teamsTobeRemoved.Count == removedTeams;
        }
        return success;
    }
    #endregion Teams Methods

    #endregion Methods
}
