using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

using static RescueScoreManager.Data.EnumRSM;

namespace RescueScoreManager.Data;

public partial class ProgramRun
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Site { get; set; } = string.Empty;
    public ProgramStatus Status { get; set; }
    public DateTime BeginHour { get; set; }
    public DateTime EndHour { get; set; }

    public int HeatId { get; set; }
    public Heat Heat { get; set; } = null!;

    public int ProgramSlotId { get; set; }
    public ProgramSlot ProgramSlot { get; set; } = null!;

    public ProgramRun(XElement xElement, List<Heat> heats)
    {
        Id = int.Parse(xElement.Attribute(Properties.Resources.Id_XMI)?.Value ?? "0");
        Name = xElement.Attribute(Properties.Resources.Name_XMI)?.Value ?? string.Empty;
        Site = xElement.Attribute(Properties.Resources.Site_XMI)?.Value ?? string.Empty;
        Status = Enum.Parse<ProgramStatus>(xElement.Attribute(Properties.Resources.Status_XMI)?.Value ?? "Unknown");
        BeginHour = DateTime.Parse(xElement.Attribute(Properties.Resources.BeginHour_XMI)?.Value ?? DateTime.MinValue.ToString());
        EndHour = DateTime.Parse(xElement.Attribute(Properties.Resources.EndHour_XMI)?.Value ?? DateTime.MinValue.ToString());
        HeatId = int.Parse(xElement.Attribute(Properties.Resources.HeatId_XMI)?.Value ?? "0");
        Heat = heats.FirstOrDefault(h => h.Id == HeatId) ?? new Heat();

    }

    #region Public Method
    public XElement WriteXml()
    {
        XElement xElement = new XElement(Properties.Resources.ProgramRun_XMI,
                            new XAttribute(Properties.Resources.Id_XMI, Id),
                            new XAttribute(Properties.Resources.Name_XMI, Name),
                            new XAttribute(Properties.Resources.Site_XMI, Site),
                            new XAttribute(Properties.Resources.Status_XMI, Status.ToString()),
                            new XAttribute(Properties.Resources.BeginHour_XMI, BeginHour.ToString("yyyy-MM-ddTHH:mm:ss")),
                            new XAttribute(Properties.Resources.EndHour_XMI, EndHour.ToString("yyyy-MM-ddTHH:mm:ss")),
                            new XAttribute(Properties.Resources.HeatId_XMI, HeatId)
                            );

        return xElement;
    }
    #endregion Public Method
}
