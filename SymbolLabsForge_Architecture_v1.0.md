# SymbolLabsForge Architecture Proposal

## Version 1.0 — Finalized 2025-11-10
- Reviewed by Michael
- Forensic enhancements merged
- Naming conventions updated to SymbolLabsForge

---

## 🎯 Executive Summary

This proposal recommends creating a dedicated synthetic symbol generation project — a standalone, governance-grade rendering and validation engine called **SymbolLabsForge** (“The Forge”). Its purpose is to generate, preprocess, validate, and package synthetic musical symbols for consumption by the main SymbolLabs detection pipeline.

The Forge resolves the root cause of recent validation failures (e.g., sharp_synthetic_template.png, flat_synthetic_template.png): the lack of a clear boundary and post-processing pipeline for synthetic templates. It introduces a multi-form generation capability and automatic JSON "Capsule" creation, enabling SymbolLabs to consume fully validated, audit-ready symbol artifacts.

---

## ✅ Strategic Advantages

1.  **Isolation of Concerns**
    *   SymbolLabs focuses on detection, replay bundles, and governance.
    *   The Forge specializes in rendering, binarization, skeletonization, and edge-case handling.

2.  **Bulletproof API Contract**
    *   The Forge exposes a clean interface that returns a fully validated `SymbolCapsuleSet`, containing all necessary forms of the symbol.
    *   SymbolLabs consumes a guaranteed-good set of representations — ideal for detection, training, and replay bundle fallback.

3.  **Embedded Governance**
    *   Validation (density, skeletonization, contamination, clipping) is a non-skippable internal step. Failed templates are either rejected or flagged with `IsValid = False` and include a `ValidationResult` report.

4.  **JSON Capsule Generation**
    *   Each `SymbolCapsule` includes a `TemplateMetadata` object that is automatically serialized to JSON and saved alongside the image (e.g., `flat_synthetic_template.png` + `flat_synthetic_template.json`).
    *   This ensures every image has a matching, audit-grade metadata capsule.

5.  **Independent Release Cycle**
    *   The Forge can be versioned and released independently (e.g., via NuGet). This allows rendering logic to evolve without destabilizing SymbolLabs.

6.  **Teaching and Onboarding Value**
    *   The Forge becomes a sandbox for contributors to learn rendering, preprocessing, and validation — without touching core detection logic.

7.  **Multi-Form Output for Replay Bundles**
    *   The Forge supports the generation of all necessary template forms:
        *   Raw (e.g., anti-aliased, for testing binarization)
        *   Binarized (crisp, 1-bit, non-skeletonized)
        *   Skeletonized (1-pixel-thin, the primary detection template)
    *   This enables SymbolLabs to build robust replay bundles and fallback logic from a single source of truth.

---

## 📐 Core Architectural Principles

*   **Graphics Library Mandate:** All image generation, processing, and manipulation within the `SymbolLabsForge` library MUST use `SixLabors.ImageSharp`.
*   **UI Library Distinction:** The `System.Drawing` library MUST only be used for WinForms UI elements (e.g., Button, Form, PictureBox). It must NOT be used for generating symbol graphics.
*   **Dependency Injection (DI):** The `SymbolLabsForge` library MUST be built using a DI-first approach, utilizing `Microsoft.Extensions.DependencyInjection`.
*   **Service Discovery:** To enforce DI and simplify consumption, the library MUST provide an `IServiceCollection` extension method (e.g., `AddSymbolForge()`). This method MUST use assembly scanning (via a lightweight library like Scrutor) to automatically discover and register all `ISymbolGenerator` and `IValidator` implementations. This makes the Forge future-proof: adding a `DoubleSharpGenerator` requires no changes to the consuming applications.

---

## 📝 API Contract (C#)

Per the new `CSharpStandards.docx`, the API contract is now defined in C#. This defines the robust contract between SymbolLabs and SymbolLabsForge and complies with the new standards (nullable-aware, explicit types, record DTOs).

```csharp
//===============================================================
// File: SymbolLabsForge.Contracts.cs
// Purpose: Defines the public API for the Synthetic Forge.
// Standard: C# 2025.09
// Version: 1.7.0 (incorporates governance review feedback)
//===============================================================
#nullable enable

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;

namespace SymbolLabsForge.Contracts
{
    // --- Core Service Contract ---

    public interface ISymbolForge
    {
        SymbolCapsuleSet Generate(SymbolRequest request);
    }

    // --- Enums for Type Safety ---

    public enum SymbolType { Flat, Sharp, Natural }
    public enum EdgeCaseType { Clipped, Rotated, InkBleed }
    public enum OutputForm { Raw, Binarized, Skeletonized }
    public enum DensityStatus { Unknown, Valid, TooHigh, TooLow }

    // --- DTOs (Data Transfer Objects) ---

    public record SymbolRequest(
        SymbolType Type,
        List<Size> Dimensions, // Supports multi-scale generation
        List<OutputForm> OutputForms, // More extensible than a bool
        int? GenerationSeed = null, // For reproducibility
        List<EdgeCaseType>? EdgeCasesToGenerate = null);

    public record SymbolCapsuleSet(
        SymbolCapsule Primary, // The main generated symbol at the first requested size
        List<SymbolCapsule> Variants); // Includes other sizes and forms

    public record SymbolCapsule(
        Image<L8> TemplateImage,
        TemplateMetadata Metadata,
        QualityMetrics Metrics,
        bool IsValid,
        List<ValidationResult> ValidationResults);

    public record ValidationResult(
        bool IsValid,
        string ValidatorName,
        string? FailureMessage = null);

    // --- DI Service Interfaces ---

    public interface ISymbolGenerator
    {
        SymbolType SupportedType { get; }
        Image<L8> GenerateRawImage(Size dimensions, int? seed);
    }

    public interface IValidator
    {
        string Name { get; }
        ValidationResult Validate(SymbolCapsule capsule, QualityMetrics metrics);
    }

    // --- Metadata Models for Serialization ---

    public class TemplateMetadata
    {
        public string CapsuleId { get; set; } = string.Empty; // Deterministic ID
        public string TemplateHash { get; set; } = string.Empty; // SHA256 of image
        public string TemplateName { get; set; } = string.Empty;
        // TemplateType removed as it's redundant with SymbolType in the request
        public string GeneratedBy { get; set; } = string.Empty;
        public string GeneratedOn { get; set; } = string.Empty; // UTC Timestamp
        public int? GenerationSeed { get; set; }
        public string? ValidationReport { get; set; }
    }

    public class QualityMetrics
    {
        public double AspectRatio { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public double Density { get; set; }
        public DensityStatus DensityStatus { get; set; } = DensityStatus.Unknown;
    }
}
```

---

## ⚠️ Risk Mitigation Plan

| Risk                   | Mitigation                                                              |
| :--------------------- | :---------------------------------------------------------------------- |
| **Initial Overhead**   | Phased rollout: scaffold first, validate later.                         |
| **Version Drift**      | Strict Semantic Versioning (e.g., `SymbolLabsForge v1.7.x`).            |
| **Integration Complexity** | DLL/API handoff is encapsulated in the `SymbolCapsuleSet` object. DI setup is handled by `AddSymbolForge()`. |
| **Governance Duplication** | Forge emits micro-reports/JSON; SymbolLabs aggregates into `TEMPLATE-QUALITY-REPORT.md`. |

---

## 🧭 Phased Rollout Plan

**🔹 Phase 1: Scaffolding & Porting (1–2 Days)**
1.  **Scaffold** `SymbolLabsForge.sln` solution.
2.  **Create Projects:**
    *   `SymbolLabsForge/` (main class library)
    *   `SymbolLabsForge.Contracts/` (holds the C# API definitions)
    *   `SymbolLabsForge.Tests/` (unit tests)
    *   `SymbolLabsForge.Tool/` (WinForms UI)
    *   `SymbolLabsForgeValidator/` (CLI validator)
    *   `SymbolLabsForge.Docs/` (Governance artifacts)
3.  **Define API:**
    *   Implement all interfaces, records, and classes from the C# contract above, including `ISymbolGenerator` and `IValidator`.
4.  **Add DI Packages:**
    *   Add `Microsoft.Extensions.DependencyInjection` and `Microsoft.Extensions.DependencyInjection.Abstractions` to `SymbolLabsForge` and `SymbolLabsForge.Tool`.
    *   Add `Scrutor` to `SymbolLabsForge`.
5.  **Implement DI Registration:**
    *   Create and implement `ServiceCollectionExtensions.cs`.
6.  **Define Schemas:**
    *   Create `TemplateMetadata` and `QualityMetrics` classes.
    *   Add `Newtonsoft.Json` dependency for serialization.
7.  **Port Drawing Logic:**
    *   Move drawing logic from `GenerateSyntheticAccidentalsRunner.cs` into new `SymbolLabsForge/Generators/` classes (e.g., `FlatGenerator.cs`, `SharpGenerator.cs`).
    *   Each generator must implement `ISymbolGenerator`.
    *   Crucially: Fix generators to produce Raw (anti-aliased), Binarized (1-bit aliased), and handle padding/clipping issues, all using `SixLabors.ImageSharp`.
8.  **Integrate:**
    *   Add `SymbolLabsForge` as a local project reference to SymbolLabs.
    *   SymbolLabs will be responsible for setting up the DI container and registering the services.

**🔹 Phase 2: Internal Validation & Skeletonization (2–3 Days)**
1.  **Implement Skeletonization:**
    *   Add a `Preprocessing/SkeletonizationProcessor.cs`.
    *   This can be an injected service (`IPreprocessingStep`) or a static utility.
    *   Add unit tests to validate the skeletonization output against a known input.
2.  **Embed Validation:**
    *   Add `Validation/` services (e.g., `DensityValidator.cs`, `ClippingValidator.cs`).
    *   Each validator must implement `IValidator`.
    *   The `ISymbolForge` implementation will receive an `IEnumerable<IValidator>` from DI, run all of them, and aggregate the `ValidationResult` objects into the `SymbolCapsule`.
3.  **Implement Basic Registry:**
    *   Create a simple `CapsuleRegistry.json` file.
    *   The SymbolLabs runner, after saving a capsule, will add an entry (e.g., `"flat-12x30-skeleton": "hash-value"`) to this file.
    *   Add a pre-generation check to warn if a capsule with matching parameters already exists.
4.  **Generate JSON Capsule:**
    *   The SymbolLabs runner, upon receiving the `SymbolCapsuleSet`, now serializes the Metadata, Metrics, and ValidationResults from each capsule into the corresponding `.json` file.
5.  **Refactor:**
    *   The SymbolLabs runner is now just a consumer. It configures the DI container (via `AddSymbolForge()`), resolves `ISymbolForge`, calls `Generate()`, and saves the results.

**🔹 Phase 3: Formalize as a Dependency (1 Day)**
1.  **Package:** Configure `SymbolLabsForge` to build as a NuGet package.
2.  **Publish:** Push to GitHub Packages or a local feed.
3.  **Update:** SymbolLabs replaces the local project reference with the new NuGet dependency.

**🔹 Phase 4: Build Forge UI Tool (1-2 Days)**
1.  **Implement** the `SymbolLabsForge.Tool` WinForms application.
2.  **Adhere to Principle:** Use `System.Drawing` only for Form controls (buttons, listboxes, etc.).
3.  **Implement PictureBox Bridge:**
    *   Create a UI helper class with the recommended `SetImage(this PictureBox, Image)` extension method, using `image.SaveAsBmp(ms)` and `new Bitmap(ms)` for safe, non-corrupted display.
4.  **Setup DI & Config in UI:**
    *   In the WinForms `Program.cs`, build a `ServiceProvider`.
    *   Use `Microsoft.Extensions.Configuration` to load `appsettings.json`.
    *   Call `services.AddSymbolForge()` to register all components.
    *   The main form will get `ISymbolForge` and `IConfiguration` from the service provider.
5.  **Implement UI Controls:**
    *   A `CheckedListBox` or `ComboBox` to select `SymbolType` (or multiple types) to generate.
    *   A "Generate" button to call the `ISymbolForge.Generate()` method.
    *   A "Reset to Standards" button that loads default settings from the injected `IConfiguration` (`appsettings.json`).
    *   A "Browse..." button (using `FolderBrowserDialog`) to set the output directory.
    *   A `TextBox` (multi-line, read-only) or `ListView` to display `ValidationResult.FailureMessage` logs from the generation process.

---

## 🛡️ Governance & Validation Enhancements

**4. Capsule Provenance & Audit Trail**
*   **Why:** For compliance and reproducibility, every capsule should carry its origin story.
*   **Enhancement:** Add to `TemplateMetadata` (as seen in C# contract):
    *   `GeneratedBy`: e.g., `SymbolLabsForge v1.7.0`
    *   `GeneratedOn`: UTC timestamp
    *   `GenerationSeed`: optional RNG seed for reproducibility
    *   `ValidationReport`: embedded or linked `.md` snippet

**5. Capsule Validator CLI**
*   **Why:** Enables batch validation of capsules outside the Forge (e.g., CI/CD, replay bundle audits).
*   **Enhancement:** Add `SymbolLabsForgeValidator` CLI project:
    *   `dotnet run -- validate-capsule flat_synthetic_template.json`
    *   **Outputs:**
        *   Pass/fail
        *   Density, contamination, clipping, hash match
        *   Optional `--fix` flag to reprocess and regenerate

**6. Capsule Registry & Indexing (Elevated Priority)**
*   **Why:** As the Forge grows, you’ll need a searchable index of all generated capsules. Foundational for duplicate detection.
*   **Enhancement:**
    *   Add `CapsuleRegistry.json` or SQLite DB (starting with JSON).
    *   Index by `SymbolType`, `Size`, `Form`, `Hash`, `IsValid`.
    *   **Ensembles:**
        *   Duplicate detection (part of Phase 2)
        *   Replay bundle assembly
        *   Contributor search tools

---

## 🔭 Future-Proofing Enhancements

**7. Symbol Morphing & Interpolation**
*   **Why:** For training data augmentation or style transfer (e.g., between handwritten and typeset styles).
*   **Enhancement:**
    *   Add `MorphRequest`: `SymbolType`, `FromStyle`, `ToStyle`, `InterpolationFactor`
    *   Output: intermediate `SymbolCapsule` with metadata noting morph lineage

**8. Capsule Registry & Indexing**
*   **Why:** As the Forge grows, you’ll need a searchable index of all generated capsules.
*   **Enhancement:**
    *   Add `CapsuleRegistry.json` or SQLite DB:
    *   Index by `SymbolType`, `Size`, `Form`, `Hash`, `IsValid`
    *   Enables:
        *   Duplicate detection
        *   Replay bundle assembly
        *   Contributor search tools

---

## 🧠 Legacy-Grade Impact

This proposal transforms symbol generation from a fragile, embedded subsystem into a robust, reusable, and auditable service. It aligns with SymbolLabs’ governance principles and enables future expansion:

*   Multi-scale templates
*   Dynamic replay bundles
*   Symbol morphing and variant generation
*   Capsule-based teaching artifacts
