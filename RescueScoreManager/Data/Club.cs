using Newtonsoft.Json.Linq;

using System.ComponentModel;
using System.Security.AccessControl;
using System.Xml.Linq;

namespace RescueScoreManager.Data;

public partial class Club
{
    #region Attributes
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    //one-to-many relationship to Competition
    public int CompetitionId { get; set; }
    public Competition Competition { get; set; } = null!;

    //one-to-many relationship to Licensees
    public ICollection<Licensee> Licensees { get; } = new List<Licensee>();

    //public List<Licensee>? Licensees { get; set; }
    #endregion Attributes

    public Club()
    {

    }

    public Club(JToken data, bool isForeignClub)
    {
        if (isForeignClub == false)
        {
            Id = data["Id"].Value<int>();
            Name = data["label"].Value<string>();
        }
        else
        {
            Id = data["athletes"][0]["idClub"].Value<int>();
            Name = data["athletes"][0]["clubLabel"].Value<string>();
        }
    }

    public Club(XElement clubElement)
    {
        Id = int.Parse(clubElement.Attribute(Properties.Resources.Id_XMI).Value);
        Name = clubElement.Attribute(Properties.Resources.Name_XMI).Value;
    }

    #region Methods

    public XElement WriteXml()
    {
        XElement xElement = new XElement(Properties.Resources.Club_XMI,
                            new XAttribute(Properties.Resources.Id_XMI, Id),
                            new XAttribute(Properties.Resources.Name_XMI, Name)
                            );
        foreach (Licensee licensee in Licensees)
        {
            xElement.Add(licensee.WriteXml());
        }

        return xElement;
    }
    #region Licensees Methods
    public bool AddLicensee(Licensee licensee)
    {
        bool success = false;

        if (!Licensees.Contains(licensee))
        {
            Licensees.Add(licensee);
            licensee.Club = this;
            success = true;
        }

        return success;
    }

    public bool AddLicensees(List<Licensee> licensees)
    {
        bool success = false;
        bool isNotContained = licensees.Except(Licensees).Any();

        if (licensees.Count > 0 && isNotContained)
        {
            Licensees.ToList<Licensee>().AddRange(licensees);
            licensees.ForEach(x => x.Club = this);
            success = true;
        }
        return success;
    }

    public bool RemoveLicensee(Licensee licensee)
    {
        bool success = false;
        if (licensee != null && Licensees.Contains(licensee))
        {
            success = Licensees.Remove(licensee);
        }
        return success;
    }

    public bool RemoveLicensees(List<Licensee> licenseesTobeRemoved)
    {
        bool success = false;

        if (licenseesTobeRemoved.Count > 0)
        {
            int removedClubs = Licensees.ToList<Licensee>().RemoveAll(club => licenseesTobeRemoved.Contains(club));
            success = licenseesTobeRemoved.Count == removedClubs;
        }
        return success;
    }
    #endregion Licensee Methods

    #region Accessor Methods
    public List<Licensee> GetAthletes()
    {
        return Licensees.Where(lic => lic is Athlete).ToList();
    }
    public List<Licensee> GetReferees()
    {
        return Licensees.Where(lic => lic is Referee).ToList();
    }
    #endregion Accessor Methods
    #endregion Methods
}
