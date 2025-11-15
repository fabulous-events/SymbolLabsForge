//===============================================================
// File: IImageContainer.cs
// Author: Claude (Phase 8.3 - Modularization)
// Date: 2025-11-14
// Purpose: Generic abstraction for an image container with metadata and metrics.
//
// PHASE 8.3: MODULARIZATION - VALIDATION FRAMEWORK
//   - Breaks SymbolCapsule coupling from validators
//   - Enables validators to work with any image container type
//   - Supports reusability in OCR, document processing, computer vision
//
// DESIGN RATIONALE:
//   - Covariant TMetadata/TMetrics (out) allows flexibility in consumer code
//   - Image<L8> is standard grayscale format for validation
//   - Metrics passed separately to Validate() to support in-place updates
//
// AUDIENCE: Graduate / PhD (abstraction design, dependency inversion)
//===============================================================
#nullable enable

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SymbolLabsForge.Validation.Contracts
{
    /// <summary>
    /// Represents a container for an image with associated metadata and quality metrics.
    /// This abstraction allows validators to work with any image container type,
    /// not just SymbolCapsule.
    /// </summary>
    /// <typeparam name="TMetadata">
    /// Type of metadata associated with the image (e.g., TemplateMetadata, OCRMetadata).
    /// Must be covariant (out) to support inheritance hierarchies.
    /// </typeparam>
    /// <typeparam name="TMetrics">
    /// Type of quality metrics (e.g., QualityMetrics, OCRMetrics).
    /// Must be covariant (out) to support inheritance hierarchies.
    /// </typeparam>
    public interface IImageContainer<out TMetadata, out TMetrics>
    {
        /// <summary>
        /// The L8 (8-bit grayscale) image to validate.
        /// All validators operate on L8 format for consistency.
        /// </summary>
        Image<L8> Image { get; }

        /// <summary>
        /// Metadata associated with the image (e.g., provenance, template name, source).
        /// Immutable during validation; used for context and logging.
        /// </summary>
        TMetadata Metadata { get; }

        /// <summary>
        /// Quality metrics associated with the image (e.g., density, aspect ratio, contrast).
        /// Validators update this object in-place during validation.
        /// </summary>
        TMetrics Metrics { get; }
    }
}
