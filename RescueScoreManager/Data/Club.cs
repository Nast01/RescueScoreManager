namespace RescueScoreManager.Model;

public class Club
{
    #region Attributes
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    //one-to-many relationship
    public int CompetitionId { get; set; }
    public Competition Competition { get; set; } = null!;


    //public List<Licensee>? Licensees { get; set; }
    #endregion Attributes
}
