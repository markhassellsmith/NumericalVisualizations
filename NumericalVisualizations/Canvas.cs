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

                    Directory.CreateDirectory(@"C:\Temp");
                    renderBmp.Save(@"C:\Temp\visualization.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

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
