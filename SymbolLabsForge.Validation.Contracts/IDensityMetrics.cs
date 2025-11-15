//===============================================================
// File: IDensityMetrics.cs
// Author: Claude (Phase 8.3 - Modularization)
// Date: 2025-11-14
// Purpose: Constraint interface for metrics that support density validation.
//
// PHASE 8.3: MODULARIZATION - VALIDATION FRAMEWORK
//   - Enables DensityValidator<TMetadata, TMetrics> where TMetrics : IDensityMetrics
//   - Decouples validators from specific metrics implementation (QualityMetrics)
//   - Allows custom metrics classes in other projects
//
// DESIGN RATIONALE:
//   - Properties are mutable (validators set values during validation)
//   - DensityFraction (0.0-1.0) for calculations, DensityPercent (0-100) for display
//   - DensityStatus enum provides categorical classification
//
// IMPLEMENTATION NOTES:
//   - SymbolLabsForge.Contracts.QualityMetrics implements this interface
//   - Custom metrics classes must implement this to use DensityValidator
//
// AUDIENCE: Graduate / PhD (interface segregation, constraint-based generics)
//===============================================================
#nullable enable

namespace SymbolLabsForge.Validation.Contracts
{
    /// <summary>
    /// Interface for metrics that support density validation.
    /// Implement this interface to enable use of DensityValidator.
    /// </summary>
    public interface IDensityMetrics
    {
        /// <summary>
        /// Pixel density as a fraction (0.0 to 1.0).
        /// Example: 0.05 = 5% of pixels are ink.
        /// Used for threshold comparisons and calculations.
        /// </summary>
        double DensityFraction { get; set; }

        /// <summary>
        /// Pixel density as a percentage (0 to 100).
        /// Example: 5.0 = 5% of pixels are ink.
        /// Used for display and logging purposes.
        /// </summary>
        double DensityPercent { get; set; }

        /// <summary>
        /// Categorical classification of density (TooLow, Acceptable, TooHigh, Unknown).
        /// Set by DensityValidator based on threshold comparison.
        /// </summary>
        DensityStatus DensityStatus { get; set; }
    }

    /// <summary>
    /// Categorical classification of pixel density.
    /// </summary>
    public enum DensityStatus
    {
        /// <summary>Density not yet calculated or unknown</summary>
        Unknown = 0,

        /// <summary>Density within valid range</summary>
        Valid = 1,

        /// <summary>Density above maximum threshold (too many ink pixels)</summary>
        TooHigh = 2,

        /// <summary>Density below minimum threshold (too few ink pixels)</summary>
        TooLow = 3
    }
}
