# Contributor's Guide - Adding New Visualizations

This guide walks you through adding a new visualization type to the project.

## Step-by-Step Tutorial

Let's add a **Julia Set** visualization as an example.

### Step 1: Create the Configuration Class

Create a new file `JuliaVisualization.cs` in the `Visualizations` folder:

```csharp
using System.Drawing;
using System.Numerics;
using static NumericalVisualizations.ScreenStructures;

namespace NumericalVisualizations.Visualizations
{
    /// <summary>
    /// Configuration for Julia set visualization
    /// </summary>
    public class JuliaConfig : VisualizationConfig
    {
        // Julia set constant (try different values for different sets)
        public Complex C { get; set; } = new Complex(-0.7, 0.27015);
        
        public double EscapeRadius { get; set; } = 2.0;
        public int ColorOffset { get; set; } = 0;
        
        public JuliaConfig()
        {
            MaxIterations = 256;
            Tolerance = 0.0;
        }
    }

    /// <summary>
    /// Julia set fractal visualization
    /// </summary>
    public class JuliaVisualization : IVisualization
    {
        private readonly JuliaConfig _config;

        public string Name => "Julia Set";
        public string Description => "Julia set fractal with configurable constant";

        public JuliaVisualization(JuliaConfig? config = null)
        {
            _config = config ?? new JuliaConfig();
        }

        public Bitmap Render(int width, int height, double xRange, double yRange)
        {
            var bitmap = new Bitmap(width, height);
            double deltaX = xRange / width;
            double deltaY = yRange / height;

            for (int px = 0; px < width; px++)
            {
                for (int py = 0; py < height; py++)
                {
                    double x = -xRange / 2.0 + deltaX * px;
                    double y = -yRange / 2.0 + deltaY * py;
                    
                    Complex z = new Complex(x, y);
                    int iterations = 0;

                    while (z.Magnitude < _config.EscapeRadius && iterations < _config.MaxIterations)
                    {
                        z = z * z + _config.C;
                        iterations++;
                    }

                    Color color;
                    if (z.Magnitude >= _config.EscapeRadius)
                    {
                        int colorIndex = (iterations + _config.ColorOffset) % 360;
                        var cs = ColorPalettes.Spectrum360[colorIndex];
                        color = Color.FromArgb(cs.red, cs.green, cs.blue);
                    }
                    else
                    {
                        color = Color.FromArgb(0, 0, 0);
                    }

                    bitmap.SetPixel(px, py, color);
                }
            }

            return bitmap;
        }
    }
}
```

### Step 2: Update the Factory

Edit `VisualizationFactory.cs`:

```csharp
public enum VisualizationType
{
    Newton,
    Mandelbrot,
    Hailstone,
    Julia  // ADD THIS
}

public static IVisualization Create(VisualizationType type)
{
    return type switch
    {
        VisualizationType.Newton => new NewtonVisualization(),
        VisualizationType.Mandelbrot => new MandelbrotVisualization(),
        VisualizationType.Hailstone => new HailstoneVisualization(),
        VisualizationType.Julia => new JuliaVisualization(),  // ADD THIS
        _ => throw new ArgumentException($"Unknown visualization type: {type}")
    };
}

public static Dictionary<VisualizationType, (string Name, string Description)> GetAvailableVisualizations()
{
    return new Dictionary<VisualizationType, (string, string)>
    {
        { VisualizationType.Newton, ("Newton's Method", "Root-finding in complex plane") },
        { VisualizationType.Mandelbrot, ("Mandelbrot Set", "Classic fractal") },
        { VisualizationType.Hailstone, ("Hailstone Sequence", "Collatz conjecture visualization") },
        { VisualizationType.Julia, ("Julia Set", "Fractal with configurable constant") }  // ADD THIS
    };
}
```

### Step 3: Test Your Visualization

Update `Program.cs` to test:

```csharp
static void Main()
{
    ApplicationConfiguration.Initialize();
    
    var canvas = new Canvas();
    canvas.SetVisualization(VisualizationFactory.VisualizationType.Julia);
    
    Application.Run(canvas);
}
```

Or programmatically:

```csharp
var config = new JuliaConfig 
{ 
    C = new Complex(-0.4, 0.6),  // Different Julia set
    MaxIterations = 512 
};
var julia = new JuliaVisualization(config);
var bitmap = julia.Render(1920, 1080, 4.0, 4.0);
bitmap.Save(@"C:\Temp\julia.png");
```

## Best Practices

### 1. Configuration Defaults

Always provide sensible defaults in the constructor:

```csharp
public MyConfig()
{
    MaxIterations = 100;  // Reasonable default
    Tolerance = 1e-6;     // Sensible precision
}
```

### 2. Parameter Documentation

Document what each parameter does:

```csharp
/// <summary>
/// Escape radius - values beyond this are considered divergent
/// Typical range: 2.0 to 1000000.0
/// </summary>
public double EscapeRadius { get; set; } = 2.0;
```

### 3. Performance Considerations

For expensive operations, consider:

```csharp
// Cache frequently used values
double escapeRadiusSquared = _config.EscapeRadius * _config.EscapeRadius;

// Use squared magnitude to avoid sqrt
while (z.Real * z.Real + z.Imaginary * z.Imaginary < escapeRadiusSquared)
{
    // ...
}
```

### 4. Color Mapping

Use the existing `Spectrum360` palette for consistency:

```csharp
// Simple mapping
var cs = ColorPalettes.Spectrum360[iterations % 360];

// With offset
var cs = ColorPalettes.Spectrum360[(iterations + offset) % 360];

// Based on value
int hueIndex = (int)(value * 360.0) % 360;
var cs = ColorPalettes.Spectrum360[hueIndex];
```

### 5. Error Handling

Handle edge cases gracefully:

```csharp
public Bitmap Render(int width, int height, double xRange, double yRange)
{
    if (width <= 0 || height <= 0)
        throw new ArgumentException("Width and height must be positive");
        
    var bitmap = new Bitmap(width, height);
    // ...
}
```

## Ideas for New Visualizations

Here are some suggestions for visualizations you could add:

### Easy
- **Burning Ship Fractal** - Similar to Mandelbrot
- **Tricorn Fractal** - Conjugate version of Mandelbrot
- **Different Newton Fractals** - Change the function in `Functions.cs`

### Moderate
- **Lorenz Attractor** - 3D chaotic system projected to 2D
- **Bifurcation Diagram** - Shows chaos in parametric systems
- **Apollonian Gasket** - Circle packing fractal
- **Sierpinski Triangle** - Classic fractal triangle

### Advanced
- **Domain Coloring** - Visualize complex functions using color
- **Buddhabrot** - Sampling-based Mandelbrot variant
- **Flame Fractals** - IFS (Iterated Function Systems)
- **L-Systems** - Lindenmayer systems for plant-like fractals

## Testing Checklist

Before submitting your visualization:

- [ ] Builds without errors or warnings
- [ ] Renders correctly at various resolutions
- [ ] Configuration parameters have sensible defaults
- [ ] Code is documented with XML comments
- [ ] Added to `VisualizationFactory` enum and methods
- [ ] Tested with different parameter values
- [ ] No obvious performance issues
- [ ] Follows existing code style

## Getting Help

If you need assistance:
1. Check existing visualization implementations as examples
2. Review the `README.md` for architecture overview
3. Look at `EXAMPLES.md` for usage patterns
4. Open an issue on GitHub with your questions

Happy coding! 🎨
