using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static WinFormsFractal.ScreenStructures;

namespace WinFormsFractal
{
    public partial class Canvas : Form
    {
        #region CanvasVariables

        //------------------ bitmap dimensions, functional range dimensions //
        private static int maxhoriz = 2000;

        private static int maxvert = 2000;
        private static int xwidth = 4;
        private static int yheight = 4;
        private double deltax = (double)xwidth / (double)maxhoriz;   // divide the xwidth up into maxhoriz parts
        private double deltay = (double)yheight / (double)maxvert;    // divide the yheight up into maxvert parts

        //------------------these are variables in my paperwork //
        private static int w = maxhoriz;

        private static int h = maxvert;
        private static int wp = xwidth;
        private static int hp = yheight;

        //------------------iterations, tolerance, and bitmap object //
        // increased iterations and tighter tolerance for finer isobar detail
        private int maxiterations = 1200;

        private double tolerance = 1e-10;  // tighter tolerance for better root convergence
        private Bitmap bmp = new Bitmap(maxhoriz, maxvert);

        // render guard to avoid repeated expensive renders and message boxes
        private bool imageRendered = false;

        private bool renderingInProgress = false;
        private Task? renderTask;

        private Point pdown;
        private Point pup;
        private Graphics? g;

        #endregion CanvasVariables

        #region FractalMethods

        private void MandelBrot()
        {
            for (int px = 0; px < maxhoriz; px++)
            {
                for (int py = 0; py < maxvert; py++)
                {
                    double x = (double)(-xwidth) / 2.0 + deltax * px;
                    double y = (double)(-yheight) / 2.0 + deltay * py;
                    Complex c = new(x, y);
                    Complex zstart = Complex.Zero;
                    Complex znext = Functions.FMandelbrot(zstart, c);  // Mandelbrot interation here
                    int color = 0;
                    while (znext.Magnitude < 1000000.0 && color < 512)
                    {
                        znext = Functions.FMandelbrot(znext, c);
                        color++;
                    }
                    if (znext.Magnitude >= 1000000.0) //  if the magnitude blew up
                    {
                        color = (((color % 359) + 270) * 19) % 359;  // color based on how many iterations it required to blow up
                    }
                    else  // it did not blow up in 512 iterations
                    {
                        color = 0;  // color it black for the Mandelbrot stable points set
                    }

                    // the number of iterations to blow up (magnitude diverges)   is being plotted in the color scale
                    //bmp.SetPixel(px,  py, Color.FromArgb(ColorPalettes.Spectrum72[color].red, ColorPalettes.Spectrum72[color].green, ColorPalettes.Spectrum72[color].blue));
                    bmp.SetPixel(px, py, Color.FromArgb(ColorPalettes.Spectrum360[color].red, ColorPalettes.Spectrum360[color].green, ColorPalettes.Spectrum360[color].blue));
                }
            }
        }

        private void NewtonsMethod(int w, int h)
        {
            // map pixel coordinates into the function domain using deltax/deltay and xwidth/yheight
            double localDeltaX = (double)xwidth / (double)w;
            double localDeltaY = (double)yheight / (double)h;
            for (int px = 0; px < w; px++)
            {
                for (int py = 0; py < h; py++)
                {
                    double x = (double)(-xwidth) / 2.0 + localDeltaX * px;
                    double y = (double)(-yheight) / 2.0 + localDeltaY * py;

                    Complex zstart = new Complex(x, y);
                    Complex znext = zstart - Functions.F(zstart) / Functions.FP(zstart);  // Newton's method here
                    int iterations = 1;
                    double mag = (znext - zstart).Magnitude;
                    while (mag > tolerance && iterations < maxiterations)   // while you are not close enough to the root and you haven't exceeded the maxiterations
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
                        // use combined hue from final iterate argument and iteration count to spread colors
                        double arg = Math.Atan2(znext.Imaginary, znext.Real);
                        int hueFromArg = (int)Math.Round(((arg / (2.0 * Math.PI)) + 0.5) * 360.0) % 360;
                        int hueIndex = (hueFromArg + iterations * 17) % 360;
                        var cs = ColorPalettes.Spectrum360[(hueIndex + 360) % 360];
                        color = Color.FromArgb(cs.red, cs.green, cs.blue);
                    }

                    bmp.SetPixel(px, py, color);
                }
            }
        }  // end of NewtonsMethod declaration

        private void HailStone(int startingX, int startingY)
        {
            Pen myDrawingPen = new Pen(Color.Red);
            myDrawingPen.Width = 4.0F;
            //TextRenderer.DrawText(e.Graphics, "This is some text.", this.Font,
            //        new Point(1000, 1000), Color.White, Color.SteelBlue, TextFormatFlags.Default);
            colorstruct cs;
            for (int index = 1; index <= maxiterations; index++)
            {
                cs = ColorPalettes.Spectrum360[(index * 517) % 360];
                myDrawingPen.Color = Color.FromArgb(cs.red, cs.green, cs.blue);
                int nextX = Functions.FHailStoneNextX(startingX, startingY);
                int nextY = Functions.FHailStoneNextY(startingX, startingY);

                // label the points using the four values
                string startinglabel = "(" + Convert.ToString(index) + ", " + Convert.ToString(startingX) + ", " + Convert.ToString(startingY) + ")";
                string endinglabel = "(" + Convert.ToString(index + 1) + ", " + Convert.ToString(nextX) + ", " + Convert.ToString(nextY) + ")";
                TextRenderer.DrawText(g, startinglabel, this.Font,
                     new Point(startingX, startingY), Color.White, Color.SteelBlue, TextFormatFlags.Default);
                TextRenderer.DrawText(g, endinglabel, this.Font,
                     new Point(nextX, nextY), Color.White, Color.SteelBlue, TextFormatFlags.Default);

                // then draw the connecting line
                g.DrawLine(myDrawingPen, new PointF(startingX, startingY), new PointF(nextX, nextY));
                startingX = nextX;
                startingY = nextY;
                if (nextX == 1 && nextY == 1) break;
            }
        }

        #endregion FractalMethods

        #region CoordinateTranformations

        private Point TransformPointToDomain(Point P)
        {
            Point Q = new Point(0, 0);
            Q.X = (wp / w) * P.X - w / 2;
            Q.Y = (-hp / h) * P.Y - h / 2 + hp;
            return Q;
        }

        private Point TransformPointToPixel(Point P)
        {
            Point Q = new Point(0, 0);
            Q.X = (w / wp) * (P.X + w / 2);
            Q.Y = h - h * h / 2 / hp - (h / hp) * P.Y;
            return Q;
        }

        #endregion CoordinateTranformations

        #region CanvasMethods

        public Canvas() // constructor
        {
            InitializeComponent();
            // ensure the display panel fills the form and scales the image
            panel1.Dock = DockStyle.Fill;
            // use Zoom so the image preserves aspect ratio; we'll render an image matching the panel aspect ratio
            panel1.BackgroundImageLayout = ImageLayout.Zoom;
            //this.BackColor = Color.Black;
            //this.MouseClick += mouseClick;
        }

        private void Canvas_Paint(object sender, PaintEventArgs e)
        {
            // start background render once
            if (imageRendered || renderingInProgress) return;

            renderingInProgress = true;
            renderTask = Task.Run(() =>
            {
                try
                {
                    // choose a render size that matches the panel aspect ratio to avoid letterboxing when using Zoom
                    int panelW = Math.Max(1, panel1.ClientSize.Width);
                    int panelH = Math.Max(1, panel1.ClientSize.Height);
                    double panelRatio = panelW / (double)panelH;
                    double baseRatio = maxhoriz / (double)maxvert;
                    int targetW, targetH;
                    if (panelRatio >= baseRatio)
                    {
                        // panel is wider: use full internal height, increase width to match aspect
                        targetH = maxvert;
                        targetW = Math.Max(1, (int)Math.Round(maxvert * panelRatio));
                    }
                    else
                    {
                        // panel is taller: use full internal width, increase height to match aspect
                        targetW = maxhoriz;
                        targetH = Math.Max(1, (int)Math.Round(maxhoriz / panelRatio));
                    }

                    var renderBmp = new Bitmap(targetW, targetH);
                    double localDeltaX = (double)xwidth / (double)targetW;
                    double localDeltaY = (double)yheight / (double)targetH;
                    for (int px = 0; px < targetW; px++)
                    {
                        for (int py = 0; py < targetH; py++)
                        {
                            double x = (double)(-xwidth) / 2.0 + localDeltaX * px;
                            double y = (double)(-yheight) / 2.0 + localDeltaY * py;

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
                                color = Color.FromArgb(0, 0, 0);
                            else
                            {
                                // combine iteration count with the final iterate's argument to spread hues
                                double arg = Math.Atan2(znext.Imaginary, znext.Real);
                                int hueFromArg = (int)Math.Round(((arg / (2.0 * Math.PI)) + 0.5) * 360.0) % 360;
                                int hueIndex = (hueFromArg + iterations * 17) % 360; // 17 spreads bands around palette
                                var cs = ColorPalettes.Spectrum360[(hueIndex + 360) % 360];
                                color = Color.FromArgb(cs.red, cs.green, cs.blue);
                            }
                            renderBmp.SetPixel(px, py, color);
                        }
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
                            panel1.BackgroundImage = bmp; // Zoom preserves aspect; image matches panel aspect so it will fill
                            MessageBox.Show("image finished");
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
            //SaveSpectrum360AsHtml(Path.ChangeExtension(path, "html"), cols, swatchSize);
        }

        #endregion CanvasMethods

        #region MouseMethods

        private void Canvas_MouseDown(object sender, MouseEventArgs e)
        {
            pdown = e.Location;
            //MessageBox.Show("Mouse down");
        }

        private void Canvas_MouseUp(object sender, MouseEventArgs e)
        {
            pup = e.Location;
            //MessageBox.Show("Mouse up");
        }

        //private void mouseClick(object sender, MouseEventArgs e)
        //{
        //    if (e.Button == MouseButtons.Left)
        //        Trace.WriteLine("Mouse clicked");
        //}

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