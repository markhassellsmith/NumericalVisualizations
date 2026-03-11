using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static WinFormsFractal.ScreenStructures;

namespace WinFormsFractal
{
    public partial class Canvas : Form
    {
        #region FractalTypeEnum

        private enum FractalType
        {
            Newton,
            Mandelbrot,
            Hailstone
        }

        #endregion FractalTypeEnum

        #region CanvasVariables

        private const int MaxHorizontal = 2000;
        private const int MaxVertical = 2000;
        private const int XWidth = 4;
        private const int YHeight = 4;

        private const int MaxIterations = 1200;
        private const double Tolerance = 1e-10;

        private const double MandelbrotEscapeRadius = 1000000.0;
        private const int MandelbrotMaxIterations = 512;
        private const int MandelbrotColorOffset = 270;
        private const int MandelbrotColorMultiplier = 19;
        private const int MandelbrotColorModulo = 359;

        private const int NewtonHueSpread = 17;

        private const int HailstoneColorSpread = 517;

        private FractalType currentFractal = FractalType.Newton;
        private int maxiterations = MaxIterations;
        private double tolerance = Tolerance;
        private Bitmap bmp = new Bitmap(MaxHorizontal, MaxVertical);

        // render guard to avoid repeated expensive renders and message boxes
        private bool imageRendered = false;

        private bool renderingInProgress = false;
        private Task? renderTask;

        private Point pdown;
        private Point pup;
        private Graphics? g;

        #endregion CanvasVariables

        #region FractalMethods

        private void Mandelbrot(int w, int h, Bitmap targetBmp)
        {
            double localDeltaX = (double)XWidth / (double)w;
            double localDeltaY = (double)YHeight / (double)h;
            for (int px = 0; px < w; px++)
            {
                for (int py = 0; py < h; py++)
                {
                    double x = (double)(-XWidth) / 2.0 + localDeltaX * px;
                    double y = (double)(-YHeight) / 2.0 + localDeltaY * py;
                    Complex c = new(x, y);
                    Complex zstart = Complex.Zero;
                    Complex znext = Functions.FMandelbrot(zstart, c);
                    int iterations = 0;
                    while (znext.Magnitude < MandelbrotEscapeRadius && iterations < MandelbrotMaxIterations)
                    {
                        znext = Functions.FMandelbrot(znext, c);
                        iterations++;
                    }

                    Color color;
                    if (znext.Magnitude >= MandelbrotEscapeRadius)
                    {
                        int colorIndex = (((iterations % MandelbrotColorModulo) + MandelbrotColorOffset) * MandelbrotColorMultiplier) % MandelbrotColorModulo;
                        var cs = ColorPalettes.Spectrum360[colorIndex];
                        color = Color.FromArgb(cs.red, cs.green, cs.blue);
                    }
                    else
                    {
                        color = Color.FromArgb(0, 0, 0);
                    }

                    targetBmp.SetPixel(px, py, color);
                }
            }
        }

        private void NewtonsMethod(int w, int h, Bitmap targetBmp)
        {
            double localDeltaX = (double)XWidth / (double)w;
            double localDeltaY = (double)YHeight / (double)h;
            for (int px = 0; px < w; px++)
            {
                for (int py = 0; py < h; py++)
                {
                    double x = (double)(-XWidth) / 2.0 + localDeltaX * px;
                    double y = (double)(-YHeight) / 2.0 + localDeltaY * py;

                    Complex zstart = new Complex(x, y);
                    Complex znext = zstart - Functions.F(zstart) / Functions.FP(zstart);
                    int iterations = 1;
                    double mag = (znext - zstart).Magnitude;
                    while (mag > tolerance && iterations < maxiterations)
                    {
                        zstart = znext;
                        znext = zstart - Functions.F(zstart) / Functions.FP(zstart);
                        mag = (znext - zstart).Magnitude;
                        iterations++;
                    }

                    Color color;
                    if (iterations >= maxiterations)
                    {
                        color = Color.FromArgb(0, 0, 0);
                    }
                    else
                    {
                        double arg = Math.Atan2(znext.Imaginary, znext.Real);
                        int hueFromArg = (int)Math.Round(((arg / (2.0 * Math.PI)) + 0.5) * 360.0) % 360;
                        int hueIndex = (hueFromArg + iterations * NewtonHueSpread) % 360;
                        var cs = ColorPalettes.Spectrum360[(hueIndex + 360) % 360];
                        color = Color.FromArgb(cs.red, cs.green, cs.blue);
                    }

                    targetBmp.SetPixel(px, py, color);
                }
            }
        }

        private void Hailstone(int startingX, int startingY, Graphics graphics)
        {
            using Pen drawingPen = new Pen(Color.Red) { Width = 4.0F };

            for (int index = 1; index <= maxiterations; index++)
            {
                var cs = ColorPalettes.Spectrum360[(index * HailstoneColorSpread) % 360];
                drawingPen.Color = Color.FromArgb(cs.red, cs.green, cs.blue);
                int nextX = Functions.FHailStoneNextX(startingX, startingY);
                int nextY = Functions.FHailStoneNextY(startingX, startingY);

                string startinglabel = $"({index}, {startingX}, {startingY})";
                string endinglabel = $"({index + 1}, {nextX}, {nextY})";
                TextRenderer.DrawText(graphics, startinglabel, this.Font,
                     new Point(startingX, startingY), Color.White, Color.SteelBlue, TextFormatFlags.Default);
                TextRenderer.DrawText(graphics, endinglabel, this.Font,
                     new Point(nextX, nextY), Color.White, Color.SteelBlue, TextFormatFlags.Default);

                graphics.DrawLine(drawingPen, new PointF(startingX, startingY), new PointF(nextX, nextY));
                startingX = nextX;
                startingY = nextY;
                if (nextX == 1 && nextY == 1) break;
            }
        }

        #endregion FractalMethods

        #region CanvasMethods

        public Canvas()
        {
            InitializeComponent();
            panel1.Dock = DockStyle.Fill;
            panel1.BackgroundImageLayout = ImageLayout.Zoom;
        }

        private void Canvas_Paint(object sender, PaintEventArgs e)
        {
            if (imageRendered || renderingInProgress) return;

            renderingInProgress = true;
            renderTask = Task.Run(() =>
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

                    var renderBmp = new Bitmap(targetW, targetH);

                    switch (currentFractal)
                    {
                        case FractalType.Newton:
                            NewtonsMethod(targetW, targetH, renderBmp);
                            break;
                        case FractalType.Mandelbrot:
                            Mandelbrot(targetW, targetH, renderBmp);
                            break;
                        case FractalType.Hailstone:
                            using (var g = Graphics.FromImage(renderBmp))
                            {
                                g.Clear(Color.Black);
                                Hailstone(targetW / 2, targetH / 2, g);
                            }
                            break;
                    }

                    Directory.CreateDirectory(@"C:\Temp");
                    renderBmp.Save(@"C:\Temp\fractal.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

                    // marshal UI update to UI thread
                    BeginInvoke(() =>
                    {
                        try
                        {
                            // dispose old bmp
                            bmp?.Dispose();
                            bmp = renderBmp;
                            panel1.BackgroundImage = bmp;
                            imageRendered = true;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("UI update failed: " + ex.Message);
                        }
                        finally
                        {
                            renderingInProgress = false;
                        }
                    });
                }
                catch (Exception ex)
                {
                    BeginInvoke(() =>
                    {
                        MessageBox.Show("Rendering failed: " + ex.Message);
                        renderingInProgress = false;
                    });
                }
            });
        }

        #endregion CanvasMethods

        #region MouseMethods

        private void Canvas_MouseDown(object sender, MouseEventArgs e)
        {
            pdown = e.Location;
        }

        private void Canvas_MouseUp(object sender, MouseEventArgs e)
        {
            pup = e.Location;
        }

        #endregion MouseMethods
    }

    public static class PaletteHelpers
    {
        // Save Spectrum360 as a PNG grid (cols x rows). Each swatch is swatchSize x swatchSize.
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

        // Save Spectrum360 as a simple HTML file with inline blocks. Browser friendly and copy/paste sharable.
        public static void SaveSpectrum360AsHtml(string path, int cols = 36, int swatchPx = 30)
        {
            var palette = ColorPalettes.Spectrum360;
            int total = palette.Length;

            var sb = new StringBuilder();
            sb.AppendLine("<!doctype html>");
            sb.AppendLine("<html><head><meta charset=\"utf-8\"><title>Spectrum360</title>");
            sb.AppendLine("<style>body{font-family:Segoe UI,Arial;padding:10px;background:#111;color:#ddd} .sw{display:inline-block;margin:1px;width:" + swatchPx + "px;height:" + swatchPx + "px;border:1px solid #333;box-sizing:border-box} .item{display:inline-block;text-align:center;width:" + swatchPx + "px;font-size:10px;color:#ddd}</style>");
            sb.AppendLine("</head><body>");
            sb.AppendLine("<h2>Spectrum360 — natural order</h2>");
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

            // attempt to open in default browser
            try
            {
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            }
            catch { /* fail silently — file still written */ }
        }
    }
}