namespace RescueScoreManager.Model;

public partial class Athlete : Licensee
{
    #region Attributes
    public bool IsForfeit { get; set; }
    public int OrderNumber { get; set; }

    #endregion Attributes

}
