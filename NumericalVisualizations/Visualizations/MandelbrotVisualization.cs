using System.ComponentModel;
using System.Drawing;
using System.Numerics;
using NumericalVisualizations.Performance;
using static NumericalVisualizations.ScreenStructures;

namespace NumericalVisualizations.Visualizations
{
    /// <summary>
    /// Configuration for Mandelbrot set visualization
    /// </summary>
    public class MandelbrotConfig : VisualizationConfig
    {
        [Category("Algorithm")]
        [Description("Boundary for considering a point escaped to infinity")]
        public double EscapeRadius { get; set; } = 1000000.0;

        [Category("Appearance")]
        [Description("Shifts the color palette (0-359)")]
        public int ColorOffset { get; set; } = 270;

        [Category("Appearance")]
        [Description("Controls color frequency/speed of change")]
        public int ColorMultiplier { get; set; } = 19;

        [Category("Appearance")]
        [Description("Modulo for color wrapping (typically 359)")]
        public int ColorModulo { get; set; } = 359;

        public MandelbrotConfig()
        {
            MaxIterations = 512;
            Tolerance = 0.0;
            ShowAxes = false;  // Default off for fractals (can be toggled)
        }
    }

    /// <summary>
    /// Mandelbrot set visualization
    /// </summary>
    public class MandelbrotVisualization : IVisualization
    {
        private readonly MandelbrotConfig _config;

        public string Name => "Mandelbrot Set";
        public string Description => "Classic Mandelbrot set fractal in the complex plane";

        public MandelbrotVisualization(MandelbrotConfig? config = null)
        {
            _config = config ?? new MandelbrotConfig();
        }

        public VisualizationConfig GetConfig()
        {
            return _config;
        }

        public IVisualization WithConfig(VisualizationConfig config)
        {
            return new MandelbrotVisualization(config as MandelbrotConfig);
        }

        public Bitmap Render(int width, int height, double xRange, double yRange)
        {
            var bitmap = new Bitmap(width, height);

            RenderingHelpers.RenderFast(bitmap, xRange, yRange, (x, y) =>
            {
                Complex c = new(x, y);
                Complex zstart = Complex.Zero;
                Complex znext = Functions.FMandelbrot(zstart, c);
                int iterations = 0;

                while (znext.Magnitude < _config.EscapeRadius && iterations < _config.MaxIterations)
                {
                    znext = Functions.FMandelbrot(znext, c);
                    iterations++;
                }

                Color color;
                if (znext.Magnitude >= _config.EscapeRadius)
                {
                    int colorIndex = (((iterations % _config.ColorModulo) + _config.ColorOffset) * _config.ColorMultiplier) % _config.ColorModulo;
                    var cs = ColorPalettes.Spectrum360[colorIndex];
                    color = Color.FromArgb(cs.red, cs.green, cs.blue);
                }
                else
                {
                    color = Color.FromArgb(0, 0, 0);
                }

                return color;
            });

            // Draw axes if enabled
            if (_config.ShowAxes)
            {
                RenderingHelpers.DrawAxesOnBitmap(bitmap, xRange, yRange);
            }

            return bitmap;
        }
    }
}
