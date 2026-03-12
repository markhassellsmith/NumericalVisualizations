using System.ComponentModel;
using System.Drawing;
using System.Numerics;
using NumericalVisualizations.Performance;
using static NumericalVisualizations.ScreenStructures;

namespace NumericalVisualizations.Visualizations
{
    /// <summary>
    /// Configuration for Newton's method visualization
    /// </summary>
    public class NewtonConfig : VisualizationConfig
    {
        [Category("Appearance")]
        [Description("Color variation per iteration (affects banding patterns)")]
        public int HueSpread { get; set; } = 17;

        public NewtonConfig()
        {
            MaxIterations = 1200;
            Tolerance = 1e-10;
            ShowAxes = false;  // Default off for fractals (can be toggled)
        }
    }

    /// <summary>
    /// Newton's method fractal visualization
    /// </summary>
    public class NewtonVisualization : IVisualization
    {
        private readonly NewtonConfig _config;

        public string Name => "Newton's Method";
        public string Description => "Visualization of Newton's method root-finding algorithm in the complex plane";

        public NewtonVisualization(NewtonConfig? config = null)
        {
            _config = config ?? new NewtonConfig();
        }

        public VisualizationConfig GetConfig()
        {
            return _config;
        }

        public IVisualization WithConfig(VisualizationConfig config)
        {
            return new NewtonVisualization(config as NewtonConfig);
        }

        public Bitmap Render(int width, int height, double xRange, double yRange)
        {
            var bitmap = new Bitmap(width, height);

            RenderingHelpers.RenderFast(bitmap, xRange, yRange, (x, y) =>
            {
                Complex zstart = new Complex(x, y);
                Complex znext = zstart - Functions.F(zstart) / Functions.FP(zstart);
                int iterations = 1;
                double mag = (znext - zstart).Magnitude;

                while (mag > _config.Tolerance && iterations < _config.MaxIterations)
                {
                    zstart = znext;
                    znext = zstart - Functions.F(zstart) / Functions.FP(zstart);
                    mag = (znext - zstart).Magnitude;
                    iterations++;
                }

                Color color;
                if (iterations >= _config.MaxIterations)
                {
                    color = Color.FromArgb(0, 0, 0);
                }
                else
                {
                    double arg = Math.Atan2(znext.Imaginary, znext.Real);
                    int hueFromArg = (int)Math.Round(((arg / (2.0 * Math.PI)) + 0.5) * 360.0) % 360;
                    int hueIndex = (hueFromArg + iterations * _config.HueSpread) % 360;
                    var cs = ColorPalettes.Spectrum360[(hueIndex + 360) % 360];
                    color = Color.FromArgb(cs.red, cs.green, cs.blue);
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
