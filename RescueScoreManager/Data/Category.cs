namespace RescueScoreManager.Data;

public class Category
{
    #region Attributes
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<Licensee> Licensees { get; } = new List<Licensee>();//one-to-many relationship
    public ICollection<Race> Races { get; } = new List<Race>(); // Many to many relation
    public ICollection<MeetingElement> MeetingElements { get; } = new List<MeetingElement>(); // Many to many relation
    public ICollection<Round> Rounds { get; } = new List<Round>();//one-to-many relationship
    #endregion Attributes

}
