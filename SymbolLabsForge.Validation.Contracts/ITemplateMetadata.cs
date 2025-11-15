//===============================================================
// File: ITemplateMetadata.cs
// Author: Claude (Phase 8.3 - Modularization)
// Date: 2025-11-14
// Purpose: Constraint interface for metadata that supports template validation.
//
// PHASE 8.3: MODULARIZATION - VALIDATION FRAMEWORK
//   - Enables TemplateValidator<TMetadata, TMetrics> where TMetadata : ITemplateMetadata
//   - Decouples validators from specific metadata implementation (TemplateMetadata)
//   - Allows custom metadata classes in other projects
//
// DESIGN RATIONALE:
//   - Properties are read-only (validators check values, don't modify)
//   - ProvenanceMetadata is nullable (legacy capsules may lack provenance)
//   - TemplateHash must be populated for integrity verification
//
// IMPLEMENTATION NOTES:
//   - SymbolLabsForge.Contracts.TemplateMetadata implements this interface
//   - Custom metadata classes must implement this to use TemplateValidator
//
// AUDIENCE: Graduate / PhD (interface segregation, metadata governance)
//===============================================================
#nullable enable

using System;

namespace SymbolLabsForge.Validation.Contracts
{
    /// <summary>
    /// Interface for metadata that supports template validation.
    /// Implement this interface to enable use of TemplateValidator.
    /// </summary>
    public interface ITemplateMetadata
    {
        /// <summary>
        /// Human-readable name of the template (e.g., "treble-clef-standard").
        /// Must not be "unknown", "default", or empty (enforced by TemplateValidator).
        /// </summary>
        string TemplateName { get; }

        /// <summary>
        /// Generator version that created the template (e.g., "SymbolLabsForge v1.5.0").
        /// Used for traceability and debugging.
        /// </summary>
        string GeneratedBy { get; }

        /// <summary>
        /// SHA256 hash of the template image for integrity verification.
        /// Must not be "unhashed" or empty (enforced by TemplateValidator).
        /// </summary>
        string TemplateHash { get; }

        /// <summary>
        /// Provenance metadata (source image, preprocessing method, validation date).
        /// May be null for legacy capsules created before Phase 5.
        /// TemplateValidator checks for completeness if non-null.
        /// </summary>
        ProvenanceMetadata? Provenance { get; }
    }

    /// <summary>
    /// Structured provenance metadata for generated templates.
    /// All templates MUST have complete provenance for traceability.
    /// </summary>
    /// <remarks>
    /// PHASE 8.3: Moved from SymbolLabsForge.Contracts to Validation.Contracts.
    /// This is the canonical implementation (SymbolLabsForge.Contracts.ProvenanceMetadata is now a type alias).
    /// </remarks>
    public record ProvenanceMetadata
    {
        /// <summary>
        /// Original source image file path.
        /// REQUIRED for traceability back to input data.
        /// </summary>
        public required string SourceImage { get; init; }

        /// <summary>
        /// Preprocessing method applied to generate this template.
        /// REQUIRED to ensure reproducibility.
        /// </summary>
        public required PreprocessingMethod Method { get; init; }

        /// <summary>
        /// Date and time when template quality was validated.
        /// REQUIRED for audit compliance.
        /// </summary>
        public required DateTime ValidationDate { get; init; }

        /// <summary>
        /// Tool or generator version that validated this template.
        /// REQUIRED for reproducibility (e.g., "SymbolLabsForge v1.5.0").
        /// </summary>
        public required string ValidatedBy { get; init; }

        /// <summary>
        /// Optional notes about custom preprocessing or special handling.
        /// Use this for PreprocessingMethod.Custom to document the process.
        /// </summary>
        public string? Notes { get; init; }
    }

    /// <summary>
    /// Standard preprocessing methods applied to templates.
    /// Used for provenance tracking and reproducibility.
    /// </summary>
    /// <remarks>
    /// PHASE 8.3: Moved from SymbolLabsForge.Contracts to Validation.Contracts.
    /// This is the canonical implementation (SymbolLabsForge.Contracts.PreprocessingMethod is now a type alias).
    /// </remarks>
    public enum PreprocessingMethod
    {
        /// <summary>
        /// No preprocessing applied - raw pixel data.
        /// </summary>
        Raw = 0,

        /// <summary>
        /// Adaptive binarization applied (threshold-based).
        /// Converts grayscale to binary (0 = ink, 255 = background).
        /// </summary>
        Binarized = 1,

        /// <summary>
        /// Zhang-Suen thinning algorithm applied (skeletonization).
        /// Reduces thick strokes to single-pixel-wide skeletons.
        /// </summary>
        Skeletonized = 2,

        /// <summary>
        /// Custom preprocessing pipeline (not a standard method).
        /// Use ProvenanceMetadata.Notes to document the custom process.
        /// </summary>
        Custom = 99
    }
}
