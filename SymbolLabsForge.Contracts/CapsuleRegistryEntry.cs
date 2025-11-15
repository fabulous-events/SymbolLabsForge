//===============================================================
// File: CapsuleRegistryEntry.cs
// Author: Gemini (Original), Claude (Registry & Provenance Tracking Phase 5.3)
// Date: 2025-11-14
// Purpose: Registry entry for a capsule with structured provenance tracking.
//
// REGISTRY & PROVENANCE TRACKING (Phase 5.3):
//   - Replaced nullable string provenance with structured ProvenanceMetadata
//   - Enforces provenance completeness for all registry entries
//   - Critical for audit compliance and traceability
//
// DEFECT HISTORY:
//   - Original Implementation: Provenance was optional nullable string
//   - Root Cause: No structured provenance, no enforcement
//   - Impact: Registry entries had missing/incomplete provenance
//   - Fix: Changed to required ProvenanceMetadata with [Required] attribute
//
// VALIDATION STRATEGY:
//   - Provenance must be complete (not null, all required fields populated)
//   - CapsuleRegistryManager must validate before adding entries
//   - Fail fast if incomplete metadata detected
//
// AUDIENCE: Graduate / PhD (registry management, provenance tracking)
//===============================================================
#nullable enable

using System.ComponentModel.DataAnnotations;

namespace SymbolLabsForge.Contracts
{
    public class CapsuleRegistryEntry
    {
        public required string CapsuleId { get; set; }
        public required string TemplateName { get; set; }
        public required SymbolType SymbolType { get; set; }
        public required string TemplateHash { get; set; }
        public bool IsValid { get; set; }
        public required string SourcePath { get; set; }
        public string LastSeenUtc { get; set; } = string.Empty;
        public long SizeBytes { get; set; }

        /// <summary>
        /// Structured provenance metadata for full traceability.
        /// REQUIRED for all registry entries - cannot be null.
        /// </summary>
        [Required(ErrorMessage = "CapsuleRegistryEntry.Provenance is required for traceability.")]
        public required ProvenanceMetadata Provenance { get; set; }

        public bool IsConflict { get; set; } = false;
    }
}
