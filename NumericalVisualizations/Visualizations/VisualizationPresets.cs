using System.ComponentModel;

namespace NumericalVisualizations.Visualizations
{
    /// <summary>
    /// Predefined configuration presets for all visualizations
    /// </summary>
    public static class VisualizationPresets
    {
        /// <summary>
        /// Newton's Method presets
        /// </summary>
        public static Dictionary<string, NewtonConfig> NewtonPresets = new()
        {
            ["Default"] = new NewtonConfig(),
            
            ["High Detail"] = new NewtonConfig 
            { 
                MaxIterations = 5000, 
                Tolerance = 1e-15,
                HueSpread = 17
            },
            
            ["Fast Preview"] = new NewtonConfig 
            { 
                MaxIterations = 200, 
                Tolerance = 1e-6,
                HueSpread = 17
            },
            
            ["Vibrant Colors"] = new NewtonConfig 
            { 
                MaxIterations = 1200,
                Tolerance = 1e-10,
                HueSpread = 40
            },
            
            ["Subtle Bands"] = new NewtonConfig 
            { 
                MaxIterations = 1200,
                Tolerance = 1e-10,
                HueSpread = 5
            }
        };

        /// <summary>
        /// Mandelbrot Set presets
        /// </summary>
        public static Dictionary<string, MandelbrotConfig> MandelbrotPresets = new()
        {
            ["Classic"] = new MandelbrotConfig(),
            
            ["Deep Zoom"] = new MandelbrotConfig 
            { 
                MaxIterations = 2000,
                EscapeRadius = 1000000.0,
                ColorOffset = 270,
                ColorMultiplier = 19,
                ColorModulo = 359
            },
            
            ["Psychedelic"] = new MandelbrotConfig 
            { 
                MaxIterations = 512,
                EscapeRadius = 1000000.0,
                ColorOffset = 0,
                ColorMultiplier = 50,
                ColorModulo = 359
            },
            
            ["Smooth Gradient"] = new MandelbrotConfig 
            { 
                MaxIterations = 512,
                EscapeRadius = 1000000.0,
                ColorOffset = 180,
                ColorMultiplier = 5,
                ColorModulo = 359
            },
            
            ["Fast Preview"] = new MandelbrotConfig 
            { 
                MaxIterations = 128,
                EscapeRadius = 1000000.0,
                ColorOffset = 270,
                ColorMultiplier = 19,
                ColorModulo = 359
            }
        };

        /// <summary>
        /// Hailstone Sequence presets - All use the same starting point for comparison
        /// Focus on analytical perspectives (iterations/scale), not display options
        /// </summary>
        public static Dictionary<string, HailstoneConfig> HailstonePresets = new()
        {
            ["Default - Balanced View"] = new HailstoneConfig 
            { 
                StartIntX = -10,
                StartIntY = 6,
                MaxIterations = 150,
                ScaleFactorX = 0.0,  // Auto-calculate
                ScaleFactorY = 0.0,  // Auto-calculate
                LineWidth = 0.002f,
                DotSize = 0.012f,
                ColorSpread = 7,
                ShowPointLabels = true,
                ShowDots = true,
                ShowAxes = true
            },

            ["First 50 Steps - Early Behavior"] = new HailstoneConfig 
            { 
                StartIntX = -10,
                StartIntY = 6,
                MaxIterations = 50,
                ScaleFactorX = 0.0,  // Auto-calculate (will be larger for fewer steps)
                ScaleFactorY = 0.0,  // Auto-calculate
                LineWidth = 0.003f,
                DotSize = 0.018f,
                ColorSpread = 10,
                ShowPointLabels = true,
                ShowDots = true,
                ShowAxes = true
            },

            ["300 Steps - Long Term Behavior"] = new HailstoneConfig 
            { 
                StartIntX = -10,
                StartIntY = 6,
                MaxIterations = 300,
                ScaleFactorX = 0.0,  // Auto-calculate (will be smaller for more steps)
                ScaleFactorY = 0.0,  // Auto-calculate
                LineWidth = 0.001f,
                DotSize = 0.006f,
                ColorSpread = 5,
                ShowPointLabels = false,  // Too crowded with 300 points
                ShowDots = true,
                ShowAxes = true
            }
        };

        /// <summary>
        /// Get all preset names for a visualization type
        /// </summary>
        public static List<string> GetPresetNames(VisualizationFactory.VisualizationType type)
        {
            return type switch
            {
                VisualizationFactory.VisualizationType.Newton => NewtonPresets.Keys.ToList(),
                VisualizationFactory.VisualizationType.Mandelbrot => MandelbrotPresets.Keys.ToList(),
                VisualizationFactory.VisualizationType.Hailstone => HailstonePresets.Keys.ToList(),
                _ => new List<string>()
            };
        }

        /// <summary>
        /// Get a preset configuration by name and type
        /// </summary>
        public static VisualizationConfig? GetPreset(VisualizationFactory.VisualizationType type, string presetName)
        {
            return type switch
            {
                VisualizationFactory.VisualizationType.Newton => 
                    NewtonPresets.TryGetValue(presetName, out var nc) ? nc : null,
                VisualizationFactory.VisualizationType.Mandelbrot => 
                    MandelbrotPresets.TryGetValue(presetName, out var mc) ? mc : null,
                VisualizationFactory.VisualizationType.Hailstone => 
                    HailstonePresets.TryGetValue(presetName, out var hc) ? hc : null,
                _ => null
            };
        }
    }
}
