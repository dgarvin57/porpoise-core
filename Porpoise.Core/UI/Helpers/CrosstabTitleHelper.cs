// Porpoise.Core/UI/Helpers/CrosstabTitleHelper.cs
using System.Drawing;

namespace Porpoise.Core.UI.Helpers;

public static class CrosstabTitleHelper
{
    /// <summary>
    /// The exact smart ellipsis logic from the old UcCxStatisticsPresenter
    /// Preserves perfect UX: "Very Long Question Name... by Short" or "Short by Very Long Ques..."
    /// </summary>
    public static string FormatCrosstabTitle(string dvLabel, string ivLabel, Font font, int maxPixelWidth)
    {
        var full = $"{dvLabel} by {ivLabel}";
        var fullSize = TextRenderer.MeasureText(full, font);

        if (fullSize.Width <= maxPixelWidth)
            return full;

        const string ellipsis = "...";
        var ellipsisSize = TextRenderer.MeasureText(ellipsis, font).Width;
        var bySize = TextRenderer.MeasureText(" by ", font).Width;

        var dvSize = TextRenderer.MeasureText(dvLabel, font).Width;
        var ivSize = TextRenderer.MeasureText(ivLabel, font).Width;

        // Shrink the longer one first
        if (dvSize > ivSize)
        {
            var available = maxPixelWidth - ivSize - bySize - ellipsisSize - 10;
            return $"{TruncateToFit(dvLabel, font, available)}{ellipsis} by {ivLabel}";
        }
        else
        {
            var available = maxPixelWidth - dvSize - bySize - ellipsisSize - 10;
            return $"{dvLabel} by {TruncateToFit(ivLabel, font, available)}{ellipsis}";
        }
    }

    private static string TruncateToFit(string text, Font font, int maxWidth)
    {
        if (TextRenderer.MeasureText(text, font).Width <= maxWidth)
            return text;

        while (text.Length > 3 && TextRenderer.MeasureText(text + "...", font).Width > maxWidth)
            text = text[..^1];

        return text;
    }
}