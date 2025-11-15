//===============================================================
// File: SkeletonizationProcessor.cs
// Author: Gemini (Original), Claude (Phase 2A Refactor, Phase 8.8 Extraction)
// Date: 2025-11-15
// Purpose: Zhang-Suen thinning algorithm for skeletonization.
//
// PHASE 8.8: MODULARIZATION - UTILITY EXTRACTION
//   - Extracted from SymbolLabsForge.Preprocessing to ImageProcessing.Utilities
//   - Implements IPreprocessingStep for composable preprocessing pipelines
//   - Preserves Phase 2A critical bug fix documentation
//
// PHASE 2A CRITICAL BUG FIX (Historical Reference):
//   - Refactored entire processor to follow canonical ink/background standard
//   - DEFECT HISTORY: Original implementation had inverted and inconsistent
//     pixel representation:
//       * Line 37: Treated 0 as background (skip if 0)
//       * Lines 54-67: Treated 0 as ink (neighbor checks)
//       * GetA: Counted 0→255 transitions (backward)
//       * GetB: Counted 255 pixels as ink (inverted)
//       * Line 80: Set removed pixels to 0 (treated 0 as background)
//   - ROOT CAUSE: Lack of canonical standard enforcement
//   - IMPACT: Zhang-Suen algorithm produced unreliable/incorrect skeletons
//   - FIX: Established canonical standard (0 = ink, 255 = background)
//          and refactored all logic to use PixelUtils.IsInk() consistently
//
// CANONICAL STANDARD (aligned with Phase I-III):
//   - 0 (black) = ink / foreground
//   - 255 (white) = background
//   - PixelUtils.IsInk(byte value) returns true if value < 128
//
// ZHANG-SUEN ALGORITHM:
//   - A = number of background→ink transitions in 8-connected neighborhood
//   - B = number of ink neighbors (non-background pixels)
//   - Remove pixel if: A == 1 && (2 <= B <= 6) && border conditions met
//   - Iterates until no more pixels can be removed (convergence)
//
// WHY THIS MATTERS:
//   - Skeletonization reduces thick strokes to 1-pixel-wide centerlines
//   - Critical for template matching (reduces variability from stroke thickness)
//   - Zhang-Suen preserves topology (no disconnections or holes created)
//
// TEACHING VALUE:
//   - Undergraduate: Morphological operations, iterative algorithms
//   - Graduate: Topology preservation, 8-connectivity, algorithm correctness
//   - PhD: Phase 2A bug case study (defect archaeology, pixel standard governance)
//
// USAGE EXAMPLE:
//   var processor = new SkeletonizationProcessor();
//   Image<L8> skeletonized = processor.Process(binarizedImage);
//
// AUDIENCE: Graduate / PhD (algorithm validation, image processing)
//===============================================================
#nullable enable

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;

namespace SymbolLabsForge.ImageProcessing.Utilities
{
    /// <summary>
    /// Zhang-Suen thinning algorithm for skeletonization.
    /// Reduces thick strokes to 1-pixel-wide centerlines while preserving topology.
    /// </summary>
    /// <remarks>
    /// <para><b>Algorithm Overview:</b></para>
    /// <para>Zhang-Suen is a two-pass iterative algorithm that removes border pixels
    /// from ink regions until only the skeleton remains. It alternates between two
    /// sub-iterations to prevent disconnections and maintain topology.</para>
    ///
    /// <para><b>Convergence:</b></para>
    /// <para>The algorithm runs until no pixels are removed in both sub-iterations (stable state).
    /// For typical musical symbols, this takes 5-15 iterations.</para>
    ///
    /// <para><b>Performance:</b></para>
    /// <para>Time complexity: O(n × k) where n = pixel count, k = iterations (typically 5-15).</para>
    /// <para>Space complexity: O(n) for pixels-to-remove list.</para>
    ///
    /// <para><b>Phase 2A Bug Fix Reference:</b></para>
    /// <para>The original implementation had inverted pixel logic (0 treated as both ink AND background).
    /// This caused unreliable skeletons. The fix established canonical PixelUtils.IsInk() standard.</para>
    /// <para>See defect history in file header for full root cause analysis.</para>
    /// </remarks>
    public class SkeletonizationProcessor : IPreprocessingStep
    {
        /// <summary>
        /// Applies Zhang-Suen thinning to the input image.
        /// </summary>
        /// <param name="image">The input L8 grayscale image (must be binarized for best results).</param>
        /// <returns>A new L8 image with skeletonized (1-pixel-wide) strokes.</returns>
        /// <remarks>
        /// The input image is NOT modified. A clone is created for processing.
        /// </remarks>
        public Image<L8> Process(Image<L8> image)
        {
            var clone = image.Clone(); // Work on a copy
            bool changed;
            do
            {
                changed = false;
                changed |= ThinningIteration(clone, 0);
                changed |= ThinningIteration(clone, 1);
            } while (changed);

            return clone;
        }

        /// <summary>
        /// Performs one thinning sub-iteration (pass 0 or pass 1).
        /// </summary>
        /// <param name="image">The image to thin (modified in place).</param>
        /// <param name="iter">The sub-iteration index (0 or 1).</param>
        /// <returns>True if any pixels were removed, otherwise false.</returns>
        private bool ThinningIteration(Image<L8> image, int iter)
        {
            var pixelsToRemove = new List<Point>();
            bool changed = false;

            for (int y = 1; y < image.Height - 1; y++)
            {
                for (int x = 1; x < image.Width - 1; x++)
                {
                    // PHASE 2A FIX: Skip background pixels, process only ink pixels
                    // Original: if (image[x, y].PackedValue == 0) continue; (WRONG - treated 0 as background)
                    // Corrected: Use PixelUtils.IsInk() to follow canonical standard
                    if (!PixelUtils.IsInk(image[x, y].PackedValue)) continue;

                    int a = GetA(image, x, y);
                    int b = GetB(image, x, y);
                    byte p2 = image[x, y - 1].PackedValue;
                    byte p4 = image[x + 1, y].PackedValue;
                    byte p6 = image[x, y + 1].PackedValue;
                    byte p8 = image[x - 1, y].PackedValue;

                    if (a == 1 && (b >= 2 && b <= 6))
                    {
                        if (iter == 0)
                        {
                            // PHASE 2A FIX: Check if at least one neighbor is background (not ink)
                            // Original: if (p2 == 0 || p4 == 0 || p6 == 0) (WRONG - checked for ink)
                            // Corrected: Check for background pixels using !IsInk()
                            if (!PixelUtils.IsInk(p2) || !PixelUtils.IsInk(p4) || !PixelUtils.IsInk(p6))
                            {
                                if (!PixelUtils.IsInk(p4) || !PixelUtils.IsInk(p6) || !PixelUtils.IsInk(p8))
                                {
                                    pixelsToRemove.Add(new Point(x, y));
                                    changed = true;
                                }
                            }
                        }
                        else // iter == 1
                        {
                            if (!PixelUtils.IsInk(p2) || !PixelUtils.IsInk(p4) || !PixelUtils.IsInk(p8))
                            {
                                if (!PixelUtils.IsInk(p2) || !PixelUtils.IsInk(p6) || !PixelUtils.IsInk(p8))
                                {
                                    pixelsToRemove.Add(new Point(x, y));
                                    changed = true;
                                }
                            }
                        }
                    }
                }
            }

            // PHASE 2A FIX: Set removed pixels to background (255), not ink (0)
            // Original: image[p.X, p.Y] = new L8(0); (WRONG - treated 0 as background)
            // Corrected: Set to 255 (white/background) to remove ink
            foreach (var p in pixelsToRemove)
            {
                image[p.X, p.Y] = new L8(255);
            }

            return changed;
        }

        /// <summary>
        /// PHASE 2A FIX: Counts background→ink transitions in 8-connected neighborhood.
        /// Original counted ink→background (0→255), which was inverted.
        /// Corrected to count background→ink (255→0 or !IsInk→IsInk).
        /// </summary>
        /// <param name="image">The image to analyze.</param>
        /// <param name="x">The x-coordinate of the center pixel.</param>
        /// <param name="y">The y-coordinate of the center pixel.</param>
        /// <returns>The number of background→ink transitions (A value).</returns>
        private int GetA(Image<L8> image, int x, int y)
        {
            int count = 0;
            // p2 -> p3
            if (!PixelUtils.IsInk(image[x, y - 1].PackedValue) && PixelUtils.IsInk(image[x + 1, y - 1].PackedValue)) count++;
            // p3 -> p4
            if (!PixelUtils.IsInk(image[x + 1, y - 1].PackedValue) && PixelUtils.IsInk(image[x + 1, y].PackedValue)) count++;
            // p4 -> p5
            if (!PixelUtils.IsInk(image[x + 1, y].PackedValue) && PixelUtils.IsInk(image[x + 1, y + 1].PackedValue)) count++;
            // p5 -> p6
            if (!PixelUtils.IsInk(image[x + 1, y + 1].PackedValue) && PixelUtils.IsInk(image[x, y + 1].PackedValue)) count++;
            // p6 -> p7
            if (!PixelUtils.IsInk(image[x, y + 1].PackedValue) && PixelUtils.IsInk(image[x - 1, y + 1].PackedValue)) count++;
            // p7 -> p8
            if (!PixelUtils.IsInk(image[x - 1, y + 1].PackedValue) && PixelUtils.IsInk(image[x - 1, y].PackedValue)) count++;
            // p8 -> p9
            if (!PixelUtils.IsInk(image[x - 1, y].PackedValue) && PixelUtils.IsInk(image[x - 1, y - 1].PackedValue)) count++;
            // p9 -> p2
            if (!PixelUtils.IsInk(image[x - 1, y - 1].PackedValue) && PixelUtils.IsInk(image[x, y - 1].PackedValue)) count++;
            return count;
        }

        /// <summary>
        /// PHASE 2A FIX: Counts ink neighbors (non-background pixels).
        /// Original counted background pixels (== 255), which was inverted.
        /// Corrected to count ink pixels using IsInk().
        /// </summary>
        /// <param name="image">The image to analyze.</param>
        /// <param name="x">The x-coordinate of the center pixel.</param>
        /// <param name="y">The y-coordinate of the center pixel.</param>
        /// <returns>The number of ink neighbors (B value).</returns>
        private int GetB(Image<L8> image, int x, int y)
        {
            int count = 0;
            if (PixelUtils.IsInk(image[x, y - 1].PackedValue)) count++;
            if (PixelUtils.IsInk(image[x + 1, y - 1].PackedValue)) count++;
            if (PixelUtils.IsInk(image[x + 1, y].PackedValue)) count++;
            if (PixelUtils.IsInk(image[x + 1, y + 1].PackedValue)) count++;
            if (PixelUtils.IsInk(image[x, y + 1].PackedValue)) count++;
            if (PixelUtils.IsInk(image[x - 1, y + 1].PackedValue)) count++;
            if (PixelUtils.IsInk(image[x - 1, y].PackedValue)) count++;
            if (PixelUtils.IsInk(image[x - 1, y - 1].PackedValue)) count++;
            return count;
        }
    }
}
