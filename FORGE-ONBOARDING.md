# SymbolLabsForge - Contributor Onboarding

Welcome to the Forge! This document provides everything you need to know to contribute to the synthetic symbol generation engine.

## 1. Capsule Anatomy

A "Capsule" is the core artifact of the Forge. It's a self-contained, validated package containing a symbol image and its metadata.

*   **`SymbolCapsuleSet`**: The top-level object returned by the Forge. It contains the primary symbol and all its variants (different sizes, forms, etc.).
*   **`SymbolCapsule`**: A single artifact containing:
    *   `TemplateImage`: The `Image<L8>` of the symbol.
    *   `TemplateMetadata`: The ".json" part. Contains ID, hash, and provenance.
    *   `QualityMetrics`: Density, aspect ratio, etc.
    *   `ValidationResults`: A list of pass/fail results from all validators.

## 2. The Generation Pipeline

1.  **Request**: A `SymbolRequest` is created, specifying the symbol type, sizes, and desired output forms.
2.  **Generation**: The corresponding `ISymbolGenerator` (e.g., `FlatGenerator`) creates the raw, anti-aliased image.
3.  **Processing**: The raw image is processed into different forms (`Binarized`, `Skeletonized`).
4.  **Validation**: Each generated capsule is run through all registered `IValidator` services.
5.  **Packaging**: The final `SymbolCapsuleSet` is assembled and returned.

## 3. Replay Bundle Handoff

The primary consumer of capsules is the **SymbolLabs** project, which assembles them into replay bundles for testing the detection engine.

*   **Indexing**: Every capsule has a `CapsuleId` and `TemplateHash`. These are logged in the `CapsuleRegistry.json`.
*   **Consumption**: The SymbolLabs runner calls `ISymbolForge.Generate()`, receives the capsule set, and saves the images and `.json` files to the `GoldenMaster` directory.
*   **Registry Update**: The runner is responsible for updating the `CapsuleRegistry.json` with the new capsule hashes.
