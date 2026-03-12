# Examples - Using Different Visualizations

This document shows examples of how to use and configure different visualizations in the application.

## Basic Usage

### Switching Visualizations

```csharp
// In Canvas form or from menu
canvas.SetVisualization(VisualizationFactory.VisualizationType.Newton);
canvas.SetVisualization(VisualizationFactory.VisualizationType.Mandelbrot);
canvas.SetVisualization(VisualizationFactory.VisualizationType.Hailstone);
```

## Custom Configuration Examples

### Newton's Method with Custom Settings

```csharp
var config = new NewtonConfig
{
    MaxIterations = 2000,      // More iterations for finer detail
    Tolerance = 1e-12,         // Tighter convergence
    HueSpread = 25             // Different color spread
};

var newton = new NewtonVisualization(config);
var bitmap = newton.Render(1920, 1080, 4.0, 4.0);
```

### Mandelbrot Set with Custom Colors

```csharp
var config = new MandelbrotConfig
{
    MaxIterations = 1000,
    EscapeRadius = 10000.0,
    ColorOffset = 180,          // Shift color palette
    ColorMultiplier = 25,       // Change color frequency
    ColorModulo = 359
};

var mandelbrot = new MandelbrotVisualization(config);
var bitmap = mandelbrot.Render(2000, 2000, 4.0, 4.0);
```

### Hailstone Sequence Visualization

```csharp
var config = new HailstoneConfig
{
    MaxIterations = 500,
    StartX = 800,               // Start from specific point
    StartY = 600,
    LineWidth = 2.0f,           // Thinner lines
    ShowLabels = false,         // Hide coordinate labels
    ColorSpread = 300           // Different color progression
};

var hailstone = new HailstoneVisualization(config);
var bitmap = hailstone.Render(1600, 1200, 4.0, 4.0);
```

## Getting All Available Visualizations

```csharp
var visualizations = VisualizationFactory.GetAvailableVisualizations();

foreach (var kvp in visualizations)
{
    var type = kvp.Key;
    var (name, description) = kvp.Value;
    
    Console.WriteLine($"{type}: {name}");
    Console.WriteLine($"  {description}");
    
    // Create instance
    var viz = VisualizationFactory.Create(type);
}
```

## Exporting Color Palettes

```csharp
// Export palette as PNG image
PaletteHelpers.SaveSpectrum360AsImage(@"C:\Temp\palette.png", swatchSize: 30, cols: 40);

// Export palette as HTML (auto-opens in browser)
PaletteHelpers.SaveSpectrum360AsHtml(@"C:\Temp\palette.html", cols: 36, swatchPx: 35);
```

## Changing Mathematical Functions

To use different functions in Newton's method, edit `Functions.cs`:

```csharp
// Current: 6z^4 + 4z^2 - z + 1
public static Complex F(Complex zin)
{
    return 6.0 * zin * zin * zin * zin + 4.0 * zin * zin - zin + 1.0;
}

// Example alternatives (uncomment to use):

// Polynomial: z^3 - 1
// Complex zout = zin * zin * zin - 1.0;

// Trigonometric: sin(z)
// Complex zout = Complex.Sin(zin);

// Rational: (4z^2 - 3z + 7) / (z + 4)
// Complex zout = (4.0 * zin * zin - 3.0 * zin + 7.0) / (zin + 4.0);

// Exponential-trig: e^(-z) * sin(z)
// Complex zout = Complex.Exp(-zin) * Complex.Sin(zin);
```

Don't forget to update the derivative `FP(z)` to match!

## Rendering to File

```csharp
// Create a visualization
var viz = VisualizationFactory.Create(VisualizationFactory.VisualizationType.Newton);

// Render at high resolution
var bitmap = viz.Render(4000, 4000, 4.0, 4.0);

// Save to file
Directory.CreateDirectory(@"C:\Temp");
bitmap.Save(@"C:\Temp\newton_4k.png", System.Drawing.Imaging.ImageFormat.Png);
bitmap.Dispose();
```

## Performance Tips

1. **Start with lower resolutions** during development (e.g., 800x600)
2. **Reduce MaxIterations** for faster previews
3. **Use async rendering** to keep UI responsive (already implemented in Canvas)
4. **Save renders** to avoid re-computing expensive visualizations

## Next Steps

Consider adding:
- UI controls to switch visualizations at runtime
- Parameter sliders for real-time adjustment
- Zoom/pan functionality
- Animation by varying parameters over time
- Batch rendering for creating videos
