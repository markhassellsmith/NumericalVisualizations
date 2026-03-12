using System.Diagnostics;
using System.IO;
using System.Text;
using NumericalVisualizations.Visualizations;
using NumericalVisualizations.Performance;

namespace NumericalVisualizations
{
    public partial class Canvas : Form
    {
        #region CanvasVariables

        private const int MaxHorizontal = 2000;
        private const int MaxVertical = 2000;
        private const int XWidth = 4;
        private const int YHeight = 4;

        private VisualizationFactory.VisualizationType _currentVisualizationType = VisualizationFactory.VisualizationType.Newton;
        private IVisualization? _currentVisualization;
        private Bitmap? _bmp;
        private Bitmap? _cachedBaseVisualization;  // Cache of visualization without overlays

        private bool _imageRendered = false;
        private bool _renderingInProgress = false;
        private Task? _renderTask;
        private bool _visualizationSelected = false;  // Track if user has selected a visualization

        private Point _pdown;
        private Point _pup;

        #endregion CanvasVariables

        #region CanvasMethods

        public Canvas()
        {
            InitializeComponent();
            panel1.Dock = DockStyle.Fill;
            panel1.BackgroundImageLayout = ImageLayout.Zoom;
            panel1.Paint += Panel1_Paint;  // Add paint handler for welcome message
            _currentVisualization = null;  // Don't create visualization until user selects one

            // Initialize toolbar as disabled
            UpdateToolbarFromConfig();

            // Use professional renderer that respects custom colors
            toolStrip1.Renderer = new ToolStripProfessionalRenderer(new CustomColorTable());
        }

        // Custom color table for checked toolbar buttons
        private class CustomColorTable : ProfessionalColorTable
        {
            public override Color ButtonSelectedHighlight => Color.FromArgb(200, 220, 240);
            public override Color ButtonSelectedGradientBegin => Color.FromArgb(200, 220, 240);
            public override Color ButtonSelectedGradientEnd => Color.FromArgb(200, 220, 240);
            public override Color ButtonSelectedGradientMiddle => Color.FromArgb(200, 220, 240);
            public override Color ButtonCheckedHighlight => Color.FromArgb(180, 210, 240);
            public override Color ButtonCheckedGradientBegin => Color.FromArgb(180, 210, 240);
            public override Color ButtonCheckedGradientEnd => Color.FromArgb(180, 210, 240);
            public override Color ButtonCheckedGradientMiddle => Color.FromArgb(180, 210, 240);
            public override Color ButtonPressedHighlight => Color.FromArgb(150, 200, 240);
            public override Color ButtonPressedGradientBegin => Color.FromArgb(150, 200, 240);
            public override Color ButtonPressedGradientEnd => Color.FromArgb(150, 200, 240);
            public override Color ButtonPressedGradientMiddle => Color.FromArgb(150, 200, 240);
        }

        /// <summary>
        /// Change the current visualization type
        /// </summary>
        public void SetVisualization(VisualizationFactory.VisualizationType type)
        {
            _currentVisualizationType = type;
            _currentVisualization = VisualizationFactory.Create(type);
            _visualizationSelected = true;
            _imageRendered = false;
            _renderingInProgress = false;  // Reset to allow new render

            // Invalidate cache when changing visualizations
            _cachedBaseVisualization?.Dispose();
            _cachedBaseVisualization = null;

            // Clear the old image immediately
            panel1.BackgroundImage?.Dispose();
            panel1.BackgroundImage = null;

            UpdateToolbarFromConfig();

            // Trigger repaint
            Invalidate();
        }

        /// <summary>
        /// Update toolbar button states from current visualization config
        /// </summary>
        private void UpdateToolbarFromConfig()
        {
            if (_currentVisualization == null)
            {
                axesToolStripButton.Checked = false;
                pointLabelsToolStripButton.Checked = false;
                pointLabelsToolStripButton.Enabled = false;
                dotsToolStripButton.Checked = false;
                dotsToolStripButton.Enabled = false;
                return;
            }

            var config = _currentVisualization.GetConfig();

            // Universal option (all visualizations)
            axesToolStripButton.Checked = config.ShowAxes;

            // Hailstone-specific options
            if (config is HailstoneConfig hailstoneConfig)
            {
                pointLabelsToolStripButton.Enabled = true;
                pointLabelsToolStripButton.Checked = hailstoneConfig.ShowPointLabels;
                dotsToolStripButton.Enabled = true;
                dotsToolStripButton.Checked = hailstoneConfig.ShowDots;
            }
            else
            {
                pointLabelsToolStripButton.Enabled = false;
                pointLabelsToolStripButton.Checked = false;
                dotsToolStripButton.Enabled = false;
                dotsToolStripButton.Checked = false;
            }
        }

        /// <summary>
        /// Load a preset configuration for the current visualization
        /// </summary>
        public void LoadPreset(string presetName)
        {
            var config = VisualizationPresets.GetPreset(_currentVisualizationType, presetName);
            if (config != null && _currentVisualization != null)
            {
                _currentVisualization = _currentVisualization.WithConfig(config);
                _imageRendered = false;
                UpdateToolbarFromConfig();  // Update toolbar to match preset
                Invalidate();
            }
        }

        private void Panel1_Paint(object sender, PaintEventArgs e)
        {
            // Show welcome message if no visualization selected
            if (!_visualizationSelected)
            {
                e.Graphics.Clear(Color.Black);

                string message = "Select a visualization from the Visualizations menu or press Ctrl+1, Ctrl+2, or Ctrl+3";

                using var font = new Font("Segoe UI", 14, FontStyle.Regular);
                using var brush = new SolidBrush(Color.FromArgb(200, 200, 200));

                var size = e.Graphics.MeasureString(message, font, panel1.Width - 100);
                float x = (panel1.Width - size.Width) / 2;
                float y = (panel1.Height - size.Height) / 2;

                e.Graphics.DrawString(message, font, brush, new RectangleF(x, y, size.Width, size.Height));
            }
        }

        private void Canvas_Paint(object sender, PaintEventArgs e)
        {
            // Don't render if user hasn't selected a visualization yet
            if (!_visualizationSelected) return;

            if (_imageRendered || _renderingInProgress || _currentVisualization == null) return;

            _renderingInProgress = true;
            _renderTask = Task.Run(() =>
            {
                try
                {
                    int panelW = Math.Max(1, panel1.ClientSize.Width);
                    int panelH = Math.Max(1, panel1.ClientSize.Height);
                    double panelRatio = panelW / (double)panelH;
                    double baseRatio = MaxHorizontal / (double)MaxVertical;
                    int targetW, targetH;

                    if (panelRatio >= baseRatio)
                    {
                        targetH = MaxVertical;
                        targetW = Math.Max(1, (int)Math.Round(MaxVertical * panelRatio));
                    }
                    else
                    {
                        targetW = MaxHorizontal;
                        targetH = Math.Max(1, (int)Math.Round(MaxHorizontal / panelRatio));
                    }

                    var renderBmp = _currentVisualization.Render(targetW, targetH, XWidth, YHeight);

                    // Cache the base visualization (without axes overlay) for fast axes toggling
                    // Only cache for Newton/Mandelbrot (fractals with overlay axes)
                    // Don't cache Hailstone - its axes are integrated into rendering
                    var config = _currentVisualization.GetConfig();
                    if (config is not HailstoneConfig)
                    {
                        _cachedBaseVisualization?.Dispose();
                        _cachedBaseVisualization = new Bitmap(renderBmp);
                    }

                    BeginInvoke(() =>
                    {
                        try
                        {
                            _bmp?.Dispose();
                            _bmp = renderBmp;
                            panel1.BackgroundImage = _bmp;
                            _imageRendered = true;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("UI update failed: " + ex.Message);
                        }
                        finally
                        {
                            _renderingInProgress = false;
                        }
                    });
                }
                catch (Exception ex)
                {
                    BeginInvoke(() =>
                    {
                        MessageBox.Show("Rendering failed: " + ex.Message);
                        _renderingInProgress = false;
                    });
                }
            });
        }

        #endregion CanvasMethods

        #region MouseMethods

        private void Canvas_MouseDown(object sender, MouseEventArgs e)
        {
            _pdown = e.Location;
        }

        private void Canvas_MouseUp(object sender, MouseEventArgs e)
        {
            _pup = e.Location;
        }

        #endregion MouseMethods

        #region MenuEventHandlers

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void newtonsMethodToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Only switch if not already on Newton (clicking on parent while dropdown is open)
            if (sender is ToolStripMenuItem menuItem && menuItem.DropDownItems.Count == 0)
            {
                SetVisualization(VisualizationFactory.VisualizationType.Newton);
            }
        }

        private void newtonsMethodToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            var menuItem = (ToolStripMenuItem)sender;
            menuItem.DropDownItems.Clear();

            // Add presets for Newton
            var presetNames = VisualizationPresets.GetPresetNames(VisualizationFactory.VisualizationType.Newton);
            foreach (var presetName in presetNames)
            {
                var presetItem = new ToolStripMenuItem(presetName);
                presetItem.Click += (s, args) =>
                {
                    SetVisualization(VisualizationFactory.VisualizationType.Newton);
                    LoadPreset(presetName);
                };
                menuItem.DropDownItems.Add(presetItem);
            }

            // Add separator and Settings
            menuItem.DropDownItems.Add(new ToolStripSeparator());
            var settingsItem = new ToolStripMenuItem("Settings...");
            settingsItem.Click += (s, args) =>
            {
                if (_currentVisualizationType != VisualizationFactory.VisualizationType.Newton)
                {
                    SetVisualization(VisualizationFactory.VisualizationType.Newton);
                }
                OpenSettings();
            };
            menuItem.DropDownItems.Add(settingsItem);
        }

        private void mandelbrotSetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Only switch if not already on Mandelbrot (clicking on parent while dropdown is open)
            if (sender is ToolStripMenuItem menuItem && menuItem.DropDownItems.Count == 0)
            {
                SetVisualization(VisualizationFactory.VisualizationType.Mandelbrot);
            }
        }

        private void mandelbrotSetToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            var menuItem = (ToolStripMenuItem)sender;
            menuItem.DropDownItems.Clear();

            // Add presets for Mandelbrot
            var presetNames = VisualizationPresets.GetPresetNames(VisualizationFactory.VisualizationType.Mandelbrot);
            foreach (var presetName in presetNames)
            {
                var presetItem = new ToolStripMenuItem(presetName);
                presetItem.Click += (s, args) =>
                {
                    SetVisualization(VisualizationFactory.VisualizationType.Mandelbrot);
                    LoadPreset(presetName);
                };
                menuItem.DropDownItems.Add(presetItem);
            }

            // Add separator and Settings
            menuItem.DropDownItems.Add(new ToolStripSeparator());
            var settingsItem = new ToolStripMenuItem("Settings...");
            settingsItem.Click += (s, args) =>
            {
                if (_currentVisualizationType != VisualizationFactory.VisualizationType.Mandelbrot)
                {
                    SetVisualization(VisualizationFactory.VisualizationType.Mandelbrot);
                }
                OpenSettings();
            };
            menuItem.DropDownItems.Add(settingsItem);
        }

        private void hailstoneSequenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Only switch if not already on Hailstone (clicking on parent while dropdown is open)
            if (sender is ToolStripMenuItem menuItem && menuItem.DropDownItems.Count == 0)
            {
                SetVisualization(VisualizationFactory.VisualizationType.Hailstone);
            }
        }

        private void hailstoneSequenceToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            var menuItem = (ToolStripMenuItem)sender;
            menuItem.DropDownItems.Clear();

            // Add presets for Hailstone
            var presetNames = VisualizationPresets.GetPresetNames(VisualizationFactory.VisualizationType.Hailstone);
            foreach (var presetName in presetNames)
            {
                var presetItem = new ToolStripMenuItem(presetName);
                presetItem.Click += (s, args) =>
                {
                    SetVisualization(VisualizationFactory.VisualizationType.Hailstone);
                    LoadPreset(presetName);
                };
                menuItem.DropDownItems.Add(presetItem);
            }

            // Add separator and Settings
            menuItem.DropDownItems.Add(new ToolStripSeparator());
            var settingsItem = new ToolStripMenuItem("Settings...");
            settingsItem.Click += (s, args) =>
            {
                if (_currentVisualizationType != VisualizationFactory.VisualizationType.Hailstone)
                {
                    SetVisualization(VisualizationFactory.VisualizationType.Hailstone);
                }
                OpenSettings();
            };
            menuItem.DropDownItems.Add(settingsItem);
        }

        private void OpenSettings()
        {
            if (_currentVisualization == null) return;

            // Get current configuration
            var config = _currentVisualization.GetConfig();

            // Create settings dialog (no 'using' - form manages its own lifetime)
            var settingsForm = new Form
            {
                Text = $"Settings - {_currentVisualization.Name}",
                Width = 650,
                Height = 750,
                MinimumSize = new Size(500, 600),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.Sizable,
                MaximizeBox = true,
                MinimizeBox = false,
                TopMost = true
            };

            // Add PropertyGrid
            var propertyGrid = new PropertyGrid
            {
                Dock = DockStyle.Fill,
                SelectedObject = config,
                PropertySort = PropertySort.Categorized,
                HelpVisible = true
            };

            // Add Apply/Close buttons
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                Padding = new Padding(10)
            };

            var closeButton = new Button
            {
                Text = "Close",
                Width = 120,
                Height = 40,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Font = new Font("Segoe UI", 10F)
            };

            var applyButton = new Button
            {
                Text = "Apply",
                Width = 120,
                Height = 40,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Font = new Font("Segoe UI", 10F)
            };

            // Apply button: Update visualization but keep dialog open
            applyButton.Click += (s, args) =>
            {
                _currentVisualization = _currentVisualization.WithConfig(config);
                _imageRendered = false;
                Invalidate();
            };

            // Close button: Apply settings and close dialog
            closeButton.Click += (s, args) =>
            {
                _currentVisualization = _currentVisualization.WithConfig(config);
                _imageRendered = false;
                Invalidate();
                settingsForm.Close();
            };

            // Position buttons after panel is added to form
            buttonPanel.Controls.Add(applyButton);
            buttonPanel.Controls.Add(closeButton);
            settingsForm.Controls.Add(propertyGrid);
            settingsForm.Controls.Add(buttonPanel);

            // Position buttons on the right side after layout is complete
            settingsForm.Load += (s, args) =>
            {
                closeButton.Location = new Point(buttonPanel.ClientSize.Width - closeButton.Width - 15, 15);
                applyButton.Location = new Point(closeButton.Left - applyButton.Width - 15, 15);
            };

            // Keep buttons positioned when resizing
            buttonPanel.Resize += (s, args) =>
            {
                closeButton.Location = new Point(buttonPanel.ClientSize.Width - closeButton.Width - 15, 15);
                applyButton.Location = new Point(closeButton.Left - applyButton.Width - 15, 15);
            };

            // Dispose form when closed
            settingsForm.FormClosed += (s, args) => settingsForm.Dispose();

            // Show modeless dialog (doesn't block main window)
            settingsForm.Show(this);
        }

        #endregion MenuEventHandlers

        #region ExportMenuHandlers

        private void exportToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            // Enable/disable SVG based on current visualization
            bool isSVGSupported = _currentVisualization?.GetConfig() is HailstoneConfig;
            exportSVGToolStripMenuItem.Enabled = isSVGSupported;

            // Disable all export options if no visualization selected
            bool hasVisualization = _currentVisualization != null && _imageRendered;
            exportPNGToolStripMenuItem.Enabled = hasVisualization;
            exportJPEGToolStripMenuItem.Enabled = hasVisualization;
            exportBMPToolStripMenuItem.Enabled = hasVisualization;
            exportSVGToolStripMenuItem.Enabled = hasVisualization && isSVGSupported;
        }

        private void exportPNGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportBitmap(System.Drawing.Imaging.ImageFormat.Png, "PNG Image|*.png", "png");
        }

        private void exportJPEGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportBitmap(System.Drawing.Imaging.ImageFormat.Jpeg, "JPEG Image|*.jpg;*.jpeg", "jpg");
        }

        private void exportBMPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportBitmap(System.Drawing.Imaging.ImageFormat.Bmp, "Bitmap Image|*.bmp", "bmp");
        }

        private void exportSVGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportSVG();
        }

        private void ExportBitmap(System.Drawing.Imaging.ImageFormat format, string filter, string extension)
        {
            if (_bmp == null || _currentVisualization == null)
            {
                MessageBox.Show("No visualization to export. Please render a visualization first.",
                    "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var saveDialog = new SaveFileDialog
            {
                Filter = filter,
                DefaultExt = extension,
                FileName = $"{_currentVisualization.Name.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.{extension}",
                Title = $"Export as {extension.ToUpper()}"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Clone bitmap to add metadata
                    using var exportBitmap = new Bitmap(_bmp);

                    // Add metadata based on format
                    if (format.Equals(System.Drawing.Imaging.ImageFormat.Png))
                    {
                        AddPngMetadata(exportBitmap);
                    }
                    else if (format.Equals(System.Drawing.Imaging.ImageFormat.Jpeg))
                    {
                        AddJpegMetadata(exportBitmap);
                    }

                    exportBitmap.Save(saveDialog.FileName, format);
                    MessageBox.Show($"Image exported successfully to:\n{saveDialog.FileName}",
                        "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to export image:\n{ex.Message}",
                        "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void AddPngMetadata(Bitmap bitmap)
        {
            // PNG supports text chunks for metadata
            var config = _currentVisualization!.GetConfig();

            // Software
            AddTextProperty(bitmap, 0x0131, "Software", "Numerical Visualizations .NET 10");

            // Title
            AddTextProperty(bitmap, 0x010E, "ImageDescription", 
                $"{_currentVisualization.Name} - {DateTime.Now:yyyy-MM-dd}");

            // Comment with all parameters for reproducibility
            var metadata = BuildMetadataString(config);
            AddTextProperty(bitmap, 0x9286, "UserComment", metadata);
        }

        private void AddJpegMetadata(Bitmap bitmap)
        {
            // JPEG supports limited EXIF metadata
            var config = _currentVisualization!.GetConfig();

            // Software
            AddTextProperty(bitmap, 0x0131, "Software", "Numerical Visualizations");

            // Image description (brief)
            AddTextProperty(bitmap, 0x010E, "ImageDescription", 
                $"{_currentVisualization.Name}: Iterations={config.MaxIterations}");
        }

        private void AddTextProperty(Bitmap bitmap, int propertyId, string name, string value)
        {
            try
            {
                // Convert string to bytes (ASCII)
                byte[] bytes = System.Text.Encoding.ASCII.GetBytes(value + "\0");

                var propertyItem = (System.Drawing.Imaging.PropertyItem)
                    System.Runtime.Serialization.FormatterServices.GetUninitializedObject(
                        typeof(System.Drawing.Imaging.PropertyItem));

                propertyItem.Id = propertyId;
                propertyItem.Type = 2; // ASCII string
                propertyItem.Len = bytes.Length;
                propertyItem.Value = bytes;

                bitmap.SetPropertyItem(propertyItem);
            }
            catch
            {
                // Silently fail if metadata can't be added
            }
        }

        private string BuildMetadataString(VisualizationConfig config)
        {
            var sb = new System.Text.StringBuilder();

            sb.AppendLine($"Visualization: {_currentVisualization!.Name}");
            sb.AppendLine($"Export Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Resolution: {_bmp!.Width}x{_bmp.Height}");
            sb.AppendLine($"Coordinate Range: X={XWidth}, Y={YHeight}");
            sb.AppendLine();
            sb.AppendLine("Algorithm Parameters:");
            sb.AppendLine($"  MaxIterations: {config.MaxIterations}");
            sb.AppendLine($"  Tolerance: {config.Tolerance:E2}");

            // Add visualization-specific parameters
            if (config is NewtonConfig newton)
            {
                sb.AppendLine($"  HueSpread: {newton.HueSpread}");
                sb.AppendLine();
                sb.AppendLine("Color Mapping:");
                sb.AppendLine($"  Palette: Spectrum360 (360-degree HSV color wheel)");
                sb.AppendLine($"  Method: Hue = (Atan2(imag, real) * 180/π + 180 + iterations * HueSpread) mod 360");
                sb.AppendLine($"  HueSpread: {newton.HueSpread} degrees per iteration");
                sb.AppendLine($"  Description: Colors determined by complex angle (argument) and iteration count");
            }
            else if (config is MandelbrotConfig mandelbrot)
            {
                sb.AppendLine($"  EscapeRadius: {mandelbrot.EscapeRadius}");
                sb.AppendLine($"  ColorOffset: {mandelbrot.ColorOffset}");
                sb.AppendLine($"  ColorMultiplier: {mandelbrot.ColorMultiplier}");
                sb.AppendLine($"  ColorModulo: {mandelbrot.ColorModulo}");
                sb.AppendLine();
                sb.AppendLine("Color Mapping:");
                sb.AppendLine($"  Palette: Spectrum360 (360-degree HSV color wheel)");
                sb.AppendLine($"  Method: Hue = ((iterations mod {mandelbrot.ColorModulo} + {mandelbrot.ColorOffset}) * {mandelbrot.ColorMultiplier}) mod {mandelbrot.ColorModulo}");
                sb.AppendLine($"  Description: Banded color patterns based on escape iteration count");
            }
            else if (config is HailstoneConfig hailstone)
            {
                sb.AppendLine($"  StartIntX: {hailstone.StartIntX} (integer coordinate)");
                sb.AppendLine($"  StartIntY: {hailstone.StartIntY} (integer coordinate)");
                sb.AppendLine($"  ScaleFactorX: {hailstone.ScaleFactorX} (0 = auto, X-axis spacing)");
                sb.AppendLine($"  ScaleFactorY: {hailstone.ScaleFactorY} (0 = auto, Y-axis spacing)");
                sb.AppendLine($"  LineWidth: {hailstone.LineWidth}");
                sb.AppendLine($"  DotSize: {hailstone.DotSize}");
                sb.AppendLine($"  ColorSpread: {hailstone.ColorSpread}");
                sb.AppendLine();
                sb.AppendLine("Color Mapping:");
                sb.AppendLine($"  Palette: Spectrum360 (360-degree HSV color wheel)");
                sb.AppendLine($"  Method: Hue = (step * ColorSpread) mod 360");
                sb.AppendLine($"  ColorSpread: {hailstone.ColorSpread} degrees per step");
                sb.AppendLine($"  Description: Sequential rainbow gradient along path");
            }

            sb.AppendLine();
            sb.AppendLine("Display Settings:");
            sb.AppendLine($"  ShowAxes: {config.ShowAxes}");

            if (config is HailstoneConfig hs)
            {
                sb.AppendLine($"  ShowPointLabels: {hs.ShowPointLabels}");
                sb.AppendLine($"  ShowDots: {hs.ShowDots}");
            }

            return sb.ToString();
        }

        private void ExportSVG()
        {
            if (_currentVisualization?.GetConfig() is not HailstoneConfig hailstoneConfig)
            {
                MessageBox.Show("SVG export is only available for Hailstone Sequence visualization.",
                    "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var saveDialog = new SaveFileDialog
            {
                Filter = "SVG Vector Image|*.svg",
                DefaultExt = "svg",
                FileName = $"Hailstone_Sequence_{DateTime.Now:yyyyMMdd_HHmmss}.svg",
                Title = "Export as SVG"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Render at high resolution for SVG
                    int width = 1920;
                    int height = 1080;
                    string svgContent = GenerateHailstoneSVG(hailstoneConfig, width, height);

                    File.WriteAllText(saveDialog.FileName, svgContent);
                    MessageBox.Show($"SVG exported successfully to:\n{saveDialog.FileName}",
                        "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to export SVG:\n{ex.Message}",
                        "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private string GenerateHailstoneSVG(HailstoneConfig config, int width, int height)
        {
            // First pass: Calculate all integer points (unscaled)
            var intPoints = new List<(int step, int intX, int intY, Color color)>();

            int intX = config.StartIntX;
            int intY = config.StartIntY;
            intPoints.Add((0, intX, intY, Color.Red));

            for (int index = 1; index <= config.MaxIterations; index++)
            {
                var cs = ColorPalettes.Spectrum360[(index * config.ColorSpread) % 360];
                Color lineColor = Color.FromArgb(cs.red, cs.green, cs.blue);

                int nextIntX = Functions.FHailStoneNextX(intX, intY);
                int nextIntY = Functions.FHailStoneNextY(intX, intY);

                intPoints.Add((index, nextIntX, nextIntY, lineColor));

                intX = nextIntX;
                intY = nextIntY;

                if (intX == 1 && intY == 1) break;
            }

            // Auto-calculate scale factors if set to 0
            double scaleX = config.ScaleFactorX;
            double scaleY = config.ScaleFactorY;

            if (scaleX == 0.0 || scaleY == 0.0)
            {
                // Focus on EARLY iterations to keep view near starting point
                // Use first 30% of sequence (or 50 iterations max, whichever is less)
                int iterationsForScaling = Math.Min(50, Math.Max(10, intPoints.Count * 30 / 100));
                var earlyPoints = intPoints.Take(iterationsForScaling).ToList();

                int minIntX = earlyPoints.Min(p => p.intX);
                int maxIntX = earlyPoints.Max(p => p.intX);
                int minIntY = earlyPoints.Min(p => p.intY);
                int maxIntY = earlyPoints.Max(p => p.intY);

                int rangeIntX = maxIntX - minIntX;
                int rangeIntY = maxIntY - minIntY;

                if (scaleX == 0.0)
                    scaleX = rangeIntX > 0 ? 3.0 / rangeIntX : 0.05;
                if (scaleY == 0.0)
                    scaleY = rangeIntY > 0 ? 3.0 / rangeIntY : 0.05;
            }

            // Second pass: Convert to scaled coordinates
            var points = new List<(int step, double x, double y, Color color)>();
            foreach (var (step, ix, iy, color) in intPoints)
            {
                points.Add((step, ix * scaleX, iy * scaleY, color));
            }

            // Calculate bounds
            double minX = points.Min(p => p.x);
            double maxX = points.Max(p => p.x);
            double minY = points.Min(p => p.y);
            double maxY = points.Max(p => p.y);

            double rangeX = maxX - minX;
            double rangeY = maxY - minY;
            double paddingX = rangeX * 0.15;
            double paddingY = rangeY * 0.15;

            minX -= paddingX;
            maxX += paddingX;
            minY -= paddingY;
            maxY += paddingY;

            double centerX = (minX + maxX) / 2.0;
            double centerY = (minY + maxY) / 2.0;
            double dataRangeX = maxX - minX;
            double dataRangeY = maxY - minY;

            dataRangeX = Math.Max(dataRangeX, 0.1);
            dataRangeY = Math.Max(dataRangeY, 0.1);

            int screenCenterX = width / 2;
            int screenCenterY = height / 2;
            float pixelsPerUnitX = width / (float)dataRangeX;
            float pixelsPerUnitY = height / (float)dataRangeY;

            // Generate SVG with comprehensive metadata
            var svg = new System.Text.StringBuilder();
            svg.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>");
            svg.AppendLine($"<svg width=\"{width}\" height=\"{height}\" xmlns=\"http://www.w3.org/2000/svg\" version=\"1.1\">");

            // Add metadata
            svg.AppendLine("  <!-- ========================================= -->");
            svg.AppendLine("  <!-- Numerical Visualizations Metadata       -->");
            svg.AppendLine("  <!-- ========================================= -->");
            svg.AppendLine($"  <title>Hailstone Sequence Visualization</title>");
            svg.AppendLine($"  <desc>2D visualization of Collatz-inspired dynamics generated by Numerical Visualizations</desc>");
            svg.AppendLine();
            svg.AppendLine("  <metadata>");
            svg.AppendLine("    <rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\"");
            svg.AppendLine("             xmlns:dc=\"http://purl.org/dc/elements/1.1/\">");
            svg.AppendLine("      <rdf:Description>");
            svg.AppendLine($"        <dc:title>Hailstone Sequence Visualization</dc:title>");
            svg.AppendLine($"        <dc:creator>Numerical Visualizations .NET 10</dc:creator>");
            svg.AppendLine($"        <dc:date>{DateTime.Now:yyyy-MM-ddTHH:mm:ss}</dc:date>");
            svg.AppendLine("        <dc:format>image/svg+xml</dc:format>");
            svg.AppendLine($"        <dc:description>Exported from Numerical Visualizations application</dc:description>");
            svg.AppendLine("      </rdf:Description>");
            svg.AppendLine("    </rdf:RDF>");
            svg.AppendLine("  </metadata>");
            svg.AppendLine();
            svg.AppendLine("  <!-- Application-Specific Metadata for Reproducibility -->");
            svg.AppendLine("  <g id=\"visualization-parameters\" style=\"display:none;\">");
            svg.AppendLine("    <desc>");
            svg.AppendLine($"      Visualization Type: Hailstone Sequence");
            svg.AppendLine($"      Software: Numerical Visualizations v1.0 (.NET 10)");
            svg.AppendLine($"      Export Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            svg.AppendLine($"      Resolution: {width}x{height} pixels");
            svg.AppendLine();
            svg.AppendLine("      Algorithm Parameters:");
            svg.AppendLine($"        MaxIterations: {config.MaxIterations}");
            svg.AppendLine($"        StartIntX: {config.StartIntX} (integer coordinate)");
            svg.AppendLine($"        StartIntY: {config.StartIntY} (integer coordinate)");
            svg.AppendLine($"        ScaleFactorX: {config.ScaleFactorX} (0 = auto-calculated)");
            svg.AppendLine($"        ScaleFactorY: {config.ScaleFactorY} (0 = auto-calculated)");
            svg.AppendLine();
            svg.AppendLine("      Color Mapping:");
            svg.AppendLine($"        Palette: Spectrum360 (360-degree HSV color wheel)");
            svg.AppendLine($"        Method: Hue = (step * ColorSpread) mod 360");
            svg.AppendLine($"        ColorSpread: {config.ColorSpread} degrees per step");
            svg.AppendLine($"        Description: Sequential rainbow gradient progressing along the path");
            svg.AppendLine($"        Result: Each step advances {config.ColorSpread}° around the color wheel");
            svg.AppendLine();
            svg.AppendLine("      Rendering Parameters:");
            svg.AppendLine($"        LineWidth: {config.LineWidth}");
            svg.AppendLine($"        DotSize: {config.DotSize}");
            svg.AppendLine();
            svg.AppendLine("      Display Settings:");
            svg.AppendLine($"        ShowAxes: {config.ShowAxes}");
            svg.AppendLine($"        ShowPointLabels: {config.ShowPointLabels}");
            svg.AppendLine($"        ShowDots: {config.ShowDots}");
            svg.AppendLine();
            svg.AppendLine("      Coordinate System:");
            svg.AppendLine($"        Data Range X: [{minX:F4}, {maxX:F4}]");
            svg.AppendLine($"        Data Range Y: [{minY:F4}, {maxY:F4}]");
            svg.AppendLine($"        Center: ({centerX:F4}, {centerY:F4})");
            svg.AppendLine($"        Total Points: {points.Count}");
            svg.AppendLine("    </desc>");
            svg.AppendLine("  </g>");
            svg.AppendLine();
            svg.AppendLine("  <!-- Visualization Graphics -->");
            svg.AppendLine($"  <rect width=\"100%\" height=\"100%\" fill=\"black\"/>");

            // Draw lines
            for (int i = 0; i < points.Count - 1; i++)
            {
                var (step1, x1, y1, color1) = points[i];
                var (step2, x2, y2, color2) = points[i + 1];

                float sx1 = screenCenterX + (float)((x1 - centerX) * pixelsPerUnitX);
                float sy1 = screenCenterY - (float)((y1 - centerY) * pixelsPerUnitY);
                float sx2 = screenCenterX + (float)((x2 - centerX) * pixelsPerUnitX);
                float sy2 = screenCenterY - (float)((y2 - centerY) * pixelsPerUnitY);

                svg.AppendLine($"  <line x1=\"{sx1:F2}\" y1=\"{sy1:F2}\" x2=\"{sx2:F2}\" y2=\"{sy2:F2}\" " +
                    $"stroke=\"rgb({color2.R},{color2.G},{color2.B})\" stroke-width=\"{config.LineWidth * 1000}\" />");
            }

            // Draw dots if enabled
            if (config.ShowDots)
            {
                svg.AppendLine();
                svg.AppendLine("  <!-- Point Markers -->");
                foreach (var (step, x, y, color) in points)
                {
                    float sx = screenCenterX + (float)((x - centerX) * pixelsPerUnitX);
                    float sy = screenCenterY - (float)((y - centerY) * pixelsPerUnitY);
                    float radius = config.DotSize * 500;

                    svg.AppendLine($"  <circle cx=\"{sx:F2}\" cy=\"{sy:F2}\" r=\"{radius:F2}\" " +
                        $"fill=\"rgb({color.R},{color.G},{color.B})\" />");
                }
            }

            // Draw labels if enabled
            if (config.ShowPointLabels)
            {
                svg.AppendLine();
                svg.AppendLine("  <!-- Point Labels -->");
                foreach (var (step, x, y, color) in points)
                {
                    float sx = screenCenterX + (float)((x - centerX) * pixelsPerUnitX);
                    float sy = screenCenterY - (float)((y - centerY) * pixelsPerUnitY);

                    // Calculate integer coordinates from scaled values
                    int labelIntX = (int)Math.Round(x / config.ScaleFactor);
                    int labelIntY = (int)Math.Round(y / config.ScaleFactor);
                    string label = $"({step}, {labelIntX}, {labelIntY})";

                    svg.AppendLine($"  <text x=\"{sx + 8:F2}\" y=\"{sy + 4:F2}\" " +
                        $"fill=\"rgb(220,220,220)\" font-family=\"Arial\" font-size=\"8\">{label}</text>");
                }
            }

            svg.AppendLine("</svg>");
            return svg.ToString();
        }

        #endregion ExportMenuHandlers

        #region ToolbarEventHandlers

        private void axesToolStripButton_Click(object sender, EventArgs e)
        {
            if (_currentVisualization == null) return;

            var config = _currentVisualization.GetConfig();

            // Hailstone renders axes internally - needs full re-render
            if (config is HailstoneConfig)
            {
                config.ShowAxes = axesToolStripButton.Checked;
                _currentVisualization = _currentVisualization.WithConfig(config);

                // Full re-render for Hailstone
                _imageRendered = false;
                _renderingInProgress = false;
                _cachedBaseVisualization?.Dispose();
                _cachedBaseVisualization = null;
                panel1.BackgroundImage?.Dispose();
                panel1.BackgroundImage = null;
                Invalidate();
            }
            else if (_cachedBaseVisualization != null)
            {
                // Fast path for Newton/Mandelbrot: Use cached overlay
                config.ShowAxes = axesToolStripButton.Checked;
                _currentVisualization = _currentVisualization.WithConfig(config);
                ApplyAxesOverlay();
            }
        }

        private void ApplyAxesOverlay()
        {
            if (_cachedBaseVisualization == null || _currentVisualization == null) return;

            // Clone the cached base visualization
            var displayBitmap = new Bitmap(_cachedBaseVisualization);

            // Apply axes overlay if enabled (fast!)
            var config = _currentVisualization.GetConfig();
            if (config.ShowAxes)
            {
                RenderingHelpers.DrawAxesOnBitmap(displayBitmap, XWidth, YHeight);
            }

            // Update display without re-rendering
            _bmp?.Dispose();
            _bmp = displayBitmap;
            panel1.BackgroundImage?.Dispose();
            panel1.BackgroundImage = _bmp;
            panel1.Invalidate();
        }

        private void pointLabelsToolStripButton_Click(object sender, EventArgs e)
        {
            if (_currentVisualization == null) return;

            var config = _currentVisualization.GetConfig();
            if (config is HailstoneConfig hailstoneConfig)
            {
                hailstoneConfig.ShowPointLabels = pointLabelsToolStripButton.Checked;
                _currentVisualization = _currentVisualization.WithConfig(hailstoneConfig);

                // Hailstone needs full re-render (labels are part of the path rendering)
                _imageRendered = false;
                _renderingInProgress = false;
                _cachedBaseVisualization?.Dispose();
                _cachedBaseVisualization = null;
                panel1.BackgroundImage?.Dispose();
                panel1.BackgroundImage = null;
                Invalidate();
            }
        }

        private void dotsToolStripButton_Click(object sender, EventArgs e)
        {
            if (_currentVisualization == null) return;

            var config = _currentVisualization.GetConfig();
            if (config is HailstoneConfig hailstoneConfig)
            {
                hailstoneConfig.ShowDots = dotsToolStripButton.Checked;
                _currentVisualization = _currentVisualization.WithConfig(hailstoneConfig);

                // Hailstone needs full re-render (dots are part of the path rendering)
                _imageRendered = false;
                _renderingInProgress = false;
                _cachedBaseVisualization?.Dispose();
                _cachedBaseVisualization = null;
                panel1.BackgroundImage?.Dispose();
                panel1.BackgroundImage = null;
                Invalidate();
            }
        }

        #endregion ToolbarEventHandlers
    }
}
