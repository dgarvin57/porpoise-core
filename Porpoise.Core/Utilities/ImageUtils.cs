// Porpoise.Core/Utilities/ImageUtils.cs
#nullable enable

using SkiaSharp;

namespace Porpoise.Core.Utilities;

/// <summary>
/// Modern, cross-platform image resizing — works on Mac, Linux, Windows, web
/// Replaces old System.Drawing version
/// </summary>
public static class ImageUtils
{
    public enum ImageVertAlign { Top, Middle, Bottom }
    public enum ImageHorizAlign { Left, Middle, Right }

    public static byte[]? ResizeImage(byte[] sourceBytes, int targetWidth, int targetHeight,
        ImageVertAlign verticalAlign = ImageVertAlign.Middle,
        ImageHorizAlign horizontalAlign = ImageHorizAlign.Middle)
    {
        if (sourceBytes == null || sourceBytes.Length == 0) return null;

        using var original = SKBitmap.Decode(sourceBytes);
        if (original == null) return null;

        using var resized = new SKBitmap(targetWidth, targetHeight, original.ColorType, original.AlphaType);
        using var canvas = new SKCanvas(resized);
        canvas.Clear(SKColors.Transparent);

        float scale = Math.Min((float)targetWidth / original.Width, (float)targetHeight / original.Height);
        int newWidth = (int)(original.Width * scale);
        int newHeight = (int)(original.Height * scale);

        int x = horizontalAlign switch
        {
            ImageHorizAlign.Left => 10,
            ImageHorizAlign.Right => targetWidth - newWidth - 10,
            _ => (targetWidth - newWidth) / 2
        };

        int y = verticalAlign switch
        {
            ImageVertAlign.Top => 10,
            ImageVertAlign.Bottom => targetHeight - newHeight - 10,
            _ => (targetHeight - newHeight) / 2
        };

        using var paint = new SKPaint
        {
            IsAntialias = true,
            FilterQuality = SKFilterQuality.High
        };

        canvas.DrawBitmap(original, SKRect.Create(x, y, newWidth, newHeight), paint);

        using var image = SKImage.FromBitmap(resized);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }
}