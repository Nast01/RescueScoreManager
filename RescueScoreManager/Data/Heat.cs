using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RescueScoreManager.Data;

public partial class Heat
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; } 
    public string Label { get; set; }
    public int Number {  get; set; }
    public DateTime StartHour { get; set; }
    public DateTime EndHour { get; set; }
    public bool IsFinalA { get; set; } = false;
    public bool IsFinalB { get; set; } = false;

    //one-to-many relationship to Round
    public int RoundId { get; set; }
    public Round Round { get; set; } = null!;

    public ICollection<HeatResult> HeatResults { get; set; } //one to many relation
}
