using System.Drawing;

namespace NumericalVisualizations.Visualizations
{
    /// <summary>
    /// Base interface for all numerical visualizations
    /// </summary>
    public interface IVisualization
    {
        /// <summary>
        /// Name of the visualization type
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Description of the visualization
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Render the visualization to a bitmap
        /// </summary>
        /// <param name="width">Target bitmap width</param>
        /// <param name="height">Target bitmap height</param>
        /// <param name="xRange">Horizontal range to visualize</param>
        /// <param name="yRange">Vertical range to visualize</param>
        /// <returns>Rendered bitmap</returns>
        Bitmap Render(int width, int height, double xRange, double yRange);

        /// <summary>
        /// Get the current configuration object for this visualization
        /// </summary>
        /// <returns>Configuration object that can be edited</returns>
        VisualizationConfig GetConfig();

        /// <summary>
        /// Create a new instance with updated configuration
        /// </summary>
        /// <param name="config">Updated configuration</param>
        /// <returns>New visualization instance with the configuration applied</returns>
        IVisualization WithConfig(VisualizationConfig config);
    }
}
