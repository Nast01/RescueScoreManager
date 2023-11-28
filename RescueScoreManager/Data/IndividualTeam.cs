namespace RescueScoreManager.Data;

public partial class IndividualTeam : Team
{
    public string AthleteId { get; set; } // Required foreign key property
    public Athlete Athlete { get; set; } = null!; // Required reference navigation to principal

}
