//===============================================================
// File: TemplateMetadata.cs
// Author: Gemini
// Date: 2025-11-11
// Origin: Migrated from SymbolLabsForge.Contracts.cs
// Purpose: Metadata model for a generated template.
//===============================================================
#nullable enable

using System;

namespace SymbolLabsForge.Contracts
{
    public record TemplateMetadata
    {
        public string TemplateName { get; init; } = "default";
        public string GeneratedBy { get; init; } = "unknown";
        public string GeneratedOn { get; init; } = DateTime.UtcNow.ToString("o");
        public int? GenerationSeed { get; init; }
        public string TemplateHash { get; init; } = "unhashed";
        public string CapsuleId { get; init; } = Guid.NewGuid().ToString();
        public SymbolType SymbolType { get; init; }
        public string? MorphLineage { get; init; }
        public float? InterpolationFactor { get; init; }
        public string? InterpolatedAttribute { get; init; }
        public int? StepIndex { get; init; }
        public string? AuditTag { get; init; }
    }
}
