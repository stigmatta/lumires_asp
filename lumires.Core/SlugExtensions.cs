using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace lumires.Core;

public static partial class SlugExtensions
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Globalization", "CA1308", 
        Justification = "Slugs must be lowercase by convention")]
    public static string Slugify(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        
        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
    
        foreach (var c in normalized
                     .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark))
        {
            sb.Append(c);
        }
    
        var result = sb.ToString()
            .Normalize(NormalizationForm.FormC)
            .ToLowerInvariant();
    
        result = MyRegex().Replace(result, "");
        result = TrimRegex().Replace(result, "-").Trim('-');
    
        return result;
    }

    [GeneratedRegex(@"[\s-]+")]
    private static partial Regex TrimRegex();
    [GeneratedRegex(@"[^a-z0-9\s-]")]
    private static partial Regex MyRegex();
}