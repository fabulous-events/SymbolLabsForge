# Training Data Augmentation Guide

*   **Status**: ✅ Active
*   **AuditTag**: `Phase5.3`

## Overview
The Forge can generate large, diverse datasets of synthetic symbols suitable for training machine learning models for optical music recognition (OMR) and other analysis tasks.

## CLI Command
The `augment-training` command is the primary tool for this workflow.

```bash
# Generates 100 variants of the Clef symbol and saves them to ./AugmentedData/Clef
forge augment-training --symbol Clef --variants 100 --output ./AugmentedData/Clef
```

## Augmentation Strategies
The command automatically applies a variety of strategies to create a diverse dataset:

*   **Dimension Variation**: Generates symbols at various sizes and aspect ratios.
*   **Style Interpolation**: Creates variants with different stroke widths, densities, and rotations.
*   **Symbol Morphing**: Blends between different foundational styles (e.g., handwritten and typeset).
*   **Edge Cases**: Intentionally generates symbols with common real-world defects like clipping, ink bleed, and noise.

## Output
Each generated symbol is a complete, validated `SymbolCapsule` containing:
*   The symbol image (`.png`).
*   The full metadata (`.json`) with provenance, including any morphing or interpolation factors.

This ensures that every piece of training data is fully traceable and reproducible. The generated capsules are also automatically added to the `CapsuleRegistry.json` for indexing.
