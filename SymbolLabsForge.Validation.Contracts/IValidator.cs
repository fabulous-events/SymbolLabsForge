//===============================================================
// File: IValidator.cs
// Author: Claude (Phase 8.3 - Modularization)
// Date: 2025-11-14
// Purpose: Generic interface for image validators.
//
// PHASE 8.3: MODULARIZATION - VALIDATION FRAMEWORK
//   - Generic over TMetadata and TMetrics for reusability
//   - Replaces hardcoded SymbolCapsule dependency
//   - Enables validation of any IImageContainer implementation
//
// DESIGN RATIONALE:
//   - Contravariant parameters (in) allow validators to accept derived types
//   - Metrics passed separately to Validate() for in-place updates
//   - ValidationResult provides narratable error messages
//
// BACKWARD COMPATIBILITY:
//   - SymbolLabsForge.Contracts maintains legacy IValidator interface
//   - ValidatorAdapter wraps generic validators for legacy code
//
// AUDIENCE: Graduate / PhD (generic programming, interface design)
//===============================================================
#nullable enable

namespace SymbolLabsForge.Validation.Contracts
{
    /// <summary>
    /// Interface for a validator that validates an image container.
    /// </summary>
    /// <typeparam name="TMetadata">Type of metadata (e.g., TemplateMetadata, OCRMetadata)</typeparam>
    /// <typeparam name="TMetrics">Type of quality metrics (e.g., QualityMetrics, OCRMetrics)</typeparam>
    public interface IValidator<in TMetadata, in TMetrics>
    {
        /// <summary>
        /// Human-readable name of the validator (e.g., "DensityValidator", "ContrastValidator").
        /// Used in narratable error messages and logging.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Validates the image container and updates metrics in-place.
        /// </summary>
        /// <param name="container">
        /// Container with image, metadata, and metrics.
        /// If null, validation fails with descriptive error message.
        /// </param>
        /// <param name="metrics">
        /// Metrics object to update (same reference as container.Metrics).
        /// Validators modify this object in-place (e.g., set DensityFraction, DensityStatus).
        /// </param>
        /// <returns>
        /// Validation result with narratable failure message.
        /// IsValid=true if validation passed; IsValid=false with FailureMessage if failed.
        /// </returns>
        ValidationResult Validate(IImageContainer<TMetadata, TMetrics>? container, TMetrics metrics);
    }
}
