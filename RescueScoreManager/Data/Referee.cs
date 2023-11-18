using RescueScoreManager.Data;

using static RescueScoreManager.Model.EnumRSM;

namespace RescueScoreManager.Model;

public partial class Referee : Licensee
{
    #region Attributes
    public RefereeLevel RefereeLevel { get; set; }
    public List<RefereeDate> RefereeAvailabilities { get; set; }
    //public Category Category { get; set; }
    #endregion Attributes
}
