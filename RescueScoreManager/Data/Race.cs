using System.ComponentModel.DataAnnotations;

using RescueScoreManager.Helper;

using static RescueScoreManager.Data.EnumRSM;

namespace RescueScoreManager.Data;

public class Race
{
    #region Attributes
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Label => Categories.Count == 1 ? $"{Name} {Categories.FirstOrDefault().Name}":$"{Name}";
    public Gender Gender { get; set; }
    public Speciality Speciality { get; set; }
    public int Discipline { get; set; }
    public int NumberByTeam { get; set; }
    public int? Distance { get; set; } = null;
    public int Interval { get; set; } = 9000;
    public string IntervalLabel => TimeHelper.ConvertCentisecondInString(Interval);
    public bool IsRelay { get; set; }

    //one-to-many relationship to Competition
    public int CompetitionId { get; set; } // Required foreign key property
    public Competition Competition { get; set; } = null!; // Required reference navigation to principal
    public ICollection<Category> Categories { get; } = new List<Category>();
    public ICollection<Team> Teams { get; } = new List<Team>();
    public ICollection<MeetingElement> MeetingElements { get; } = new List<MeetingElement>();
    #endregion Attributes
}
