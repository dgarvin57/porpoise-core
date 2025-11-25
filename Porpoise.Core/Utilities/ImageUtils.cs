#nullable enable

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Porpoise.Core.Utilities;

/// <summary>
/// High-quality image resizing with alignment control.
/// Used for researcher logos, client logos, and report thumbnails.
/// </summary>
public static class ImageUtils
{
    public static Bitmap ResizeImage(Image sourceImage, int targetWidth, int targetHeight,
        ImageVertAlign verticalAlign = ImageVertAlign.Middle,
        ImageHorizAlign horizontalAlign = ImageHorizAlign.Middle)
    {
        ArgumentNullException.ThrowIfNull(sourceImage);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetWidth);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetHeight);

        using var source = new Bitmap(sourceImage);
        var dest = new Bitmap(targetWidth, targetHeight, PixelFormat.Format32bppArgb);

        double sourceRatio = (double)source.Width / source.Height;
        double destRatio = (double)dest.Width / dest.Height;

        int newX = 0, newY = 0;
        int newWidth = dest.Width;
        int newHeight = dest.Height;

        if (Math.Abs(destRatio - sourceRatio) < 0.0001) // Same aspect ratio
        {
            // No change needed
        }
        else if (destRatio > sourceRatio) // Destination is wider → letterboxed vertically
        {
            newWidth = (int)Math.Floor(sourceRatio * newHeight);
            newX = horizontalAlign switch
            {
                ImageHorizAlign.Left => 3,
                ImageHorizAlign.Middle => (dest.Width - newWidth) / 2,
                ImageHorizAlign.Right => dest.Width - newWidth - 3,
                _ => newX
            };
        }
        else // Destination is taller → letterboxed horizontally
        {
            newHeight = (int)Math.Floor(newWidth / sourceRatio);
            newY = verticalAlign switch
            {
                ImageVertAlign.Top => 3,
                ImageVertAlign.Middle => (dest.Height - newHeight) / 2,
                ImageVertAlign.Bottom => dest.Height - newHeight - 3,
                _ => newY
            };
        }

        using var graphics = Graphics.FromImage(dest);
        graphics.CompositingQuality = CompositingQuality.HighQuality;
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;

        graphics.DrawImage(source, newX, newY, newWidth, newHeight);

        return dest;
    }
}

public enum ImageVertAlign
{
    Top,
    Middle,
    Bottom
}

public enum ImageHorizAlign
{
    Left,
    Middle,
    Right
}