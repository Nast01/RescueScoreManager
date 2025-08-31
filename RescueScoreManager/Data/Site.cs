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

    #region Public Method
    public XElement WriteXml()
    {
        XElement xElement = new XElement(Properties.Resources.Site_XMI,
                            new XAttribute(Properties.Resources.Id_XMI, Id),
                            new XAttribute(Properties.Resources.Name_XMI, Name),
                            new XAttribute(Properties.Resources.Description_XMI, Description),
                            new XAttribute(Properties.Resources.Icon_XMI, Icon)
                            );

        return xElement;
    }
    #endregion Public Method
}
