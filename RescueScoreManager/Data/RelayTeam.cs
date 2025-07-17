using System.Xml.Linq;

using Newtonsoft.Json.Linq;

namespace RescueScoreManager.Data;

public partial class RelayTeam: Team
{
    public ICollection<Athlete> Athletes { get; } = new List<Athlete>();

    #region Constructor
    public RelayTeam()
    {
    }

    public RelayTeam(JToken jData, Race race,List<Licensee> lics) 
    {
        Id = jData["Id"].Value<int>();
        EntryTime = jData["Performance"].Value<int>();
        IsForfeit = false;
        IsForfeitFinal = false;
        Number = race.NumberByTeam;
        foreach (JToken jAthlete in jData["athletes"].Children())
        {
            Athlete athlete = null;// lics.Find(l => l.Id == jData["athletes"][0]["NumeroLicence"].Value<string>()) as Athlete;
            AddAthlete(athlete);
        }
    }

    public RelayTeam(XElement xElement,List<Athlete> lics)
    {
        Id = int.Parse(xElement.Attribute(Properties.Resources.Id_XMI).Value);
        IsForfeit = bool.Parse(xElement.Attribute(Properties.Resources.IsForfeit_XMI).Value);
        IsForfeitFinal = bool.Parse(xElement.Attribute(Properties.Resources.IsForfeitFinal_XMI).Value);
        EntryTime = int.Parse(xElement.Attribute(Properties.Resources.EntryTime_XMI).Value);
        Number = int.Parse(xElement.Attribute(Properties.Resources.EntryTime_XMI).Value);

        string[] athIds = xElement.Attribute(Properties.Resources.Athletes_XMI).Value.Split(" ");
        foreach (string id in athIds)
        {
            Athlete athlete = null;// lics.Find(l => l.Id == id);
            if (athlete != null)
            {
                Athletes.Add(athlete);
                athlete.RelayTeams.Add(this);
            }
        }
    }
    #endregion Constructor

    #region Methods
    #region Export
    //public override void ToExcel(ref ExcelWorksheet worksheet, ref int row, ref int column)
    //{

    //}
    #endregion Export
    public override XElement WriteXml()
    {
        string athIds = string.Empty;
        foreach (Athlete athlete in Athletes)
        {
            athIds += athlete.Id + " ";
        }

        athIds = athIds.Trim();

        return new XElement(Properties.Resources.RelayTeam_XMI,
                                new XAttribute(Properties.Resources.Id_XMI, Id),
                                new XAttribute(Properties.Resources.IsForfeit_XMI, IsForfeit),
                                new XAttribute(Properties.Resources.IsForfeitFinal_XMI, IsForfeitFinal),
                                new XAttribute(Properties.Resources.EntryTime_XMI, EntryTime),
                                new XAttribute(Properties.Resources.Number_XMI, Number),
                                //new XAttribute(Properties.Resources.IdApi_XMI, IdApi),
                                new XAttribute(Properties.Resources.Athletes_XMI, athIds),
                                new XAttribute(Properties.Resources.Race_XMI, Race.Id)
                            );
    }
    #region Athlete Methods
    public bool AddAthlete(Athlete athlete)
    {
        bool success = false;

        if (!Athletes.Contains(athlete))
        {
            Athletes.Add(athlete);
            success = true;
        }

        return success;
    }

    public bool AddAthletes(List<Athlete> athletes)
    {
        bool success = false;

        bool isNotContained = athletes.Except(Athletes).Any();

        if (athletes.Count > 0 && isNotContained)
        {
            Athletes.ToList<Athlete>().AddRange(athletes);
            success = true;
        }
        return success;
    }

    public bool RemoveAthlete(Athlete athlete)
    {
        bool success = false;
        if (athlete != null && Athletes.Contains(athlete))
        {
            success = Athletes.Remove(athlete);
        }
        return success;
    }

    public bool RemoveAthletes(List<Athlete> athleteTobeRemoved)
    {
        bool success = false;

        if (athleteTobeRemoved.Count > 0)
        {
            int removedAthletes = Athletes.ToList<Athlete>().RemoveAll(ath => athleteTobeRemoved.Contains(ath));
            success = athleteTobeRemoved.Count == removedAthletes;
        }
        return success;
    }
    #endregion Athlete Methods

    public Club GetClub()
    {
        return Athletes.First().Club;
    }
    #endregion Methods
}
