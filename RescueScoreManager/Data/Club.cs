namespace RescueScoreManager.Model;

public partial class Club
{
    #region Attributes
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    //one-to-many relationship to Competition
    public int CompetitionId { get; set; }
    public Competition Competition { get; set; } = null!;

    //one-to-many relationship to Licensees
    public ICollection<Licensee> Licensees { get; } = new List<Licensee>();
    //public List<Licensee>? Licensees { get; set; }
    #endregion Attributes
}
