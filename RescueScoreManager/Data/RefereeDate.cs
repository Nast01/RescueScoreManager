using RescueScoreManager.Model;

namespace RescueScoreManager.Data;

public partial class RefereeDate
{
    public int Id { get; set; }
    public DateTime Availability { get; set; }
    public string RefereeId { get; set; }
    public Referee Referee { get; set; }
}
