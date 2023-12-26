using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RescueScoreManager.Data;

public partial class RefereeDate
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public DateTime Availability { get; set; }
    public string RefereeId { get; set; }
    public Referee Referee { get; set; }
}
