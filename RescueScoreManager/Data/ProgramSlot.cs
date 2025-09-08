using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

namespace RescueScoreManager.Data;

public partial class ProgramSlot
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public DateTime BeginHour { get; set; }
    public DateTime EndHour { get; set; }

    public int RaceFormatDetailId { get; set; }
    public RaceFormatDetail RaceFormatDetail { get; set; } = null!;

    public ICollection<ProgramRun> ProgramRuns { get; set; } = new List<ProgramRun>();

    public int ProgramMeetingId { get; set; }
    public ProgramMeeting ProgramMeeting { get; set; } = null!;

    public ProgramSlot()
    {
        // Parameterless constructor for manual creation
    }

    public ProgramSlot(XElement xElement, List<RaceFormatDetail> raceFormatDetails, List<Heat> heats)
    {
        Id = int.Parse(xElement.Attribute(Properties.Resources.Id_XMI).Value);
        Name = xElement.Attribute(Properties.Resources.Name_XMI).Value;
        BeginHour = DateTime.Parse(xElement.Attribute(Properties.Resources.BeginHour_XMI).Value);
        EndHour = DateTime.Parse(xElement.Attribute(Properties.Resources.EndHour_XMI).Value);
        RaceFormatDetailId = int.Parse(xElement.Attribute(Properties.Resources.RaceFormatDetailId_XMI).Value);
        RaceFormatDetail = raceFormatDetails.FirstOrDefault(r => r.Id == RaceFormatDetailId) ?? new RaceFormatDetail();

        var runs = xElement.Elements(Properties.Resources.ProgramRun_XMI);
        if (runs != null)
        {
            foreach (var run in runs)
            {
                ProgramRuns.Add(new ProgramRun(run,heats));
            }
        }
    }

    #region Public Method
    public XElement WriteXml()
    {
        XElement xElement = new XElement(Properties.Resources.ProgramSlot_XMI,
                            new XAttribute(Properties.Resources.Id_XMI, Id),
                            new XAttribute(Properties.Resources.Name_XMI, Name),
                            new XAttribute(Properties.Resources.BeginHour_XMI, BeginHour.ToString("yyyy-MM-ddTHH:mm:ss")),
                            new XAttribute(Properties.Resources.EndHour_XMI, EndHour.ToString("yyyy-MM-ddTHH:mm:ss")),
                            new XAttribute(Properties.Resources.RaceFormatDetailId_XMI, RaceFormatDetailId)
                            );

        foreach (var programRun in ProgramRuns)
        {
            xElement.Add(programRun.WriteXml());
        }

        return xElement;
    }
    #endregion Public Method
}
