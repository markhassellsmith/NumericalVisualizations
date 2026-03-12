# Usage Examples

This document provides practical examples of using the Numerical Visualizations application.

---

## Table of Contents
1. [Basic Usage](#basic-usage)
2. [Using the Toolbar](#using-the-toolbar)
3. [Working with Presets](#working-with-presets)
4. [Settings Dialog](#settings-dialog)
5. [Programmatic Usage](#programmatic-usage)

---

## Basic Usage

### Selecting a Visualization

**Via Keyboard Shortcuts:**
```
Ctrl+1  → Newton's Method
Ctrl+2  → Mandelbrot Set
Ctrl+3  → Hailstone Sequence
```

**Via Menu:**
1. Click **Visualizations** in the menu bar
2. Click visualization name to use defaults
3. Hover over name to see presets dropdown

### First Time Launch

When you launch the app, you'll see:
```
┌────────────────────────────────────────┐
│                                        │
│  Select a visualization from the       │
│  Visualizations menu or press          │
│  Ctrl+1, Ctrl+2, or Ctrl+3            │
│                                        │
└────────────────────────────────────────┘
```

Just press Ctrl+1 to see Newton's Method with default settings!

---

## Using the Toolbar

The toolbar provides instant display option toggling:

### Example 1: Toggle Axes on Newton's Method

1. Press `Ctrl+1` (Newton's Method loads)
2. Click **[Axes]** button in toolbar
3. Axes appear instantly (~50ms) ⚡
4. Click again to remove axes

**Result:** Coordinate reference overlay without recomputing the fractal!

### Example 2: Customize Hailstone Display

1. Press `Ctrl+3` (Hailstone Sequence)
2. Toolbar shows: `[Axes] | [Point Labels] [Dots]`
3. Click **[Point Labels]** to hide (N, X, Y) labels
4. Click **[Dots]** to hide endpoint markers
5. Result: Just the colored path line

**Button States:**
- **Blue background** = Enabled
- **Gray background** = Disabled
- **Grayed out text** = Not applicable (e.g., Point Labels on Newton)

---

## Working with Presets

### Example 1: Quick Exploration of Newton's Method

**Goal:** See different color schemes and detail levels

```
1. Click Visualizations → Newton's Method (hover)
2. Click "Default" → See balanced view
3. Click Visualizations → Newton's Method → "Vibrant Colors"
   → See bold color scheme
4. Click Visualizations → Newton's Method → "High Detail"
   → See 5000 iterations with ultra-precision
```

### Example 2: Analyzing Hailstone Behavior

**Goal:** Study sequence at different time scales

```
1. Select Visualizations → Hailstone Sequence → "First 50 Steps - Early Behavior"
   → Zoomed view (ScaleFactor=0.08) of first 50 steps
   → Shows initial dynamics clearly

2. Switch to "300 Steps - Long Term Behavior"
   → Wider view (ScaleFactor=0.03) showing 300 steps
   → Point labels disabled (too crowded)
   → See where the sequence wanders over time

3. Compare to "Default - Balanced View"
   → Middle ground: 150 steps at 0.05 scale
   → All same starting point (-0.5, 0.3)!
```

**Key Insight:** Presets change analytical perspective (iterations, zoom), not just aesthetics.

---

## Settings Dialog

### Example 1: Fine-Tune Newton's Method

1. Select Newton's Method
2. Hover over **Newton's Method** → Click **Settings...**
3. PropertyGrid opens showing categories
4. Change `MaxIterations` to 2000
5. Click **Apply** → See updated rendering (dialog stays open!)
6. Change `HueSpread` to 30
7. Click **Apply** → See new color scheme
8. Click **Close** when satisfied

### Example 2: Detailed Hailstone Configuration

1. Select Hailstone Sequence
2. Go to Settings...
3. Adjust MaxIterations to 200
4. Adjust ScaleFactor to 0.06
5. Click **Apply** after each change to see effect
6. Use toolbar to quickly toggle ShowAxes without opening settings again!

---

## Programmatic Usage

### Example 1: Creating a Custom Configuration

```csharp
using NumericalVisualizations.Visualizations;

// Create custom Newton configuration
var config = new NewtonConfig
{
    MaxIterations = 3000,
    Tolerance = 1e-12,
    HueSpread = 25,
    ShowAxes = true
};

// Create visualization with custom config
var viz = new NewtonVisualization(config);

// Render to bitmap
var bitmap = viz.Render(1920, 1080, 4.0, 4.0);
bitmap.Save("custom_newton.png");
```

### Example 2: Adding Axes Overlay Programmatically

```csharp
using NumericalVisualizations.Performance;

// Render base visualization
var viz = new NewtonVisualization();
var bitmap = viz.Render(1920, 1080, 4.0, 4.0);

// Add axes overlay (fast!)
RenderingHelpers.DrawAxesOnBitmap(bitmap, 4.0, 4.0);

bitmap.Save("newton_with_axes.png");
```

---

## Tips & Tricks

### Performance
- **Axes Toggle**: Use toolbar for instant axes on/off (Newton/Mandelbrot)
- **Fast Preview Presets**: Use "Fast Preview" presets for quick exploration
- **Settings Apply**: Click Apply multiple times to iterate on parameters

### Visual Clarity
- **Hailstone Labels**: Toggle Point Labels off when viewing 300 steps (too crowded)
- **Axes**: Enable axes to understand coordinate space
- **Color Schemes**: Try different HueSpread or ColorMultiplier values

---

## See Also

- [README.md](README.md) - Project overview and architecture
- [CONTRIBUTING.md](CONTRIBUTING.md) - How to add new visualizations/presets
- [REFACTORING_SUMMARY.md](REFACTORING_SUMMARY.md) - Technical implementation details
