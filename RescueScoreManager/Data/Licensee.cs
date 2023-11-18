using System.ComponentModel.DataAnnotations;

using static RescueScoreManager.Model.EnumRSM;

namespace RescueScoreManager.Model;

public abstract partial class Licensee
{
    #region Attributes
    [Key]
    public String Id { get; set; }
    public String LastName { get; set; }
    public String FirstName { get; set; }
    public int BirthYear { get; set; }
    public Gender Gender { get; set; }
    public String? FullName { get { return String.Join(" ", LastName, FirstName); } }
    public bool IsLicencee { get; set; }
    public bool IsGuest { get; set; }
    public int ClubId { get; set; } // Required foreign key property
    public Club Club { get; set; } = null!; // Required reference navigation to principal
    #endregion Attributes


}
