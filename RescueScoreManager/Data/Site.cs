using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using DocumentFormat.OpenXml.Spreadsheet;

using static RescueScoreManager.Data.EnumRSM;

namespace RescueScoreManager.Data;

public class Site
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string Icon { get; set; }
    
    public ICollection<ProgramMeeting> ProgramMeetings { get; set; } = new List<ProgramMeeting>();

    public Site(int id, string name, string? description, string icon   )
    {
        Id = id;
        Name = name;
        Description = description;
        Icon = icon;
    }
    public Site(XElement xElement)
    {
        Id = int.Parse(xElement.Attribute(Properties.Resources.Id_XMI)?.Value ?? "0");
        Name = xElement.Attribute(Properties.Resources.Name_XMI)?.Value ?? "Site 1";
        Description = xElement.Attribute(Properties.Resources.Description_XMI)?.Value ?? "";
        Icon = xElement.Attribute(Properties.Resources.Icon_XMI)?.Value ?? "";
    }

    public Site(XElement xElement, List<RaceFormatDetail> raceFormatDetails, List<Heat> heats)
    {
        Id = int.Parse(xElement.Attribute(Properties.Resources.Id_XMI)?.Value ?? "0");
        Name = xElement.Attribute(Properties.Resources.Name_XMI)?.Value ?? "Site 1";
        Description = xElement.Attribute(Properties.Resources.Description_XMI)?.Value ?? "";
        Icon = xElement.Attribute(Properties.Resources.Icon_XMI)?.Value ?? "";

        // Load program meetings for this site
        var meetingElements = xElement.Elements(Properties.Resources.ProgramMeeting_XMI);
        foreach (var meetingElement in meetingElements)
        {
            var meeting = new ProgramMeeting(meetingElement, raceFormatDetails, heats);
            ProgramMeetings.Add(meeting);
        }
    }

    #region Public Method
    public XElement WriteXml()
    {
        XElement xElement = new XElement(Properties.Resources.Site_XMI,
                            new XAttribute(Properties.Resources.Id_XMI, Id),
                            new XAttribute(Properties.Resources.Name_XMI, Name),
                            new XAttribute(Properties.Resources.Description_XMI, Description),
                            new XAttribute(Properties.Resources.Icon_XMI, Icon)
                            );

        // Add program meetings for this site
        foreach (var meeting in ProgramMeetings)
        {
            xElement.Add(meeting.WriteXml());
        }

        return xElement;
    }
    #endregion Public Method
}
