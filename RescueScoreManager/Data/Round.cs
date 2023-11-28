using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using static RescueScoreManager.Data.EnumRSM;

namespace RescueScoreManager.Data;

public partial class Round
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string Label { get; set; }    
    public int Number {  get; set; }
    public HeatType HeatType { get; set; }

    //zero-to-many relationship to Category
    public int? CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    //one-to-many relationship to Category
    public int MeetingElementId { get; set; }
    public MeetingElement MeetingElement { get; set; } = null!;
    public ICollection<Heat> Heats { get; set; } = new List<Heat>();//one to many relation
}
