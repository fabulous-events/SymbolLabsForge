//===============================================================
// File: PixelUtils.cs
// Author: Gemini (Original), Claude (Phase 8.4 - Extracted to ImageProcessing.Utilities)
// Date: 2025-11-14
// Purpose: Canonical pixel-level operations for all image processing.
//
// PHASE 8.4: MODULARIZATION - IMAGE PROCESSING UTILITIES
//   - Extracted from SymbolLabsForge.Utils to ImageProcessing.Utilities
//   - Canonical implementation (replaces duplicates in Validation.Core and SymbolLabsForge)
//   - Establishes universal ink/background standard for entire codebase
//
// CANONICAL PIXEL STANDARD:
//   - 0 (black) = ink / foreground
//   - 255 (white) = background
//   - Threshold: 128 (values < 128 are ink, >= 128 are background)
//
// WHY THIS MATTERS:
//   - Phase 2A Bug: Skeletonization had inverted pixel logic (0 treated as both ink AND background)
//   - Impact: Zhang-Suen algorithm produced unreliable skeletons
//   - Fix: Centralized PixelUtils.IsInk() ensures consistency across all image processing
//
// AUDIENCE: Undergraduate / Graduate (image processing fundamentals, governance standards)
//===============================================================
#nullable enable

namespace SymbolLabsForge.ImageProcessing.Utilities
{
    /// <summary>
    /// Universal constants for image processing to avoid magic numbers.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The canonical threshold for considering a pixel as "ink".
        /// Values below this in an L8 image are ink (foreground).
        /// Values at or above this are background.
        /// </summary>
        /// <remarks>
        /// GOVERNANCE: This threshold is canonical across the entire codebase.
        /// DO NOT use hardcoded values like 128 in image processing logic.
        /// Always use PixelUtils.IsInk(value) for pixel classification.
        /// </remarks>
        public const byte DefaultInkThreshold = 128;
    }

    /// <summary>
    /// Canonical pixel-level operations for image processing.
    /// All image processing code must use these utilities to ensure consistency.
    /// </summary>
    public static class PixelUtils
    {
        /// <summary>
        /// Determines if a pixel value represents ink (foreground) based on a given threshold.
        /// </summary>
        /// <param name="value">The L8 pixel value (0-255).</param>
        /// <param name="threshold">The threshold below which a pixel is considered ink. Default: 128.</param>
        /// <returns>True if the pixel is ink (value &lt; threshold), otherwise false.</returns>
        /// <remarks>
        /// CANONICAL STANDARD:
        /// - 0 (black) = ink
        /// - 255 (white) = background
        /// - Threshold: 128 (default)
        ///
        /// USAGE:
        /// - Validators: Use for density and contrast calculations
        /// - Preprocessing: Use for skeletonization, binarization, morphology
        /// - Detection: Use for template matching, component analysis
        ///
        /// DEFECT PREVENTION:
        /// - Phase 2A Bug: Inverted pixel logic in SkeletonizationProcessor
        /// - Fix: Centralized pixel classification prevents inconsistent interpretations
        /// </remarks>
        public static bool IsInk(byte value, byte threshold = Constants.DefaultInkThreshold)
        {
            return value < threshold;
        }

        /// <summary>
        /// Determines if a pixel value represents background based on a given threshold.
        /// </summary>
        /// <param name="value">The L8 pixel value (0-255).</param>
        /// <param name="threshold">The threshold at or above which a pixel is considered background. Default: 128.</param>
        /// <returns>True if the pixel is background (value >= threshold), otherwise false.</returns>
        public static bool IsBackground(byte value, byte threshold = Constants.DefaultInkThreshold)
        {
            return value >= threshold;
        }
    }
}
