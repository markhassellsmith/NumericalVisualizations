namespace NumericalVisualizations
{
    partial class Canvas
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            panel1 = new Panel();
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            exportToolStripMenuItem = new ToolStripMenuItem();
            exportPNGToolStripMenuItem = new ToolStripMenuItem();
            exportJPEGToolStripMenuItem = new ToolStripMenuItem();
            exportBMPToolStripMenuItem = new ToolStripMenuItem();
            exportSVGToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            exitToolStripMenuItem = new ToolStripMenuItem();
            visualizationsToolStripMenuItem = new ToolStripMenuItem();
            newtonsMethodToolStripMenuItem = new ToolStripMenuItem();
            mandelbrotSetToolStripMenuItem = new ToolStripMenuItem();
            hailstoneSequenceToolStripMenuItem = new ToolStripMenuItem();
            toolStrip1 = new ToolStrip();
            axesToolStripButton = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            pointLabelsToolStripButton = new ToolStripButton();
            dotsToolStripButton = new ToolStripButton();
            menuStrip1.SuspendLayout();
            toolStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = Color.Black;
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 53);
            panel1.Name = "panel1";
            panel1.Size = new Size(1924, 905);
            panel1.TabIndex = 0;
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, visualizationsToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1924, 28);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { exportToolStripMenuItem, toolStripSeparator3, exitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(46, 24);
            fileToolStripMenuItem.Text = "&File";
            // 
            // exportToolStripMenuItem
            // 
            exportToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { exportPNGToolStripMenuItem, exportJPEGToolStripMenuItem, exportBMPToolStripMenuItem, exportSVGToolStripMenuItem });
            exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            exportToolStripMenuItem.Size = new Size(180, 26);
            exportToolStripMenuItem.Text = "&Export";
            exportToolStripMenuItem.DropDownOpening += exportToolStripMenuItem_DropDownOpening;
            // 
            // exportPNGToolStripMenuItem
            // 
            exportPNGToolStripMenuItem.Name = "exportPNGToolStripMenuItem";
            exportPNGToolStripMenuItem.Size = new Size(200, 26);
            exportPNGToolStripMenuItem.Text = "Export as &PNG...";
            exportPNGToolStripMenuItem.Click += exportPNGToolStripMenuItem_Click;
            // 
            // exportJPEGToolStripMenuItem
            // 
            exportJPEGToolStripMenuItem.Name = "exportJPEGToolStripMenuItem";
            exportJPEGToolStripMenuItem.Size = new Size(200, 26);
            exportJPEGToolStripMenuItem.Text = "Export as &JPEG...";
            exportJPEGToolStripMenuItem.Click += exportJPEGToolStripMenuItem_Click;
            // 
            // exportBMPToolStripMenuItem
            // 
            exportBMPToolStripMenuItem.Name = "exportBMPToolStripMenuItem";
            exportBMPToolStripMenuItem.Size = new Size(200, 26);
            exportBMPToolStripMenuItem.Text = "Export as &BMP...";
            exportBMPToolStripMenuItem.Click += exportBMPToolStripMenuItem_Click;
            // 
            // exportSVGToolStripMenuItem
            // 
            exportSVGToolStripMenuItem.Name = "exportSVGToolStripMenuItem";
            exportSVGToolStripMenuItem.Size = new Size(200, 26);
            exportSVGToolStripMenuItem.Text = "Export as &SVG...";
            exportSVGToolStripMenuItem.Click += exportSVGToolStripMenuItem_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(177, 6);
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.ShortcutKeys = Keys.Alt | Keys.F4;
            exitToolStripMenuItem.Size = new Size(169, 26);
            exitToolStripMenuItem.Text = "E&xit";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // visualizationsToolStripMenuItem
            // 
            visualizationsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { newtonsMethodToolStripMenuItem, mandelbrotSetToolStripMenuItem, hailstoneSequenceToolStripMenuItem });
            visualizationsToolStripMenuItem.Name = "visualizationsToolStripMenuItem";
            visualizationsToolStripMenuItem.Size = new Size(118, 24);
            visualizationsToolStripMenuItem.Text = "&Visualizations";
            // 
            // newtonsMethodToolStripMenuItem
            // 
            newtonsMethodToolStripMenuItem.Name = "newtonsMethodToolStripMenuItem";
            newtonsMethodToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.D1;
            newtonsMethodToolStripMenuItem.Size = new Size(272, 26);
            newtonsMethodToolStripMenuItem.Text = "&Newton's Method";
            newtonsMethodToolStripMenuItem.Click += newtonsMethodToolStripMenuItem_Click;
            newtonsMethodToolStripMenuItem.DropDownOpening += newtonsMethodToolStripMenuItem_DropDownOpening;
            // 
            // mandelbrotSetToolStripMenuItem
            // 
            mandelbrotSetToolStripMenuItem.Name = "mandelbrotSetToolStripMenuItem";
            mandelbrotSetToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.D2;
            mandelbrotSetToolStripMenuItem.Size = new Size(272, 26);
            mandelbrotSetToolStripMenuItem.Text = "&Mandelbrot Set";
            mandelbrotSetToolStripMenuItem.Click += mandelbrotSetToolStripMenuItem_Click;
            mandelbrotSetToolStripMenuItem.DropDownOpening += mandelbrotSetToolStripMenuItem_DropDownOpening;
            // 
            // hailstoneSequenceToolStripMenuItem
            // 
            hailstoneSequenceToolStripMenuItem.Name = "hailstoneSequenceToolStripMenuItem";
            hailstoneSequenceToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.D3;
            hailstoneSequenceToolStripMenuItem.Size = new Size(272, 26);
            hailstoneSequenceToolStripMenuItem.Text = "&Hailstone Sequence";
            hailstoneSequenceToolStripMenuItem.Click += hailstoneSequenceToolStripMenuItem_Click;
            hailstoneSequenceToolStripMenuItem.DropDownOpening += hailstoneSequenceToolStripMenuItem_DropDownOpening;
            // 
            // toolStrip1
            // 
            toolStrip1.Items.AddRange(new ToolStripItem[] { axesToolStripButton, toolStripSeparator2, pointLabelsToolStripButton, dotsToolStripButton });
            toolStrip1.Location = new Point(0, 28);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(1924, 25);
            toolStrip1.TabIndex = 2;
            toolStrip1.Text = "toolStrip1";
            // 
            // axesToolStripButton
            // 
            axesToolStripButton.CheckOnClick = true;
            axesToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            axesToolStripButton.Name = "axesToolStripButton";
            axesToolStripButton.Size = new Size(39, 22);
            axesToolStripButton.Text = "Axes";
            axesToolStripButton.ToolTipText = "Show/hide axes with tick marks";
            axesToolStripButton.Click += axesToolStripButton_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 25);
            // 
            // pointLabelsToolStripButton
            // 
            pointLabelsToolStripButton.CheckOnClick = true;
            pointLabelsToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            pointLabelsToolStripButton.Enabled = false;
            pointLabelsToolStripButton.Name = "pointLabelsToolStripButton";
            pointLabelsToolStripButton.Size = new Size(80, 22);
            pointLabelsToolStripButton.Text = "Point Labels";
            pointLabelsToolStripButton.ToolTipText = "Show/hide (N, X, Y) labels at each point";
            pointLabelsToolStripButton.Click += pointLabelsToolStripButton_Click;
            // 
            // dotsToolStripButton
            // 
            dotsToolStripButton.CheckOnClick = true;
            dotsToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            dotsToolStripButton.Enabled = false;
            dotsToolStripButton.Name = "dotsToolStripButton";
            dotsToolStripButton.Size = new Size(36, 22);
            dotsToolStripButton.Text = "Dots";
            dotsToolStripButton.ToolTipText = "Show/hide dots at segment endpoints";
            dotsToolStripButton.Click += dotsToolStripButton_Click;
            // 
            // Canvas
            //
            AutoScaleDimensions = new SizeF(9F, 23F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(1924, 958);
            Controls.Add(panel1);
            Controls.Add(toolStrip1);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "Canvas";
            Text = "Numerical Visualizations";
            Paint += Canvas_Paint;
            MouseDown += Canvas_MouseDown;
            MouseUp += Canvas_MouseUp;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        protected internal Panel panel1;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem exportToolStripMenuItem;
        private ToolStripMenuItem exportPNGToolStripMenuItem;
        private ToolStripMenuItem exportJPEGToolStripMenuItem;
        private ToolStripMenuItem exportBMPToolStripMenuItem;
        private ToolStripMenuItem exportSVGToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem visualizationsToolStripMenuItem;
        private ToolStripMenuItem newtonsMethodToolStripMenuItem;
        private ToolStripMenuItem mandelbrotSetToolStripMenuItem;
        private ToolStripMenuItem hailstoneSequenceToolStripMenuItem;
        private ToolStrip toolStrip1;
        private ToolStripButton axesToolStripButton;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripButton pointLabelsToolStripButton;
        private ToolStripButton dotsToolStripButton;
    }
}