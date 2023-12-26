using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace RescueScoreManager.Helpers;

public static class TextHelper
{
    public static string CleanedText(string text)
    {
        string cleanedName = string.Empty;
        string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
        foreach (char c in invalid)
        {
            cleanedName = text.Replace(c.ToString(), "-");
        }
        return cleanedName;
    }

    public static string RemoveDiacritics(string text)
    {
        string normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (char c in from c in normalized
                           let u = CharUnicodeInfo.GetUnicodeCategory(c)
                           where u != UnicodeCategory.NonSpacingMark
                           select c)
        {
            sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
