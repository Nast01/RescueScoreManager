namespace RescueScoreManager.Model;

public partial class Athlete : Licensee
{
    #region Attributes
    public bool IsForfeit { get; set; }
    public int OrderNumber { get; set; }
    //public Category Category { get; set; }
    #endregion Attributes

}
