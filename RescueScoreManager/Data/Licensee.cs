using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

using static RescueScoreManager.Data.EnumRSM;

namespace RescueScoreManager.Data;

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
    public bool IsLicensee { get; set; }
    public bool IsGuest { get; set; }
    public string Nationality { get; set; }

    //one-to-many relationship to Club
    public int ClubId { get; set; } // Required foreign key property
    public Club Club { get; set; } = null!; // Required reference navigation to principal

    #endregion Attributes

    public abstract XElement WriteXml();
}
