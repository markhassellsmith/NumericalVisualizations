using System.ComponentModel;

namespace NumericalVisualizations.Visualizations
{
    /// <summary>
    /// Base configuration for all visualizations
    /// </summary>
    public abstract class VisualizationConfig
    {
        [Category("Algorithm")]
        [Description("Maximum number of iterations before stopping")]
        public int MaxIterations { get; set; }

        [Category("Algorithm")]
        [Description("Convergence threshold (smaller = more precise)")]
        public double Tolerance { get; set; }

        [Category("Display - Universal")]
        [Description("Show X and Y axes with tick marks")]
        public bool ShowAxes { get; set; } = true;
    }
}
