using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;
using static RescueScoreManager.Data.EnumRSM;

namespace RescueScoreManager.Data;

public partial class Program
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int Version { get; set; } = 1;
    
    public ScheduleStatus Status { get; set; } = ScheduleStatus.Draft;
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    
    public DateTime? PublishedDate { get; set; }
    
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    public ICollection<Site> Sites { get; set; } = new List<Site>();

    public Program()
    {
        // Parameterless constructor for manual creation
    }

    public Program(XElement xElement)
    {
        Id = int.Parse(xElement.Attribute(Properties.Resources.Id_XMI)?.Value ?? "0");
        Version = int.Parse(xElement.Attribute("Version")?.Value ?? "1");
        Status = Enum.Parse<ScheduleStatus>(xElement.Attribute("Status")?.Value ?? "Draft");
        CreatedDate = DateTime.Parse(xElement.Attribute("CreatedDate")?.Value ?? DateTime.Now.ToString());

        string? publishedDateValue = xElement.Attribute("PublishedDate")?.Value;
        PublishedDate = string.IsNullOrEmpty(publishedDateValue) ? null : DateTime.Parse(publishedDateValue);
        
        Description = xElement.Attribute("Description")?.Value ?? string.Empty;
    }

    #region Public Methods
    
    public XElement WriteXml()
    {
        var xElement = new XElement("Program",
            new XAttribute(Properties.Resources.Id_XMI, Id),
            new XAttribute("Version", Version),
            new XAttribute("Status", Status.ToString()),
            new XAttribute("CreatedDate", CreatedDate.ToString("yyyy-MM-ddTHH:mm:ss")),
            new XAttribute("Description", Description));

        if (PublishedDate.HasValue)
        {
            xElement.Add(new XAttribute("PublishedDate", PublishedDate.Value.ToString("yyyy-MM-ddTHH:mm:ss")));
        }

        foreach (var site in Sites)
        {
            xElement.Add(site.WriteXml());
        }

        return xElement;
    }

    public void Publish()
    {
        Status = ScheduleStatus.Published;
        PublishedDate = DateTime.Now;
        Version++;
    }

    public void Archive()
    {
        Status = ScheduleStatus.Archived;
    }

    public bool IsPublished => Status == ScheduleStatus.Published;
    
    public bool IsDraft => Status == ScheduleStatus.Draft;
    
    public bool IsArchived => Status == ScheduleStatus.Archived;

    public bool CanBePublished => Status == ScheduleStatus.Draft;

    public bool CanBeEdited => Status == ScheduleStatus.Draft;

    public void IncrementVersion()
    {
        Version++;
    }

    public static Program CreateDefault()
    {
        var program = new Program
        {
            Id = 1,
            Version = 1,
            Status = ScheduleStatus.Draft,
            CreatedDate = DateTime.Now,
            PublishedDate = null,
            Description = "Default Competition Schedule",
            Sites = new List<Site>()
        };

        return program;
    }

    #endregion Public Methods
}
