using System.Xml.Linq;

namespace RescueScoreManager.Model;

public class Category
{
    #region Attributes
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<Licensee> Licensees { get; } = new List<Licensee>();//one-to-many relationship
    #endregion Attributes

}
