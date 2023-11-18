using static RescueScoreManager.Model.EnumRSM;

namespace RescueScoreManager.Model;
public partial class Competition
{
    #region Attributes
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime BeginDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime EntryLimitDate { get; set; }
    public BeachType? BeachType { get; set; }
    public SwimType? SwimType { get; set; }
    public Speciality Speciality { get; set; }
    public ChronoType? ChronoType { get; set; }
    public bool IsEligibleToNationalRecord { get; set; }
    public int PriceByAthlete { get; set; }
    public int PriceByEntry { get; set; }
    public int PriceByClub { get; set; }
    public string Organizer { get; set; } 
    public ICollection<Club> Clubs { get; } = new List<Club>();//one-to-many relationship
    //public List<Race> Races { get; set; }

    #endregion Attributes
}
