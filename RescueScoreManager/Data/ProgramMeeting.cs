using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

namespace RescueScoreManager.Data;

public partial class ProgramMeeting
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime ProgramDate { get; set; }
    public DateTime BeginHour { get; set; }
    public DateTime EndHour { get; set; }

    public ICollection<ProgramSlot> ProgramSlots { get; set; } = new List<ProgramSlot>();

    public ProgramMeeting()
    {
        // Parameterless constructor for manual creation
    }

    public ProgramMeeting(XElement xElement,List<RaceFormatDetail> raceFormatDetails,List<Heat> heats)
    {

        Id = int.Parse(xElement.Attribute(Properties.Resources.Id_XMI).Value);
        Name = xElement.Attribute(Properties.Resources.Name_XMI).Value;
        Description = xElement.Attribute(Properties.Resources.Description_XMI).Value;
        ProgramDate = DateTime.Parse(xElement.Attribute(Properties.Resources.ProgramDate_XMI).Value);
        BeginHour = DateTime.Parse(xElement.Attribute(Properties.Resources.BeginHour_XMI).Value);
        EndHour = DateTime.Parse(xElement.Attribute(Properties.Resources.EndHour_XMI).Value);
        var slots = xElement.Elements(Properties.Resources.ProgramSlot_XMI);

        if (slots != null)
        {
            foreach (var slot in slots)
            {
                ProgramSlots.Add(new ProgramSlot(slot, raceFormatDetails, heats));
            }
        }
    }

    #region Public Method
    public XElement WriteXml()
    {
        XElement xElement = new XElement(Properties.Resources.ProgramMeeting_XMI,
                            new XAttribute(Properties.Resources.Id_XMI, Id),
                            new XAttribute(Properties.Resources.Name_XMI, Name),
                            new XAttribute(Properties.Resources.Description_XMI, Description),
                            new XAttribute(Properties.Resources.ProgramDate_XMI, ProgramDate.ToString("yyyy-MM-ddTHH:mm:ss")),
                            new XAttribute(Properties.Resources.BeginHour_XMI, BeginHour.ToString("yyyy-MM-ddTHH:mm:ss")),
                            new XAttribute(Properties.Resources.EndHour_XMI, EndHour.ToString("yyyy-MM-ddTHH:mm:ss"))                            );

        foreach (var programSlot in ProgramSlots)
        {
            xElement.Add(programSlot.WriteXml());
        }

        return xElement;
    }
    #endregion Public Method
}
