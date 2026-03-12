# Numerical Visualizations

A .NET 10 WinForms application for visualizing various numerical and mathematical concepts through beautiful, interactive graphics with a flexible, user-friendly interface.

## Features

### 🎨 **Interactive Visualization System**
- Three mathematical visualizations: Newton's Method, Mandelbrot Set, and Hailstone Sequence
- Live toolbar with toggle buttons for display options
- Preset system with 3-5 curated configurations per visualization
- Comprehensive settings dialog with Apply/Close workflow

### 💾 **Export to Multiple Formats**
- **PNG** - Lossless with full metadata (recommended for quality)
- **JPEG** - Compressed with basic metadata (good for sharing)
- **BMP** - Uncompressed lossless (archival)
- **SVG** - Vector format with XML metadata (Hailstone only, infinitely scalable!)
- Automatic timestamped filenames
- Embedded reproducibility metadata in all formats

### ⚡ **Performance Optimizations**
- Parallel rendering using LockBits for 20-100x speedup
- Cached axes overlay for instant toggling on fractals (~50ms vs 1-2 seconds)
- Efficient bitmap handling with proper resource disposal

### 🎯 **User Interface**
- **Keyboard Shortcuts**: Ctrl+1 (Newton), Ctrl+2 (Mandelbrot), Ctrl+3 (Hailstone)
- **Toolbar Toggles**: Instantly enable/disable axes, point labels, and dots
- **Settings Dialog**: Resizable PropertyGrid with live Apply button
- **Preset System**: Quick access to analytical views (different iterations/scales)
- **Export Menu**: File → Export with format-specific options

---

## Quick Start

### Basic Usage

1. **Launch the application** - You'll see a welcome message
2. **Select a visualization**:
   - Press `Ctrl+1` for Newton's Method
   - Press `Ctrl+2` for Mandelbrot Set
   - Press `Ctrl+3` for Hailstone Sequence
   - Or use the Visualizations menu

3. **Toggle display options** using the toolbar:
   - **[Axes]** - Show/hide coordinate axes with tick marks (all visualizations)
   - **[Point Labels]** - Show (N, X, Y) labels at each point (Hailstone only)
   - **[Dots]** - Show dots at segment endpoints (Hailstone only)

4. **Try presets**: Hover over a visualization name in the menu to see presets
5. **Adjust settings**: Click Settings... at the bottom of any visualization's submenu
6. **Export your work**: File → Export → Choose format (PNG, JPEG, BMP, or SVG)

### Export Guide

All exported images include comprehensive metadata for reproducibility:

```
File → Export →
  ├─ PNG    - Lossless, full metadata (best quality-to-size ratio)
  ├─ JPEG   - Compressed, basic metadata (smallest files)
  ├─ BMP    - Uncompressed, no metadata (simple archival)
  └─ SVG    - Vector, XML metadata (Hailstone only, infinite zoom!)
```

**Viewing Metadata:**
- **PNG/JPEG**: Right-click file → Properties → Details tab
- **SVG**: Open in Notepad (it's XML!) or any text editor
- **Command-line**: Use included `View-ImageMetadata.ps1` PowerShell script

### Toolbar Guide

The toolbar provides instant access to display options:

```
[Axes] | [Point Labels] [Dots]
  ↑           ↑           ↑
Universal   Hailstone-specific
```

- **Blue background** = Option enabled
- **Gray background** = Option disabled
- **Grayed out** = Not applicable to current visualization

---

## Architecture

### Core Components

#### 1. **Visualization Interface** (`IVisualization`)
Defines the contract for all visualizations:
- `Name` and `Description` - Metadata
- `Render(width, height, xRange, yRange)` - Generate bitmap
- `GetConfig()` - Retrieve current configuration
- `WithConfig(config)` - Create instance with updated configuration

#### 2. **Configuration System**
**Base Class**: `VisualizationConfig`
- Universal options: `ShowAxes`, `MaxIterations`, `Tolerance`

**Derived Classes**:
- `NewtonConfig` - Newton-specific parameters (HueSpread)
- `MandelbrotConfig` - Mandelbrot-specific (EscapeRadius, color mapping)
- `HailstoneConfig` - Hailstone-specific (ShowPointLabels, ShowDots, ScaleFactor)

#### 3. **Preset System** (`VisualizationPresets`)
Pre-configured settings for quick access:
- Focus on analytical perspectives (iterations, scale, zoom)
- Display options controlled via toolbar
- All use same starting point for comparison (Hailstone)

#### 4. **Factory Pattern** (`VisualizationFactory`)
Creates visualization instances with default configurations.

---

## Current Visualizations

### Newton's Method
Visualizes Newton's root-finding algorithm in the complex plane. Shows basins of attraction for different roots.

**Presets:**
- Default, High Detail, Fast Preview, Vibrant Colors, Subtle Bands

**Key Parameters:**
- `MaxIterations` (default: 1200) - Computation depth
- `Tolerance` (default: 1e-10) - Convergence threshold
- `HueSpread` (default: 17) - Color variation per iteration

### Mandelbrot Set
Classic fractal showing the Mandelbrot set boundary.

**Presets:**
- Classic, Deep Zoom, Psychedelic, Smooth Gradient, Fast Preview

**Key Parameters:**
- `MaxIterations` (default: 512)
- `EscapeRadius` (default: 1000000.0)
- Color mapping: Offset, Multiplier, Modulo

### Hailstone Sequence (2D Collatz)
2D visualization of Collatz-inspired dynamics with coupled integer maps.

**Presets:**
- Default, First 50 Steps (zoomed), 300 Steps (long-term behavior)

**Key Parameters:**
- `MaxIterations` (default: 150) - Steps to follow
- `StartX, StartY` (default: -0.5, 0.3) - Starting coordinates
- `ScaleFactor` (default: 0.05) - Movement magnification
- `ShowPointLabels` - Display (N, X, Y) at each point
- `ShowDots` - Show colored dots at segment endpoints

---

## Performance Notes

### Rendering Speed
- **Newton/Mandelbrot**: 1-2 seconds (parallel pixel computation)
- **Hailstone**: ~100-200ms (vector path rendering)

### Axes Toggling
- **Newton/Mandelbrot**: ~50ms (cached overlay)
- **Hailstone**: ~100-200ms (integrated rendering, full re-render)

### Optimization Strategies
1. **Parallel rendering** - LockBits + Parallel.For across rows
2. **Cached overlays** - Fractals cache base image, overlay axes separately
3. **Integrated rendering** - Hailstone renders axes during path drawing (transform matrix)

---

## Adding New Visualizations

See [CONTRIBUTING.md](CONTRIBUTING.md) for detailed guidelines on extending the system.

---

## Examples

See [EXAMPLES.md](EXAMPLES.md) for code examples and usage patterns.

---

## License

This project is provided as-is for educational and research purposes.
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
