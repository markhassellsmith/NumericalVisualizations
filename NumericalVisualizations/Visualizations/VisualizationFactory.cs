namespace NumericalVisualizations.Visualizations
{
    /// <summary>
    /// Factory for creating visualization instances
    /// </summary>
    public static class VisualizationFactory
    {
        /// <summary>
        /// Available visualization types
        /// </summary>
        public enum VisualizationType
        {
            Newton,
            Mandelbrot,
            Hailstone
        }

        /// <summary>
        /// Create a visualization instance by type with default configuration
        /// </summary>
        public static IVisualization Create(VisualizationType type)
        {
            return type switch
            {
                VisualizationType.Newton => new NewtonVisualization(),
                VisualizationType.Mandelbrot => new MandelbrotVisualization(),
                VisualizationType.Hailstone => new HailstoneVisualization(),
                _ => throw new ArgumentException($"Unknown visualization type: {type}")
            };
        }

        /// <summary>
        /// Create a visualization instance by type with custom configuration
        /// </summary>
        public static IVisualization Create(VisualizationType type, VisualizationConfig config)
        {
            return type switch
            {
                VisualizationType.Newton => new NewtonVisualization(config as NewtonConfig),
                VisualizationType.Mandelbrot => new MandelbrotVisualization(config as MandelbrotConfig),
                VisualizationType.Hailstone => new HailstoneVisualization(config as HailstoneConfig),
                _ => throw new ArgumentException($"Unknown visualization type: {type}")
            };
        }

        /// <summary>
        /// Get all available visualization types with their metadata
        /// </summary>
        public static Dictionary<VisualizationType, (string Name, string Description)> GetAvailableVisualizations()
        {
            return new Dictionary<VisualizationType, (string, string)>
            {
                { VisualizationType.Newton, ("Newton's Method", "Root-finding in complex plane") },
                { VisualizationType.Mandelbrot, ("Mandelbrot Set", "Classic fractal") },
                { VisualizationType.Hailstone, ("Hailstone Sequence", "Collatz conjecture visualization") }
            };
        }
    }
}
