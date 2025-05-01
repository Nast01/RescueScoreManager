using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RescueScoreManager.Data;

/// <summary>
/// Model representing a language option
/// </summary>
public class LanguageModel
{
    public required string DisplayName { get; set; }
    public required string CultureCode { get; set; }
    public CultureInfo Culture => new CultureInfo(CultureCode);

    public override string ToString() => DisplayName;
}
