//===============================================================
// File: DensityValidatorSettings.cs
// Author: Gemini (Original), Claude (Configuration Validation)
// Date: 2025-11-14
// Purpose: Configuration for DensityValidator with range validation.
//
// CONFIGURATION VALIDATION (Phase 2):
//   - Added [Range] attributes to enforce valid threshold bounds (0.0-1.0)
//   - Added custom validation to ensure MinDensityThreshold < MaxDensityThreshold
//   - Thresholds are fractions (0.0-1.0), not percentages (0-100)
//
// DEFECT HISTORY:
//   - Original Implementation: No validation, allowed invalid threshold ranges
//   - Root Cause: No fail-fast validation at startup
//   - Impact: Could configure MinDensityThreshold > MaxDensityThreshold (logical error)
//   - Fix: Added [Range] and custom IValidatableObject implementation
//
// AUDIENCE: Undergraduate / Graduate (configuration hygiene)
//===============================================================
using System.ComponentModel.DataAnnotations;

namespace SymbolLabsForge.Validation
{
    public class DensityValidatorSettings : IValidatableObject
    {
        public const string SectionName = "Validation:Density";

        /// <summary>
        /// Minimum density threshold as a fraction (0.0 to 1.0).
        /// Default: 0.05 (5% of pixels must be ink).
        /// </summary>
        [Range(0.0, 1.0, ErrorMessage = "MinDensityThreshold must be between 0.0 and 1.0 (fraction, not percentage).")]
        public float MinDensityThreshold { get; set; } = 0.05f;

        /// <summary>
        /// Maximum density threshold as a fraction (0.0 to 1.0).
        /// Default: 0.12 (12% of pixels must be ink).
        /// </summary>
        [Range(0.0, 1.0, ErrorMessage = "MaxDensityThreshold must be between 0.0 and 1.0 (fraction, not percentage).")]
        public float MaxDensityThreshold { get; set; } = 0.12f;

        /// <summary>
        /// Custom validation to ensure MinDensityThreshold &lt; MaxDensityThreshold.
        /// </summary>
        public IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> Validate(ValidationContext validationContext)
        {
            if (MinDensityThreshold >= MaxDensityThreshold)
            {
                yield return new System.ComponentModel.DataAnnotations.ValidationResult(
                    $"MinDensityThreshold ({MinDensityThreshold}) must be less than MaxDensityThreshold ({MaxDensityThreshold}).",
                    new[] { nameof(MinDensityThreshold), nameof(MaxDensityThreshold) });
            }
        }
    }
}
