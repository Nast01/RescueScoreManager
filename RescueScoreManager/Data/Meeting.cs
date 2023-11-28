using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using static RescueScoreManager.Data.EnumRSM;

namespace RescueScoreManager.Data;

public partial class Meeting
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string Label { get; set; }
    public int Number {  get; set; }
    public DateTime StartHour { get; set; }
    public DateTime EndHour { get; set; }
    public Speciality MeetingType { get; set; }
    public HeatType HeatType { get; set; }

    public ICollection<MeetingElement>  MeetingElements { get; } = new List<MeetingElement>();//one-to-many relationship
    //one-to-many relationship to Competition
    public int CompetitionId { get; set; }
    public Competition Competition { get; set; } = null!;

    //one-to-many relationship to Meeting
    public int? RelatedMeetingId { get; set; }
    public Meeting RelatedMeeting { get; set; }

}
