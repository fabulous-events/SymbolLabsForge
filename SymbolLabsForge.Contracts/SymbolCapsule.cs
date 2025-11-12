//===============================================================
// File: SymbolCapsule.cs
// Author: Gemini
// Date: 2025-11-11
// Origin: Migrated from SymbolLabsForge.Contracts.cs
// Purpose: DTO for a single generated symbol capsule.
//===============================================================
#nullable enable

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;

namespace SymbolLabsForge.Contracts
{
    public record SymbolCapsule(
        Image<L8> TemplateImage,
        TemplateMetadata Metadata,
        QualityMetrics Metrics,
        bool IsValid,
        List<ValidationResult> ValidationResults);
}
