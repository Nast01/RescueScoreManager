using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

using RescueScoreManager.Helper;

namespace RescueScoreManager.Data;

public abstract partial class Team
{
    [Key]
    public int Id { get; set; }
    public bool IsForfeit { get; set; } = false;
    public bool IsForfeitFinal { get; set; } = false;
    public int EntryTime { get; set; }
    public string EntryTimeLabel => TimeHelper.ConvertCentisecondInString(EntryTime);
    public int Number {  get; set; }
    public int Status { get; set; }
    public string StatusLabel { get;set; } = string.Empty;

    //one-to-one relationship to Race
    public int RaceId { get; set; } // Required foreign key property
    public Race Race { get; set; } = null!; // Required reference navigation to principal


    //one-to-one relationship to Category
    public int CategoryId { get; set; } // Required foreign key property
    public Category Category { get; set; } = null!; // Required reference navigation to principal

    public ICollection<HeatResult> HeatResults { get; set; } = new List<HeatResult>();

    public abstract XElement WriteXml();
}
