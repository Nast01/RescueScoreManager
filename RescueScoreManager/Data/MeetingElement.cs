using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

using static RescueScoreManager.Data.EnumRSM;

namespace RescueScoreManager.Data;

public partial class MeetingElement
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public int Order { get; set; }
    public string Label { get; set; }
    public DateTime StartHour { get; set; }
    public DateTime EndHour { get; set; }
    public HeatType Type { get; set; }
    public bool isFinalA { get; set; }
    public bool isFinalB { get; set; }

    //zero-to-many relationship to Meeting
    public int? MeetingId { get; set; }
    public Meeting Meeting { get; set; } = null!;
    
    //one-to-many relationship to Race
    public int RaceId { get; set; }
    public Race Race { get; set; } = null!;

    public ICollection<Category> Categories { get; } = new List<Category>();

    public ICollection<Round> Rounds { get; } = new List<Round>();
}
