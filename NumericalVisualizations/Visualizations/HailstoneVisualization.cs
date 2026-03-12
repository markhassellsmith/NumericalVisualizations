using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using static NumericalVisualizations.ScreenStructures;

namespace NumericalVisualizations.Visualizations
{
    /// <summary>
    /// Configuration for Hailstone sequence visualization
    /// </summary>
    public class HailstoneConfig : VisualizationConfig
    {
        [Category("Algorithm")]
        [Description("Starting X coordinate (INTEGER) - the actual integer used in Hailstone rules")]
        public int StartIntX { get; set; } = -10;

        [Category("Algorithm")]
        [Description("Starting Y coordinate (INTEGER) - the actual integer used in Hailstone rules")]
        public int StartIntY { get; set; } = 6;

        [Category("Appearance")]
        [Description("Color progression speed (degrees per step)")]
        public int ColorSpread { get; set; } = 7;

        [Category("Advanced - Display")]
        [Description("X-axis scale factor (0 = auto-calculate based on sequence width)")]
        public double ScaleFactorX { get; set; } = 0.0;

        [Category("Advanced - Display")]
        [Description("Y-axis scale factor (0 = auto-calculate based on sequence height)")]
        public double ScaleFactorY { get; set; } = 0.0;

        [Category("Advanced - Display")]
        [Description("Unified scale factor (DEPRECATED - sets both X and Y equally)")]
        [Browsable(false)]
        public double ScaleFactor
        {
            get => (ScaleFactorX + ScaleFactorY) / 2;
            set { ScaleFactorX = value; ScaleFactorY = value; }
        }

        [Category("Algorithm")]
        [Description("Starting X coordinate in coordinate space (DEPRECATED - use StartIntX instead)")]
        [Browsable(false)]  // Hide from PropertyGrid
        public double StartX 
        { 
            get => StartIntX * ((ScaleFactorX > 0) ? ScaleFactorX : 0.05);
            set => StartIntX = (int)Math.Round(value / ((ScaleFactorX > 0) ? ScaleFactorX : 0.05));
        }

        [Category("Algorithm")]
        [Description("Starting Y coordinate in coordinate space (DEPRECATED - use StartIntY instead)")]
        [Browsable(false)]  // Hide from PropertyGrid
        public double StartY 
        { 
            get => StartIntY * ((ScaleFactorY > 0) ? ScaleFactorY : 0.05);
            set => StartIntY = (int)Math.Round(value / ((ScaleFactorY > 0) ? ScaleFactorY : 0.05));
        }

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
        [Description("Detect and highlight cycles (when sequence returns to a previous point)")]
        public bool DetectCycles { get; set; } = true;

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

            // First pass: Calculate all integer points (unscaled) to determine actual range
            var intPoints = new List<(int step, int intX, int intY, Color color)>();
            var visitedPoints = new HashSet<(int, int)>();  // For cycle detection

            int intX = _config.StartIntX;
            int intY = _config.StartIntY;
            intPoints.Add((0, intX, intY, Color.Red));
            visitedPoints.Add((intX, intY));

            int cycleStartStep = -1;
            int cycleEndStep = -1;
            (int cycleX, int cycleY) = (0, 0);

            for (int index = 1; index <= _config.MaxIterations; index++)
            {
                var cs = ColorPalettes.Spectrum360[(index * _config.ColorSpread) % 360];
                Color lineColor = Color.FromArgb(cs.red, cs.green, cs.blue);

                int nextIntX = Functions.FHailStoneNextX(intX, intY);
                int nextIntY = Functions.FHailStoneNextY(intX, intY);

                intPoints.Add((index, nextIntX, nextIntY, lineColor));

                intX = nextIntX;
                intY = nextIntY;

                // Cycle detection
                if (_config.DetectCycles && visitedPoints.Contains((intX, intY)))
                {
                    // Found a cycle! Find where it started
                    cycleEndStep = index;
                    cycleX = intX;
                    cycleY = intY;

                    for (int i = 0; i < intPoints.Count; i++)
                    {
                        if (intPoints[i].intX == intX && intPoints[i].intY == intY)
                        {
                            cycleStartStep = i;
                            break;
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"CYCLE DETECTED! Point ({intX}, {intY}) repeats at step {cycleEndStep}, first seen at step {cycleStartStep}. Cycle length: {cycleEndStep - cycleStartStep}");
                    break;
                }

                visitedPoints.Add((intX, intY));
            }

            // Export sequence points to CSV file for verification (include cycle info)
            ExportPointsToCSV(intPoints, cycleStartStep, cycleEndStep, cycleX, cycleY);

            // Auto-calculate scale factors if set to 0
            double scaleX = _config.ScaleFactorX;
            double scaleY = _config.ScaleFactorY;

            if (scaleX == 0.0 || scaleY == 0.0)
            {
                // Focus on EARLY iterations to keep view near starting point
                // Use first 30% of sequence (or 50 iterations max, whichever is less)
                int iterationsForScaling = Math.Min(50, Math.Max(10, intPoints.Count * 30 / 100));
                var earlyPoints = intPoints.Take(iterationsForScaling).ToList();

                // Find integer coordinate ranges from early behavior only
                int minIntX = earlyPoints.Min(p => p.intX);
                int maxIntX = earlyPoints.Max(p => p.intX);
                int minIntY = earlyPoints.Min(p => p.intY);
                int maxIntY = earlyPoints.Max(p => p.intY);

                int rangeIntX = maxIntX - minIntX;
                int rangeIntY = maxIntY - minIntY;

                // Calculate scales to fit in ~3 unit space (with padding)
                if (scaleX == 0.0)
                    scaleX = rangeIntX > 0 ? 3.0 / rangeIntX : 0.05;
                if (scaleY == 0.0)
                    scaleY = rangeIntY > 0 ? 3.0 / rangeIntY : 0.05;
            }

            // Second pass: Convert integer points to scaled coordinates for rendering
            var points = new List<(int step, double x, double y, Color color)>();
            foreach (var (step, ix, iy, color) in intPoints)
            {
                points.Add((step, ix * scaleX, iy * scaleY, color));
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
            using Pen cyclePen = new Pen(Color.Magenta, penWidth * 2.5f);  // Thicker, bright magenta for cycle
            using SolidBrush dotBrush = new SolidBrush(Color.White);

            // Draw lines
            for (int i = 0; i < points.Count - 1; i++)
            {
                var (step1, x1, y1, color1) = points[i];
                var (step2, x2, y2, color2) = points[i + 1];

                // Check if this segment is part of the cycle
                bool isInCycle = cycleStartStep >= 0 && i >= cycleStartStep && i < cycleEndStep;

                if (isInCycle)
                {
                    graphics.DrawLine(cyclePen, (float)x1, (float)y1, (float)x2, (float)y2);
                }
                else
                {
                    drawingPen.Color = color2;
                    graphics.DrawLine(drawingPen, (float)x1, (float)y1, (float)x2, (float)y2);
                }
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

            // Always draw axis tick labels if axes are shown (using INTEGER coordinates)
            if (_config.ShowAxes)
            {
                DrawAxisLabelsInIntegerSpace(graphics, width, height, intPoints, 
                    dataRangeX, dataRangeY, centerX, centerY,
                    screenCenterX, screenCenterY, pixelsPerUnitX, pixelsPerUnitY,
                    scaleX, scaleY);
            }

            // Draw point coordinate labels on top if enabled
            if (_config.ShowPointLabels)
            {
                DrawPointLabelsInIntegerSpace(graphics, intPoints, screenCenterX, screenCenterY, 
                    pixelsPerUnitX, pixelsPerUnitY, centerX, centerY, scaleX, scaleY);
            }

            // Draw sequence information overlay (matches CSV header format)
            using var font = new Font("Arial", 14, FontStyle.Bold);
            using var brush = new SolidBrush(Color.Yellow);
            using var backBrush = new SolidBrush(Color.FromArgb(220, 0, 0, 0));

            string infoText = $"Hailstone Sequence (N,X,Y)\n" +
                             $"Starting point: (0, {intPoints[0].intX}, {intPoints[0].intY})\n" +
                             $"Total points: {intPoints.Count}";

            if (cycleStartStep >= 0)
            {
                int cycleLength = cycleEndStep - cycleStartStep;
                infoText += $"\nCycle Detected: Point ({cycleEndStep}, {cycleX}, {cycleY})\n" +
                           $"Duplicate of: ({cycleStartStep}, {cycleX}, {cycleY})\n" +
                           $"Cycle length: {cycleLength}";
                brush.Color = Color.Magenta;  // Use magenta for cycle info
            }

            var textSize = graphics.MeasureString(infoText, font);
            float textX = 10;
            float textY = 10;

            graphics.FillRectangle(backBrush, textX - 5, textY - 5, textSize.Width + 10, textSize.Height + 10);
            graphics.DrawString(infoText, font, brush, textX, textY);

            return bitmap;
        }

        private void DrawPointLabelsInIntegerSpace(Graphics graphics, List<(int step, int intX, int intY, Color color)> intPoints,
            int screenCenterX, int screenCenterY, float pixelsPerUnitX, float pixelsPerUnitY,
            double dataCenterX, double dataCenterY, double scaleX, double scaleY)
        {
            using var font = new Font("Arial", 8);
            using var brush = new SolidBrush(Color.White);
            using var backBrush = new SolidBrush(Color.FromArgb(180, 0, 0, 0));

            foreach (var (step, intX, intY, color) in intPoints)
            {
                // Convert integer to scaled coordinates for positioning
                double x = intX * scaleX;
                double y = intY * scaleY;

                float screenX = screenCenterX + (float)((x - dataCenterX) * pixelsPerUnitX);
                float screenY = screenCenterY - (float)((y - dataCenterY) * pixelsPerUnitY);

                // Format label with INTEGER coordinates
                string label = $"({step}, {intX}, {intY})";

                SizeF textSize = graphics.MeasureString(label, font);
                float labelX = screenX + 8;
                float labelY = screenY + 2;

                graphics.FillRectangle(backBrush, 
                    labelX - 2, labelY - 2, 
                    textSize.Width + 4, textSize.Height + 4);

                brush.Color = Color.FromArgb(255, 220, 220, 220);
                graphics.DrawString(label, font, brush, labelX, labelY);
            }
        }

        private void DrawAxisLabelsInIntegerSpace(Graphics graphics, int width, int height, 
            List<(int step, int intX, int intY, Color color)> intPoints,
            double xRange, double yRange, double dataCenterX, double dataCenterY, 
            int screenCenterX, int screenCenterY, float pixelsPerUnitX, float pixelsPerUnitY,
            double scaleX, double scaleY)
        {
            using var font = new Font("Arial", 9);
            using var brush = new SolidBrush(Color.FromArgb(200, 220, 220, 220));
            using var format = new StringFormat { Alignment = StringAlignment.Center };

            // Find integer coordinate ranges
            int minIntX = intPoints.Min(p => p.intX);
            int maxIntX = intPoints.Max(p => p.intX);
            int minIntY = intPoints.Min(p => p.intY);
            int maxIntY = intPoints.Max(p => p.intY);

            int intRangeX = maxIntX - minIntX;
            int intRangeY = maxIntY - minIntY;

            // Calculate nice integer tick spacing (powers of 10: 1, 2, 5, 10, 20, 50, etc.)
            int xTickSpacing = CalculateNiceIntegerSpacing(intRangeX);
            int yTickSpacing = CalculateNiceIntegerSpacing(intRangeY);

            // Draw X-axis labels (positioned along y=0 axis)
            int xStart = ((minIntX / xTickSpacing) - 1) * xTickSpacing;
            int xEnd = ((maxIntX / xTickSpacing) + 1) * xTickSpacing;

            for (int intX = xStart; intX <= xEnd; intX += xTickSpacing)
            {
                if (intX == 0) continue; // Skip origin

                // Convert integer coordinate to scaled then to screen
                double scaledX = intX * scaleX;
                float screenX = screenCenterX + (float)((scaledX - dataCenterX) * pixelsPerUnitX);

                // Position label ON the X-axis (y=0 in data space)
                double scaled_Y_Zero = 0.0;
                float screenY = screenCenterY - (float)((scaled_Y_Zero - dataCenterY) * pixelsPerUnitY);

                // Only draw if on screen
                if (screenX >= 0 && screenX <= width)
                {
                    graphics.DrawString(intX.ToString(), font, brush, screenX, screenY + 5, format);
                }
            }

            // Draw Y-axis labels (positioned along x=0 axis)
            int yStart = ((minIntY / yTickSpacing) - 1) * yTickSpacing;
            int yEnd = ((maxIntY / yTickSpacing) + 1) * yTickSpacing;

            for (int intY = yStart; intY <= yEnd; intY += yTickSpacing)
            {
                if (intY == 0) continue; // Skip origin

                // Convert integer coordinate to scaled then to screen
                double scaledY = intY * scaleY;
                float screenY = screenCenterY - (float)((scaledY - dataCenterY) * pixelsPerUnitY);

                // Position label ON the Y-axis (x=0 in data space)
                double scaled_X_Zero = 0.0;
                float screenX = screenCenterX + (float)((scaled_X_Zero - dataCenterX) * pixelsPerUnitX);

                // Only draw if on screen
                if (screenY >= 0 && screenY <= height)
                {
                    format.Alignment = StringAlignment.Far;
                    graphics.DrawString(intY.ToString(), font, brush, screenX - 5, screenY - 8, format);
                }
            }
        }

        private int CalculateNiceIntegerSpacing(int range)
        {
            if (range <= 0) return 1;

            // For very small ranges, just use spacing of 1
            if (range <= 7) return 1;

            // Target ~5-10 tick marks
            int roughSpacing = range / 7;

            // Protect against roughSpacing being 0 (should not happen now, but safety check)
            if (roughSpacing == 0) return 1;

            // Round to nice numbers: 1, 2, 5, 10, 20, 50, 100, etc.
            int magnitude = (int)Math.Pow(10, Math.Floor(Math.Log10(roughSpacing)));

            // Protect against magnitude being 0
            if (magnitude == 0) magnitude = 1;

            int normalized = roughSpacing / magnitude;

            if (normalized <= 1) return magnitude;
            if (normalized <= 2) return 2 * magnitude;
            if (normalized <= 5) return 5 * magnitude;
            return 10 * magnitude;
        }

        private void DrawAxes(Graphics graphics, double xRange, double yRange, double centerX, double centerY)
        {
            // Axis color
            using var axisPen = new Pen(Color.FromArgb(150, 200, 200, 200), 0.01f);

            double xMin = centerX - xRange / 2.0;
            double xMax = centerX + xRange / 2.0;
            double yMin = centerY - yRange / 2.0;
            double yMax = centerY + yRange / 2.0;

            // Draw main axes (only if they're visible in the current view)
            // X-axis (y=0)
            if (yMin <= 0 && yMax >= 0)
            {
                graphics.DrawLine(axisPen, (float)xMin, 0, (float)xMax, 0);
            }

            // Y-axis (x=0)
            if (xMin <= 0 && xMax >= 0)
            {
                graphics.DrawLine(axisPen, 0, (float)yMin, 0, (float)yMax);
            }
        }

        /// <summary>
        /// Export Hailstone sequence points to CSV file for analysis and verification
        /// </summary>
        private void ExportPointsToCSV(List<(int step, int intX, int intY, Color color)> intPoints, 
                                       int cycleStartStep, int cycleEndStep, int cycleX, int cycleY)
        {
            try
            {
                var path = @"C:\Temp\Fractal Copies\New Project Effort\hailstone_points.csv";

                // Ensure directory exists
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (var writer = new StreamWriter(path))
                {
                    // Write header comments with sequence info - no commas to avoid CSV parsing issues
                    writer.WriteLine("# Hailstone Sequence (N X Y)");
                    writer.WriteLine($"# Starting point: (0 {intPoints[0].intX} {intPoints[0].intY})");
                    writer.WriteLine($"# Total points: {intPoints.Count}");

                    if (cycleStartStep >= 0)
                    {
                        int cycleLength = cycleEndStep - cycleStartStep;
                        writer.WriteLine($"# Cycle Detected: Point ({cycleEndStep} {cycleX} {cycleY})");
                        writer.WriteLine($"# Duplicate of: ({cycleStartStep} {cycleX} {cycleY})");
                        writer.WriteLine($"# Cycle length: {cycleLength}");
                    }
                    else
                    {
                        writer.WriteLine($"# No cycle detected - stopped at MaxIterations");
                    }

                    writer.WriteLine("#");

                    // Header row with spaces after commas
                    writer.WriteLine("N, X, Y");

                    // Data rows: step number and integer coordinates
                    for (int i = 0; i < intPoints.Count; i++)
                    {
                        var p = intPoints[i];
                        writer.WriteLine($"{p.step}, {p.intX}, {p.intY}");
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Exported {intPoints.Count} points to {path}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to export CSV: {ex.Message}");
                // Don't crash visualization if CSV export fails
            }
        }
    }
}
