# SymbolLabsForge - Initial Tasks

This document outlines the initial tasks for the SymbolLabsForge project, based on the architectural proposal.

## Phase 1: Scaffolding & Porting (Complete)

## Phase 2: Internal Validation & Skeletonization (In Progress)

### High Priority

*   **Implement ISymbolForge:** Complete the `SymbolForge.cs` implementation to orchestrate generation, processing, and validation.
    *   **Owner:** AI programmer
*   **Port Flat/Sharp Generators:** Move existing drawing logic into `FlatGenerator.cs` and `SharpGenerator.cs` (implementing `ISymbolGenerator`). Ensure they produce Raw, Binarized, and handle padding/clipping.
    *   **Owner:** Contributor
*   **Add Skeletonization Processor:** Implement `Preprocessing/SkeletonizationProcessor.cs` (or similar) to apply Zhang-Suen skeletonization.
    *   **Owner:** AI researcher
*   **Implement Core Validators:** Create `DensityValidator.cs` and `ClippingValidator.cs` (implementing `IValidator`).
    *   **Owner:** Contributor

### Medium Priority

*   **Build JSON Capsule Serializer:** Implement logic to serialize `TemplateMetadata`, `QualityMetrics`, and `ValidationResults` to `.json` files.
    *   **Owner:** Contributor
*   **Create CapsuleRegistry.json:** Implement the basic registry for tracking generated capsules.
    *   **Owner:** Contributor

## Phase 3: Formalize as a Dependency

### Medium Priority

*   **Configure NuGet Packaging:** Set up `SymbolLabsForge` to build as a NuGet package.
    *   **Owner:** Contributor

## Phase 4: Build Forge UI Tool

### Low Priority

*   **Scaffold UI Tool:** Implement the basic WinForms application structure for `SymbolLabsForge.Tool`.
    *   **Owner:** Contributor

## Governance & Documentation

### High Priority

*   **Review and Refine Documentation:** Ensure `FORGE-ONBOARDING.md`, `FORGE-VERSIONS.md`, `ValidationFailureLog.md`, and `FORGE-ROADMAP.md` are up-to-date and comprehensive.
    *   **Owner:** Michael
