# Contributing to Numerical Visualizations

Thank you for your interest in contributing! This guide will help you add new visualizations and extend the system.

---

## Table of Contents
1. [Architecture Overview](#architecture-overview)
2. [Adding a New Visualization](#adding-a-new-visualization)
3. [Adding Presets](#adding-presets)
4. [Display Options](#display-options)
5. [Performance Considerations](#performance-considerations)
6. [Testing Guidelines](#testing-guidelines)

---

## Architecture Overview

### Key Components

1. **IVisualization** - Interface all visualizations implement
2. **VisualizationConfig** - Base configuration class
3. **VisualizationFactory** - Creates visualization instances
4. **VisualizationPresets** - Predefined configurations
5. **Canvas** - Main UI coordinator

### Configuration Hierarchy

```
VisualizationConfig (base)
├─ ShowAxes (universal)
├─ MaxIterations
└─ Tolerance

NewtonConfig : VisualizationConfig
└─ HueSpread

MandelbrotConfig : VisualizationConfig
├─ EscapeRadius
└─ Color mapping parameters

HailstoneConfig : VisualizationConfig
├─ ShowPointLabels (specific)
├─ ShowDots (specific)
└─ Path rendering parameters
```

---

## Adding a New Visualization

Follow these steps to add a new visualization type.

### Step 1: Create Configuration Class

```csharp
using System.ComponentModel;

public class MyVisualizationConfig : VisualizationConfig
{
    [Category("Algorithm")]
    [Description("Your algorithm parameter")]
    public int MyParameter { get; set; } = 100;
    
    [Category("Appearance")]
    [Description("Visual parameter")]
    public double MyVisualSetting { get; set; } = 1.0;
    
    public MyVisualizationConfig()
    {
        MaxIterations = 500;
        Tolerance = 1e-8;
        ShowAxes = false;
    }
}
```

### Step 2: Implement IVisualization

```csharp
public class MyVisualization : IVisualization
{
    private readonly MyVisualizationConfig _config;
    
    public string Name => "My Visualization";
    public string Description => "Description of what it visualizes";
    
    public MyVisualization(MyVisualizationConfig? config = null)
    {
        _config = config ?? new MyVisualizationConfig();
    }
    
    public VisualizationConfig GetConfig() => _config;
    
    public IVisualization WithConfig(VisualizationConfig config)
    {
        return new MyVisualization(config as MyVisualizationConfig);
    }
    
    public Bitmap Render(int width, int height, double xRange, double yRange)
    {
        var bitmap = new Bitmap(width, height);
        
        // For pixel-based rendering
        RenderingHelpers.RenderFast(bitmap, xRange, yRange, (x, y) =>
        {
            // Your computation
            return Color.FromArgb(r, g, b);
        });
        
        // Add axes if enabled
        if (_config.ShowAxes)
        {
            RenderingHelpers.DrawAxesOnBitmap(bitmap, xRange, yRange);
        }
        
        return bitmap;
    }
}
```

### Step 3: Add to Factory

Update `VisualizationFactory.cs` to include your new type.

### Step 4: Add Menu Items and Event Handlers

Add menu integration in `Canvas.cs` and `Canvas.Designer.cs`.

---

## Adding Presets

Presets focus on analytical perspectives, not display options.

### Guidelines

1. **Same starting conditions** for comparable results
2. **Different analytical views** - Vary iterations, scale
3. **Descriptive names** - Indicate what the preset shows
4. **3-5 presets** - Don't overwhelm users

### Example

```csharp
public static Dictionary<string, MyVisualizationConfig> MyVisualizationPresets = new()
{
    ["Default - Balanced View"] = new MyVisualizationConfig
    {
        MaxIterations = 500,
        ShowAxes = true
    },
    
    ["High Detail"] = new MyVisualizationConfig
    {
        MaxIterations = 2000,
        ShowAxes = true
    }
};
```

---

## Display Options

### Universal Options

**ShowAxes** - Available for all visualizations
- Add to toolbar for live toggling
- Use cached overlay for fractals (performance)

### Visualization-Specific Options

Add to derived configs when needed:

```csharp
[Category("Display - MyVisualization Specific")]
[Description("Show custom feature")]
public bool ShowCustomFeature { get; set; } = true;
```

---

## Performance Considerations

### Rendering Strategies

**Pixel-Based:** Use `RenderingHelpers.RenderFast()` with parallel computation

**Vector-Based:** Use Graphics API, integrate axes into rendering

### Optimization Tips

1. Use LockBits for pixel manipulation
2. Parallelize with `Parallel.For`
3. Cache overlays for expensive computations
4. Dispose resources properly

---

## Testing Checklist

- [ ] Visualization renders correctly
- [ ] Presets load and apply
- [ ] Settings dialog works
- [ ] Toolbar buttons update correctly
- [ ] Axes toggle works
- [ ] No memory leaks

---

## Questions?

Check existing visualizations as examples or review the documentation files.

Thank you for contributing! 🎨✨
