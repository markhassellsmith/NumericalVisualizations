using System.Drawing;
using System.Drawing.Imaging;

namespace NumericalVisualizations.Performance
{
    /// <summary>
    /// High-performance rendering utilities for pixel-based visualizations
    /// </summary>
    public static class RenderingHelpers
    {
        /// <summary>
        /// Fast parallel bitmap rendering using LockBits (10-100x faster than SetPixel)
        /// </summary>
        /// <param name="bitmap">Target bitmap to render into</param>
        /// <param name="xRange">Horizontal range in coordinate space</param>
        /// <param name="yRange">Vertical range in coordinate space</param>
        /// <param name="calculatePixel">Function to calculate color for each (x,y) coordinate</param>
        public static unsafe void RenderFast(
            Bitmap bitmap,
            double xRange,
            double yRange,
            Func<double, double, Color> calculatePixel)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            double deltaX = xRange / width;
            double deltaY = yRange / height;

            var rect = new Rectangle(0, 0, width, height);
            var bmpData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            try
            {
                byte* scan0 = (byte*)bmpData.Scan0;
                int stride = bmpData.Stride;

                Parallel.For(0, height, y =>
                {
                    byte* row = scan0 + (y * stride);

                    for (int x = 0; x < width; x++)
                    {
                        // Map pixel coordinates to mathematical coordinates
                        // X: left=-xRange/2, center=0, right=+xRange/2
                        double xVal = -xRange / 2.0 + deltaX * x;

                        // Y: top=+yRange/2, center=0, bottom=-yRange/2 (flipped for mathematical convention)
                        double yVal = yRange / 2.0 - deltaY * y;

                        Color color = calculatePixel(xVal, yVal);

                        // Write pixel in BGR format (24bpp RGB is actually BGR)
                        row[x * 3] = color.B;
                        row[x * 3 + 1] = color.G;
                        row[x * 3 + 2] = color.R;
                    }
                });
            }
            finally
            {
                bitmap.UnlockBits(bmpData);
            }
        }

        /// <summary>
        /// Draw axes with tick marks on a bitmap
        /// </summary>
        public static void DrawAxesOnBitmap(Bitmap bitmap, double xRange, double yRange)
        {
            using var graphics = Graphics.FromImage(bitmap);
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int width = bitmap.Width;
            int height = bitmap.Height;

            // Axis color
            using var axisPen = new Pen(Color.FromArgb(150, 200, 200, 200), 2.0f);
            using var font = new Font("Arial", 9);
            using var brush = new SolidBrush(Color.FromArgb(200, 220, 220, 220));

            double xMin = -xRange / 2.0;
            double xMax = xRange / 2.0;
            double yMin = -yRange / 2.0;
            double yMax = yRange / 2.0;

            int centerX = width / 2;
            int centerY = height / 2;
            float pixelsPerUnitX = width / (float)xRange;
            float pixelsPerUnitY = height / (float)yRange;

            // Calculate nice tick spacing
            double xTickSpacing = CalculateNiceSpacing(xRange);
            double yTickSpacing = CalculateNiceSpacing(yRange);

            // Draw X-axis
            graphics.DrawLine(axisPen, 0, centerY, width, centerY);

            // Draw Y-axis
            graphics.DrawLine(axisPen, centerX, 0, centerX, height);

            // Draw X-axis tick marks and labels
            double xStart = Math.Ceiling(xMin / xTickSpacing) * xTickSpacing;
            for (double x = xStart; x <= xMax; x += xTickSpacing)
            {
                if (Math.Abs(x) < xTickSpacing / 2) continue; // Skip origin

                float screenX = centerX + (float)(x * pixelsPerUnitX);

                // Tick mark
                graphics.DrawLine(axisPen, screenX, centerY - 5, screenX, centerY + 5);

                // Label
                string label = x.ToString("F1");
                var size = graphics.MeasureString(label, font);
                graphics.DrawString(label, font, brush, screenX - size.Width / 2, centerY + 8);
            }

            // Draw Y-axis tick marks and labels
            double yStart = Math.Ceiling(yMin / yTickSpacing) * yTickSpacing;
            for (double y = yStart; y <= yMax; y += yTickSpacing)
            {
                if (Math.Abs(y) < yTickSpacing / 2) continue; // Skip origin

                // Y-axis is flipped (positive up)
                float screenY = centerY - (float)(y * pixelsPerUnitY);

                // Tick mark
                graphics.DrawLine(axisPen, centerX - 5, screenY, centerX + 5, screenY);

                // Label
                string label = y.ToString("F1");
                var size = graphics.MeasureString(label, font);
                graphics.DrawString(label, font, brush, centerX - size.Width - 10, screenY - size.Height / 2);
            }

            // Origin label
            graphics.DrawString("0", font, brush, centerX + 5, centerY + 5);
        }

        private static double CalculateNiceSpacing(double range)
        {
            // Calculate a nice spacing value for tick marks
            double roughSpacing = range / 8.0; // Aim for about 8 divisions
            double magnitude = Math.Pow(10, Math.Floor(Math.Log10(roughSpacing)));
            double normalized = roughSpacing / magnitude;

            double niceSpacing;
            if (normalized < 1.5)
                niceSpacing = 1.0;
            else if (normalized < 3.0)
                niceSpacing = 2.0;
            else if (normalized < 7.0)
                niceSpacing = 5.0;
            else
                niceSpacing = 10.0;

            return niceSpacing * magnitude;
        }
    }
}
