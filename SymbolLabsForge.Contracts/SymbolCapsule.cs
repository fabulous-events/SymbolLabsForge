//===============================================================
// File: SymbolCapsule.cs
// Author: Gemini (Original), Claude (Phase 8.3 - Modularization)
// Date: 2025-11-11 (Original), 2025-11-14 (IImageContainer Implementation)
// Origin: Migrated from SymbolLabsForge.Contracts.cs
// Purpose: DTO for a single generated symbol capsule.
//
// PHASE 8.3: MODULARIZATION - VALIDATION FRAMEWORK
//   - Implements IImageContainer<TemplateMetadata, QualityMetrics>
//   - Enables use with generic validators from Validation.Core
//   - Explicit interface implementation preserves existing public API
//
// AUDIENCE: Graduate / PhD (interface segregation, backward compatibility)
//===============================================================
#nullable enable

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using SymbolLabsForge.Validation.Contracts;

namespace SymbolLabsForge.Contracts
{
    /// <summary>
    /// DTO for a single generated symbol capsule.
    /// Implements IImageContainer to support generic validators.
    /// </summary>
    public record SymbolCapsule(
        Image<L8> TemplateImage,
        TemplateMetadata Metadata,
        QualityMetrics Metrics,
        bool IsValid,
        List<ValidationResult> ValidationResults)
        : IImageContainer<TemplateMetadata, QualityMetrics>, IDisposable
    {
        /// <summary>
        /// Explicit interface implementation to avoid polluting public API.
        /// Generic validators access image via IImageContainer.Image property.
        /// </summary>
        Image<L8> IImageContainer<TemplateMetadata, QualityMetrics>.Image => TemplateImage;

        /// <summary>
        /// Explicit interface implementation for metadata access.
        /// </summary>
        TemplateMetadata IImageContainer<TemplateMetadata, QualityMetrics>.Metadata => Metadata;

        /// <summary>
        /// Explicit interface implementation for metrics access.
        /// </summary>
        QualityMetrics IImageContainer<TemplateMetadata, QualityMetrics>.Metrics => Metrics;

        public void Dispose()
        {
            TemplateImage?.Dispose();
        }
    }
}
