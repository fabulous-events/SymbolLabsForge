//===============================================================
// File: QualityMetrics.cs
// Author: Gemini
// Date: 2025-11-11
// Updated: 2025-11-14 (Claude - Validator Redesign Phase 1B)
// Origin: Migrated from SymbolLabsForge.Contracts.cs
// Purpose: Model for quality metrics of a generated symbol.
//
// VALIDATOR REDESIGN PHASE 1B:
//   - Added DensityFraction (0.0-1.0) and DensityPercent (0-100) properties
//   - Eliminated ambiguity in density representation
//   - Marked old Density property as obsolete for migration guidance
//
// DEFECT HISTORY:
//   - Original Issue: Single Density property created ambiguity about whether
//     values represented fractions (0.05 = 5%) or percentages (5 = 5%)
//   - Root Cause: DensityValidator compared as fraction but stored as percentage
//   - Impact: Confusing threshold configuration, error messages required
//     multiplying by 100 to display correctly
//   - Fix: Split into two explicit properties with clear semantics
//
// AUDIENCE: Undergraduate / Graduate (architectural refactoring)
//===============================================================
#nullable enable

using System;
using SymbolLabsForge.Validation.Contracts;

namespace SymbolLabsForge.Contracts
{
    /// <summary>
    /// Quality metrics for a generated symbol.
    /// Implements IDensityMetrics to support DensityValidator.
    ///
    /// PHASE 8.3: Implements IDensityMetrics from Validation.Contracts.
    /// </summary>
    public class QualityMetrics : IDensityMetrics
    {
        public double AspectRatio { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        /// <summary>
        /// Pixel density as a fraction (0.0 to 1.0).
        /// Example: 0.15 = 15% of pixels are ink.
        /// Use this for threshold comparisons and calculations.
        /// </summary>
        public double DensityFraction { get; set; }

        /// <summary>
        /// Pixel density as a percentage (0 to 100).
        /// Example: 15.0 = 15% of pixels are ink.
        /// Use this for display and logging purposes.
        /// </summary>
        public double DensityPercent { get; set; }

        /// <summary>
        /// DEPRECATED: Use DensityFraction or DensityPercent instead.
        /// This property is maintained for backward compatibility only.
        /// </summary>
        [Obsolete("Use DensityFraction (0.0-1.0) or DensityPercent (0-100) instead. This property will be removed in a future version.")]
        public double Density
        {
            get => DensityPercent;
            set
            {
                DensityPercent = value;
                DensityFraction = value / 100.0;
            }
        }

        public DensityStatus DensityStatus { get; set; } = DensityStatus.Unknown;
    }
}
