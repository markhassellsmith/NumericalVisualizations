using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using static NumericalVisualizations.ScreenStructures;

namespace NumericalVisualizations.Visualizations
{
    /// <summary>
    /// Configuration for Hailstone sequence visualization
    /// </summary>
    public class HailstoneConfig : VisualizationConfig
    {
        [Category("Appearance")]
        [Description("Color progression speed (degrees per step)")]
        public int ColorSpread { get; set; } = 7;

        [Category("Algorithm")]
        [Description("Starting X coordinate in coordinate space")]
        public double StartX { get; set; } = -0.5;

        [Category("Algorithm")]
        [Description("Starting Y coordinate in coordinate space")]
        public double StartY { get; set; } = 0.3;

        [Category("Appearance")]
        [Description("Thickness of line segments")]
        public float LineWidth { get; set; } = 0.002f;

        [Category("Appearance")]
        [Description("Size of dots at segment endpoints")]
        public float DotSize { get; set; } = 0.012f;  // 30% of original 0.04

        [Category("Display - Hailstone Specific")]
        [Description("Show (N, X, Y) coordinate labels at each point, where N is the step number")]
        public bool ShowPointLabels { get; set; } = true;

        [Category("Display - Hailstone Specific")]
        [Description("Show dots at segment endpoints")]
        public bool ShowDots { get; set; } = true;

        [Category("Algorithm")]
        [Description("Scale factor for movement size")]
        public double ScaleFactor { get; set; } = 0.05;

        public HailstoneConfig()
        {
            MaxIterations = 150;
            Tolerance = 0.0;
            // Universal display settings inherited from base
            ShowAxes = true;
        }
    }

    /// <summary>
    /// Hailstone sequence (Collatz conjecture) visualization in mathematical coordinate space
    /// </summary>
    public class HailstoneVisualization : IVisualization
    {
        private readonly HailstoneConfig _config;

        public string Name => "Hailstone Sequence";
        public string Description => "2D visualization of the Collatz conjecture (3n+1 problem) in Cartesian coordinates";

        public HailstoneVisualization(HailstoneConfig? config = null)
        {
            _config = config ?? new HailstoneConfig();
        }

        public VisualizationConfig GetConfig()
        {
            return _config;
        }

        public IVisualization WithConfig(VisualizationConfig config)
        {
            return new HailstoneVisualization(config as HailstoneConfig);
        }

        public Bitmap Render(int width, int height, double xRange, double yRange)
        {
            var bitmap = new Bitmap(width, height);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.Black);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            // First pass: Calculate all points and determine bounds
            var points = new List<(int step, double x, double y, Color color)>();

            double currentX = _config.StartX;
            double currentY = _config.StartY;
            int intX = (int)Math.Round(currentX / _config.ScaleFactor);
            int intY = (int)Math.Round(currentY / _config.ScaleFactor);

            points.Add((0, currentX, currentY, Color.Red));

            for (int index = 1; index <= _config.MaxIterations; index++)
            {
                var cs = ColorPalettes.Spectrum360[(index * _config.ColorSpread) % 360];
                Color lineColor = Color.FromArgb(cs.red, cs.green, cs.blue);

                int nextIntX = Functions.FHailStoneNextX(intX, intY);
                int nextIntY = Functions.FHailStoneNextY(intX, intY);

                double nextX = nextIntX * _config.ScaleFactor;
                double nextY = nextIntY * _config.ScaleFactor;

                points.Add((index, nextX, nextY, lineColor));

                currentX = nextX;
                currentY = nextY;
                intX = nextIntX;
                intY = nextIntY;

                if (intX == 1 && intY == 1) break;
            }

            // Calculate bounds with padding
            double minX = points.Min(p => p.x);
            double maxX = points.Max(p => p.x);
            double minY = points.Min(p => p.y);
            double maxY = points.Max(p => p.y);

            // Add 10% padding on each side
            double rangeX = maxX - minX;
            double rangeY = maxY - minY;
            double paddingX = rangeX * 0.15;
            double paddingY = rangeY * 0.15;

            minX -= paddingX;
            maxX += paddingX;
            minY -= paddingY;
            maxY += paddingY;

            // Calculate center and range
            double centerX = (minX + maxX) / 2.0;
            double centerY = (minY + maxY) / 2.0;
            double dataRangeX = maxX - minX;
            double dataRangeY = maxY - minY;

            // Ensure minimum range to avoid division by zero or extreme zoom
            dataRangeX = Math.Max(dataRangeX, 0.1);
            dataRangeY = Math.Max(dataRangeY, 0.1);

            // Maintain 1:1 aspect ratio (equal X and Y units) to preserve true shape
            // Use the larger of the two ranges for both axes
            double maxRange = Math.Max(dataRangeX, dataRangeY);

            // Adjust both ranges to be equal (square viewing area in data space)
            dataRangeX = maxRange;
            dataRangeY = maxRange;

            // Save the original state for drawing labels in screen coordinates
            var originalTransform = graphics.Transform.Clone();

            // Calculate transform parameters for later use
            float pixelsPerUnitX = width / (float)dataRangeX;
            float pixelsPerUnitY = height / (float)dataRangeY;
            int screenCenterX = width / 2;
            int screenCenterY = height / 2;

            // Transform to use mathematical coordinate system with Y-up, centered on data
            graphics.TranslateTransform(width / 2.0f, height / 2.0f);  // Origin at screen center
            graphics.ScaleTransform(pixelsPerUnitX, -pixelsPerUnitY);  // Scale and flip Y
            graphics.TranslateTransform(-(float)centerX, -(float)centerY);  // Center on data

            // Draw axes if enabled
            if (_config.ShowAxes)
            {
                DrawAxes(graphics, dataRangeX, dataRangeY, centerX, centerY);
            }

            // Second pass: Draw lines and dots using calculated points
            float penWidth = _config.LineWidth;
            using Pen drawingPen = new Pen(Color.Red, penWidth);
            using SolidBrush dotBrush = new SolidBrush(Color.White);

            // Draw lines
            for (int i = 0; i < points.Count - 1; i++)
            {
                var (step1, x1, y1, color1) = points[i];
                var (step2, x2, y2, color2) = points[i + 1];

                drawingPen.Color = color2;
                graphics.DrawLine(drawingPen, (float)x1, (float)y1, (float)x2, (float)y2);
            }

            // Draw dots at each segment end
            if (_config.ShowDots)
            {
                float dotRadius = _config.DotSize / 2.0f;
                foreach (var (step, x, y, color) in points)
                {
                    dotBrush.Color = color;
                    graphics.FillEllipse(dotBrush, 
                        (float)x - dotRadius, (float)y - dotRadius, 
                        _config.DotSize, _config.DotSize);
                }
            }

            // Restore original transform for labels
            graphics.Transform = originalTransform;

            // Always draw axis tick labels if axes are shown
            if (_config.ShowAxes)
            {
                DrawAxisLabels(graphics, width, height, dataRangeX, dataRangeY, centerX, centerY,
                    screenCenterX, screenCenterY, pixelsPerUnitX, pixelsPerUnitY);
            }

            // Draw point coordinate labels on top if enabled
            if (_config.ShowPointLabels)
            {
                DrawPointLabels(graphics, points, screenCenterX, screenCenterY, 
                    pixelsPerUnitX, pixelsPerUnitY, centerX, centerY);
            }

            return bitmap;
        }

        private void DrawAxes(Graphics graphics, double xRange, double yRange, double centerX, double centerY)
        {
            // Axis color
            using var axisPen = new Pen(Color.FromArgb(150, 200, 200, 200), 0.01f);

            double xMin = centerX - xRange / 2.0;
            double xMax = centerX + xRange / 2.0;
            double yMin = centerY - yRange / 2.0;
            double yMax = centerY + yRange / 2.0;

            // Calculate nice tick spacing
            double xTickSpacing = CalculateNiceSpacing(xRange);
            double yTickSpacing = CalculateNiceSpacing(yRange);

            // Draw main axes (only if they're visible in the current view)
            // X-axis (y=0)
            if (yMin <= 0 && yMax >= 0)
            {
                graphics.DrawLine(axisPen, (float)xMin, 0, (float)xMax, 0);

                // X-axis ticks
                float tickSize = 0.03f * (float)yRange;
                double xStart = Math.Ceiling(xMin / xTickSpacing) * xTickSpacing;
                for (double x = xStart; x <= xMax; x += xTickSpacing)
                {
                    if (Math.Abs(x) > xTickSpacing / 2) // Skip origin
                        graphics.DrawLine(axisPen, (float)x, -tickSize, (float)x, tickSize);
                }
            }

            // Y-axis (x=0)
            if (xMin <= 0 && xMax >= 0)
            {
                graphics.DrawLine(axisPen, 0, (float)yMin, 0, (float)yMax);

                // Y-axis ticks
                float tickSize = 0.03f * (float)xRange;
                double yStart = Math.Ceiling(yMin / yTickSpacing) * yTickSpacing;
                for (double y = yStart; y <= yMax; y += yTickSpacing)
                {
                    if (Math.Abs(y) > yTickSpacing / 2) // Skip origin
                        graphics.DrawLine(axisPen, -tickSize, (float)y, tickSize, (float)y);
                }
            }
        }

        private void DrawPointLabels(Graphics graphics, List<(int step, double x, double y, Color color)> points, 
            int screenCenterX, int screenCenterY, float pixelsPerUnitX, float pixelsPerUnitY,
            double dataCenterX, double dataCenterY)
        {
            using var font = new Font("Arial", 8);
            using var brush = new SolidBrush(Color.White);
            using var backBrush = new SolidBrush(Color.FromArgb(180, 0, 0, 0)); // Semi-transparent background

            foreach (var (step, x, y, color) in points)
            {
                // Convert mathematical coordinates to screen coordinates
                float screenX = screenCenterX + (float)((x - dataCenterX) * pixelsPerUnitX);
                float screenY = screenCenterY - (float)((y - dataCenterY) * pixelsPerUnitY);

                // Format label as (N, X, Y)
                string label = $"({step}, {x:F2}, {y:F2})";

                // Measure text size for background
                SizeF textSize = graphics.MeasureString(label, font);

                // Position label offset from the point (bottom-right)
                float labelX = screenX + 8;
                float labelY = screenY + 2;

                // Draw semi-transparent background for readability
                graphics.FillRectangle(backBrush, 
                    labelX - 2, labelY - 2, 
                    textSize.Width + 4, textSize.Height + 4);

                // Draw label
                brush.Color = Color.FromArgb(255, 220, 220, 220);
                graphics.DrawString(label, font, brush, labelX, labelY);
            }
        }

        private void DrawAxisLabels(Graphics graphics, int width, int height, double xRange, double yRange,
            double dataCenterX, double dataCenterY, int screenCenterX, int screenCenterY,
            float pixelsPerUnitX, float pixelsPerUnitY)
        {
            using var font = new Font("Arial", 9);
            using var brush = new SolidBrush(Color.FromArgb(200, 220, 220, 220));
            using var format = new StringFormat { Alignment = StringAlignment.Center };

            double xTickSpacing = CalculateNiceSpacing(xRange);
            double yTickSpacing = CalculateNiceSpacing(yRange);

            double xMin = dataCenterX - xRange / 2.0;
            double xMax = dataCenterX + xRange / 2.0;
            double yMin = dataCenterY - yRange / 2.0;
            double yMax = dataCenterY + yRange / 2.0;

            // X-axis labels (only if X-axis is visible)
            if (yMin <= 0 && yMax >= 0)
            {
                double xStart = Math.Ceiling(xMin / xTickSpacing) * xTickSpacing;
                for (double x = xStart; x <= xMax; x += xTickSpacing)
                {
                    if (Math.Abs(x) < xTickSpacing / 2) continue; // Skip origin
                    float screenX = screenCenterX + (float)((x - dataCenterX) * pixelsPerUnitX);
                    float screenY = screenCenterY - (float)((0 - dataCenterY) * pixelsPerUnitY);
                    string label = x.ToString("F1");
                    graphics.DrawString(label, font, brush, screenX, screenY + 15, format);
                }
            }

            // Y-axis labels (only if Y-axis is visible)
            if (xMin <= 0 && xMax >= 0)
            {
                format.Alignment = StringAlignment.Far;
                double yStart = Math.Ceiling(yMin / yTickSpacing) * yTickSpacing;
                for (double y = yStart; y <= yMax; y += yTickSpacing)
                {
                    if (Math.Abs(y) < yTickSpacing / 2) continue; // Skip origin
                    float screenX = screenCenterX + (float)((0 - dataCenterX) * pixelsPerUnitX);
                    float screenY = screenCenterY - (float)((y - dataCenterY) * pixelsPerUnitY);
                    string label = y.ToString("F1");
                    graphics.DrawString(label, font, brush, screenX - 10, screenY - 7, format);
                }
            }

            // Origin label (only if visible)
            if (xMin <= 0 && xMax >= 0 && yMin <= 0 && yMax >= 0)
            {
                format.Alignment = StringAlignment.Far;
                float screenX = screenCenterX + (float)((0 - dataCenterX) * pixelsPerUnitX);
                float screenY = screenCenterY - (float)((0 - dataCenterY) * pixelsPerUnitY);
                graphics.DrawString("0", font, brush, screenX - 5, screenY + 5, format);
            }
        }

        private double CalculateNiceSpacing(double range)
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
