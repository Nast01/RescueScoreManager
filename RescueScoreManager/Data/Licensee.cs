﻿using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

using static RescueScoreManager.Data.EnumRSM;

namespace RescueScoreManager.Data;

public abstract partial class Licensee
{
    #region Attributes
    [Key]
    public int Id { get; set; }
    public string LicenseeNumber { get; set; }
    public string LastName { get; set; }
    public string FirstName { get; set; }
    public int BirthYear { get; set; }
    public Gender Gender { get; set; }
    public string? FullName { get { return string.Join(" ", LastName, FirstName); } }
    public bool IsLicensee { get; set; }
    public bool IsGuest { get; set; }
    public string Nationality { get; set; }
    public string NationalityCode { get; set; }

    //one-to-many relationship to Club
    public int ClubId { get; set; } // Required foreign key property
    public Club Club { get; set; } = null!; // Required reference navigation to principal

    #endregion Attributes

    public abstract XElement WriteXml();
}
