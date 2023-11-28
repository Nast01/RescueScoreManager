namespace RescueScoreManager.Data;

public partial class RelayTeam: Team
{
    public ICollection<Athlete> Athletes { get; } = new List<Athlete>();
}
