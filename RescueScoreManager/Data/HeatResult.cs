namespace RescueScoreManager.Data;

public partial class HeatResult
{
    public int Id { get; set; }
    public int Lane {  get; set; }
    public string Disqualification { get; set; } = "";
    public bool IsDisqualified { get; set; } = false;
    public bool IsForfeit { get; set; } = false;
    public bool IsOfficial {  get; set; } = false;
    
    //one-to-many relationship to Heat
    public int HeatId { get; set; }
    public Heat Heat { get; set; } = null!;

    //one-to-many relationship to Team
    public int TeamId { get; set; }
    public Team Team { get; set; } = null!;
}
