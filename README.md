# Numerical Visualizations

A .NET 6 WinForms application for visualizing various numerical and mathematical concepts through beautiful, interactive graphics.

## Architecture

The project follows a clean, extensible architecture that makes it easy to add new visualization types.

### Core Components

#### 1. **Visualization Interface** (`IVisualization`)
- Defines the contract for all visualizations
- Each visualization must provide:
  - `Name` - Display name
  - `Description` - Brief explanation
  - `Render()` - Method to generate the visualization bitmap

#### 2. **Visualization Factory** (`VisualizationFactory`)
- Creates visualization instances
- Manages available visualization types
- Provides metadata about each visualization

#### 3. **Configuration Classes**
- Each visualization has its own configuration class extending `VisualizationConfig`
- Allows customization of parameters (iterations, tolerance, colors, etc.)

### Current Visualizations

#### Newton's Method (`NewtonVisualization`)
Visualizes Newton's root-finding algorithm in the complex plane. Shows the basins of attraction for different roots through color coding.

**Configuration:**
- `MaxIterations` - Maximum iteration count (default: 1200)
- `Tolerance` - Convergence threshold (default: 1e-10)
- `HueSpread` - Color variation per iteration (default: 17)

#### Mandelbrot Set (`MandelbrotVisualization`)
Classic fractal showing the Mandelbrot set in the complex plane.

**Configuration:**
- `MaxIterations` - Maximum iteration count (default: 512)
- `EscapeRadius` - Boundary for divergence (default: 1000000.0)
- `ColorOffset`, `ColorMultiplier`, `ColorModulo` - Color mapping parameters

#### Hailstone Sequence (`HailstoneVisualization`)
2D visualization of the Collatz conjecture (3n+1 problem).

**Configuration:**
- `MaxIterations` - Maximum steps to follow (default: 1200)
- `StartX`, `StartY` - Starting coordinates
- `LineWidth` - Width of connecting lines (default: 4.0)
- `ShowLabels` - Whether to display coordinate labels (default: true)
- `ColorSpread` - Color variation between steps (default: 517)

## Adding New Visualizations

To add a new visualization type:

1. **Create Configuration Class** (optional)
```csharp
public class MyVisualizationConfig : VisualizationConfig
{
    public int CustomParameter { get; set; } = 100;
}
```

2. **Implement IVisualization**
```csharp
public class MyVisualization : IVisualization
{
    private readonly MyVisualizationConfig _config;
    
    public string Name => "My Visualization";
    public string Description => "Description of what it does";
    
    public MyVisualization(MyVisualizationConfig? config = null)
    {
        _config = config ?? new MyVisualizationConfig();
    }
    
    public Bitmap Render(int width, int height, double xRange, double yRange)
    {
        var bitmap = new Bitmap(width, height);
        // Render your visualization logic here
        return bitmap;
    }
}
```

3. **Update VisualizationFactory**
- Add to `VisualizationType` enum
- Add case in `Create()` method
- Add entry in `GetAvailableVisualizations()`

4. **Use in Canvas**
```csharp
canvas.SetVisualization(VisualizationFactory.VisualizationType.MyVisualization);
```

## Project Structure

```
NumericalVisualizations/
├── Visualizations/
│   ├── IVisualization.cs           - Core interface
│   ├── VisualizationConfig.cs      - Base configuration
│   ├── VisualizationFactory.cs     - Factory pattern implementation
│   ├── NewtonVisualization.cs      - Newton's method implementation
│   ├── MandelbrotVisualization.cs  - Mandelbrot set implementation
│   └── HailstoneVisualization.cs   - Hailstone sequence implementation
├── Canvas.cs                       - Main UI form
├── Functions.cs                    - Mathematical functions
├── ColorPalettes.cs                - Color spectrum definitions
├── PaletteHelpers.cs               - Palette utility methods
└── Screen.cs                       - Screen structure definitions
```

## Key Features

- **Extensible Architecture** - Easy to add new visualization types
- **Async Rendering** - Non-blocking UI during computation
- **Configurable Parameters** - Each visualization has customizable settings
- **Rich Color Palettes** - 360-degree spectrum for beautiful visualizations
- **High Resolution** - Supports rendering up to 2000x2000 pixels

## Mathematical Functions

The `Functions` class provides the mathematical operations used by visualizations:
- `F(z)` - Complex function for Newton's method
- `FP(z)` - Derivative of F for Newton's method
- `FMandelbrot(z, c)` - Mandelbrot iteration function
- `FHailStoneNextX/Y(x, y)` - 2D Hailstone sequence rules

## Color System

Uses a 360-color spectrum (`Spectrum360`) with HSL-based color generation:
- Full hue rotation from 0° to 360°
- Saturation: 100%
- Lightness: 50%

Utilities available:
- `SaveSpectrum360AsImage()` - Export palette as PNG
- `SaveSpectrum360AsHtml()` - Export palette as interactive HTML

## Future Enhancements

Potential additions:
- Julia sets
- Lorenz attractor
- Bifurcation diagrams
- L-systems
- Complex function domain coloring
- Interactive parameter adjustment UI
- Zoom and pan functionality
- Animation support

## License

MIT License - Feel free to use and extend this project!
