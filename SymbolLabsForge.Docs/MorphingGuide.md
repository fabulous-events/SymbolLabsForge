# Symbol Morphing & Interpolation Guide

*   **Status**: ✅ Active
*   **AuditTag**: `Phase5.1`

## Overview
Phase 5 introduces advanced generative capabilities to the Forge. This guide explains how to use the CLI to create morphed symbols and interpolated style variants.

## 1. Symbol Morphing
Symbol morphing generates an intermediate symbol by blending two existing styles.

### CLI Command
```bash
# Generates a symbol that is 30% of the way between "Handwritten" and "Typeset"
forge morph --from Flat_Handwritten --to Flat_Typeset --factor 0.3
```

### Key Concepts
*   **Styles**: The `--from` and `--to` styles must correspond to existing snapshot `.png` files in the `TestAssets/Snapshots` directory.
*   **Interpolation Factor**: A value from `0.0` (purely the "from" style) to `1.0` (purely the "to" style).
*   **Output**: A new `SymbolCapsule` with `MorphLineage` and `InterpolationFactor` metadata.

## 2. Style Interpolation
Style interpolation generates a series of symbols by varying a single attribute (like stroke width or rotation) over a specified range.

### CLI Command
```bash
# Generates 5 Flat symbols, varying the rotation from -15 to +15 degrees
forge interpolate-style --symbol Flat --attribute Rotation --range -15 15 --steps 5
```

### Key Concepts
*   **Attribute**: The style parameter to vary (e.g., `Rotation`, `StrokeWidth`, `Density`).
*   **Range**: The starting and ending values for the interpolation.
*   **Steps**: The number of intermediate symbols to generate.
*   **Output**: A set of `SymbolCapsules`, each with `InterpolatedAttribute` and `StepIndex` metadata.
