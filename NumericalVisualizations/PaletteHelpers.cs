using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;

namespace NumericalVisualizations
{
    /// <summary>
    /// Utility methods for working with color palettes
    /// </summary>
    public static class PaletteHelpers
    {
        /// <summary>
        /// Save Spectrum360 as a PNG grid (cols x rows). Each swatch is swatchSize x swatchSize.
        /// </summary>
        public static void SaveSpectrum360AsImage(string path, int swatchSize = 24, int cols = 36)
        {
            var palette = ColorPalettes.Spectrum360;
            int total = palette.Length;
            int rows = (int)Math.Ceiling(total / (double)cols);
            int width = cols * swatchSize;
            int height = rows * swatchSize;

            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
            using var bmp = new Bitmap(width, height);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.Black);

            for (int i = 0; i < total; i++)
            {
                int c = i % cols;
                int r = i / cols;
                var cs = palette[i];
                using var brush = new SolidBrush(Color.FromArgb(cs.red, cs.green, cs.blue));
                var rect = new Rectangle(c * swatchSize, r * swatchSize, swatchSize, swatchSize);
                g.FillRectangle(brush, rect);
                g.DrawRectangle(Pens.Gray, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
            }

            bmp.Save(path, System.Drawing.Imaging.ImageFormat.Png);
        }

        /// <summary>
        /// Save Spectrum360 as a simple HTML file with inline blocks. Browser friendly and copy/paste sharable.
        /// </summary>
        public static void SaveSpectrum360AsHtml(string path, int cols = 36, int swatchPx = 30)
        {
            var palette = ColorPalettes.Spectrum360;
            int total = palette.Length;

            var sb = new StringBuilder();
            sb.AppendLine("<!doctype html>");
            sb.AppendLine("<html><head><meta charset=\"utf-8\"><title>Spectrum360</title>");
            sb.AppendLine("<style>body{font-family:Segoe UI,Arial;padding:10px;background:#111;color:#ddd} .sw{display:inline-block;margin:1px;width:" + swatchPx + "px;height:" + swatchPx + "px;border:1px solid #333;box-sizing:border-box} .item{display:inline-block;text-align:center;width:" + swatchPx + "px;font-size:10px;color:#ddd}</style>");
            sb.AppendLine("</head><body>");
            sb.AppendLine("<h2>Spectrum360 – natural order</h2>");
            sb.AppendLine("<div>");
            for (int i = 0; i < total; i++)
            {
                var cs = palette[i];
                var rgb = $"rgb({cs.red},{cs.green},{cs.blue})";
                var hex = $"#{cs.red:X2}{cs.green:X2}{cs.blue:X2}";
                sb.AppendLine($"<div style=\"display:inline-block;text-align:center;vertical-align:top;margin:2px\">");
                sb.AppendLine($"  <div class=\"sw\" style=\"background:{rgb}\" title=\"#{i} {hex} ({cs.red},{cs.green},{cs.blue})\"></div>");
                sb.AppendLine($"  <div style=\"font-size:10px;color:#bbb\">{i}</div>");
                sb.AppendLine($"</div>");
            }
            sb.AppendLine("</div>");
            sb.AppendLine("</body></html>");

            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);

            try
            {
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            }
            catch { }
        }
    }
}
