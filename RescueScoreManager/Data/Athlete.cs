namespace RescueScoreManager.Data;

public partial class Athlete : Licensee
{
    #region Attributes
    public bool IsForfeit { get; set; }
    public int OrderNumber { get; set; }

    public ICollection<IndividualTeam> IndividualTeams { get; } = new List<IndividualTeam>();
    public ICollection<RelayTeam> RelayTeams { get; } = new List<RelayTeam>();

    #endregion Attributes

}
