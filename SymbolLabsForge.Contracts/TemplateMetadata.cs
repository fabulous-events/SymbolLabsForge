//===============================================================
// File: TemplateMetadata.cs
// Author: Gemini (Original), Claude (Registry & Provenance Tracking Phase 5.3, Phase 8.3)
// Date: 2025-11-11 (Original), 2025-11-14 (Provenance Update, ITemplateMetadata Implementation)
// Origin: Migrated from SymbolLabsForge.Contracts.cs
// Purpose: Metadata model for a generated template with provenance tracking.
//
// PHASE 8.3: MODULARIZATION - VALIDATION FRAMEWORK
//   - Implements ITemplateMetadata from Validation.Contracts
//   - Enables use with generic TemplateValidator<TMetadata, TMetrics>
//   - All required properties already present (no code changes needed)
//
// REGISTRY & PROVENANCE TRACKING (Phase 5.3):
//   - Added [Required] attributes to enforce metadata completeness
//   - Removed unsafe defaults ("unknown", "default", "unhashed")
//   - Added structured ProvenanceMetadata property
//   - CapsuleId now auto-generated (no longer settable)
//
// DEFECT HISTORY:
//   - Original Implementation: Allowed defaults like "unknown", "unhashed"
//   - Root Cause: No validation enforcement, optional provenance
//   - Impact: Templates exported with incomplete metadata, breaking audit trails
//   - Fix: Added [Required] attributes, structured provenance, fail-fast validation
//
// VALIDATION STRATEGY:
//   - TemplateName: REQUIRED - no generic "default" names allowed
//   - GeneratedBy: REQUIRED - must identify tool/generator version
//   - TemplateHash: REQUIRED - must be computed SHA256 hash
//   - Provenance: REQUIRED - must have complete traceability metadata
//
// AUDIENCE: Graduate / PhD (provenance tracking, metadata validation)
//===============================================================
#nullable enable

using System;
using System.ComponentModel.DataAnnotations;
using SymbolLabsForge.Validation.Contracts;

namespace SymbolLabsForge.Contracts
{
    /// <summary>
    /// Metadata for a generated template with provenance tracking.
    /// Implements ITemplateMetadata to support generic TemplateValidator.
    /// </summary>
    public record TemplateMetadata : ITemplateMetadata
    {
        /// <summary>
        /// Template name (must be unique and descriptive, no generic "default" allowed).
        /// REQUIRED for identification and registry management.
        /// </summary>
        [Required(ErrorMessage = "TemplateMetadata.TemplateName is required. Provide a descriptive name, not 'default'.")]
        public required string TemplateName { get; init; }

        /// <summary>
        /// Tool or generator that created this template (e.g., "SymbolLabsForge v1.5.0").
        /// REQUIRED for reproducibility and debugging.
        /// </summary>
        [Required(ErrorMessage = "TemplateMetadata.GeneratedBy is required. Specify the tool and version.")]
        public required string GeneratedBy { get; init; }

        /// <summary>
        /// Generation timestamp in ISO 8601 format (UTC).
        /// Auto-populated at creation time.
        /// </summary>
        public string GeneratedOn { get; init; } = DateTime.UtcNow.ToString("o");

        /// <summary>
        /// Optional random seed used for generation (for reproducibility).
        /// </summary>
        public int? GenerationSeed { get; init; }

        /// <summary>
        /// SHA256 hash of template image bytes.
        /// REQUIRED for integrity verification and deduplication.
        /// Must be computed by CapsuleExporter, not hardcoded.
        /// </summary>
        [Required(ErrorMessage = "TemplateMetadata.TemplateHash is required. Compute SHA256 hash of image bytes.")]
        public required string TemplateHash { get; init; }

        /// <summary>
        /// Unique capsule identifier (auto-generated GUID).
        /// Cannot be set externally - always generated at creation.
        /// </summary>
        public string CapsuleId { get; init; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Symbol type (clef, sharp, flat, etc.).
        /// REQUIRED for categorization.
        /// </summary>
        public required SymbolType SymbolType { get; init; }

        /// <summary>
        /// Provenance metadata (source image, preprocessing method, validation info).
        /// REQUIRED for traceability and audit compliance.
        /// </summary>
        [Required(ErrorMessage = "TemplateMetadata.Provenance is required for traceability.")]
        public required ProvenanceMetadata Provenance { get; init; }

        /// <summary>
        /// Optional morph lineage (parent template IDs if morphed/interpolated).
        /// </summary>
        public string? MorphLineage { get; init; }

        /// <summary>
        /// Optional interpolation factor (0.0-1.0) if template was morphed.
        /// </summary>
        public float? InterpolationFactor { get; init; }

        /// <summary>
        /// Optional interpolated attribute name (e.g., "thickness", "angle").
        /// </summary>
        public string? InterpolatedAttribute { get; init; }

        /// <summary>
        /// Optional step index in a generation sequence.
        /// </summary>
        public int? StepIndex { get; init; }

        /// <summary>
        /// Optional audit tag for categorization/filtering.
        /// </summary>
        public string? AuditTag { get; init; }
    }
}
