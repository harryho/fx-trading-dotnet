using System;
using System.Drawing;
using System.Windows.Forms;

namespace fxpa.ui;

/// <summary>
/// Bitmap helpers used to turn "the chart drew something" into an assertion.
/// </summary>
internal static class PixelProbe
{
    /// <summary>
    /// Renders a control into a bitmap through the WinForms print pipeline
    /// (WM_PRINT), i.e. exactly the code path that paints the real window.
    /// </summary>
    public static Bitmap Capture(Control control)
    {
        int width = Math.Max(1, control.Width);
        int height = Math.Max(1, control.Height);

        var bitmap = new Bitmap(width, height);
        try
        {
            control.DrawToBitmap(bitmap, new Rectangle(0, 0, width, height));
        }
        catch
        {
            bitmap.Dispose();
            throw;
        }

        return bitmap;
    }

    /// <summary>
    /// Number of pixels that differ between two equally sized bitmaps.
    /// A small per-channel tolerance absorbs GDI+ anti-aliasing jitter.
    /// </summary>
    public static int CountDifferingPixels(Bitmap left, Bitmap right, int tolerance = 8)
    {
        if (left.Width != right.Width || left.Height != right.Height)
        {
            throw new ArgumentException("Bitmaps must have the same size.");
        }

        int differing = 0;

        var leftData = left.LockBits(
            new Rectangle(0, 0, left.Width, left.Height),
            System.Drawing.Imaging.ImageLockMode.ReadOnly,
            System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        var rightData = right.LockBits(
            new Rectangle(0, 0, right.Width, right.Height),
            System.Drawing.Imaging.ImageLockMode.ReadOnly,
            System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        try
        {
            int bytes = leftData.Stride * left.Height;
            var leftBytes = new byte[bytes];
            var rightBytes = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(leftData.Scan0, leftBytes, 0, bytes);
            System.Runtime.InteropServices.Marshal.Copy(rightData.Scan0, rightBytes, 0, bytes);

            for (int i = 0; i < bytes; i += 4)
            {
                if (Math.Abs(leftBytes[i] - rightBytes[i]) > tolerance
                    || Math.Abs(leftBytes[i + 1] - rightBytes[i + 1]) > tolerance
                    || Math.Abs(leftBytes[i + 2] - rightBytes[i + 2]) > tolerance
                    || Math.Abs(leftBytes[i + 3] - rightBytes[i + 3]) > tolerance)
                {
                    differing++;
                }
            }
        }
        finally
        {
            left.UnlockBits(leftData);
            right.UnlockBits(rightData);
        }

        return differing;
    }

    /// <summary>
    /// Number of pixels that are not the given background colour.
    /// </summary>
    public static int CountNonBackgroundPixels(Bitmap bitmap, Color background, int tolerance = 8)
    {
        int count = 0;

        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                Color pixel = bitmap.GetPixel(x, y);
                if (Math.Abs(pixel.R - background.R) > tolerance
                    || Math.Abs(pixel.G - background.G) > tolerance
                    || Math.Abs(pixel.B - background.B) > tolerance)
                {
                    count++;
                }
            }
        }

        return count;
    }

    /// <summary>
    /// The colour that dominates the bitmap - used as the "nothing was drawn"
    /// reference.
    /// </summary>
    public static Color DominantColor(Bitmap bitmap)
    {
        var counts = new System.Collections.Generic.Dictionary<int, int>();
        var colors = new System.Collections.Generic.Dictionary<int, Color>();

        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                Color pixel = bitmap.GetPixel(x, y);
                int key = pixel.ToArgb();
                counts.TryGetValue(key, out int count);
                counts[key] = count + 1;
                colors[key] = pixel;
            }
        }

        int bestKey = 0;
        int bestCount = -1;
        foreach (var pair in counts)
        {
            if (pair.Value > bestCount)
            {
                bestCount = pair.Value;
                bestKey = pair.Key;
            }
        }

        return colors[bestKey];
    }
}
