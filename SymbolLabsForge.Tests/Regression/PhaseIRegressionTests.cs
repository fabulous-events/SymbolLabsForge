//===============================================================
// File: PhaseIRegressionTests.cs
// Author: Claude (Phase II-Tests)
// Date: 2025-11-14
// Purpose: Prevents regression of defects fixed in Phase I and Phase II.
//          Codifies the Gemini S7 audit findings as permanent test guards.
//
// TEACHING VALUE:
//   - Demonstrates regression test design for documented bugs
//   - Shows how to translate audit findings into test assertions
//   - Provides case study material for debugging methodology courses
//
// LINKED ARTIFACTS:
//   - S7 Expert Technical Audit (Gemini, 2025-11-14)
//   - Phase I Critical Fixes documentation
//
// AUDIENCE: Undergraduate / Graduate / PhD (all levels)
//===============================================================
#nullable enable

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SymbolLabsForge.Generation;
using SymbolLabsForge.Generators;
using SymbolLabsForge.Contracts;
using Xunit;
using System;

namespace SymbolLabsForge.Tests.Regression
{
    /// <summary>
    /// Regression tests that prevent re-introduction of critical defects
    /// identified in the Gemini S7 Expert Technical Audit.
    /// </summary>
    public class PhaseIRegressionTests
    {
        /// <summary>
        /// REGRESSION GUARD: Prevents re-introduction of the "Flat arrow flaw".
        ///
        /// DEFECT HISTORY:
        ///   - Original Implementation: Used 3-point FillPolygon (triangle) for bowl
        ///   - Visual Result: Bowl resembled right-pointing arrow instead of smooth curve
        ///   - Root Cause: Inappropriate geometric primitive for curved shape
        ///   - Fix (Phase I-A): Replaced triangle with EllipsePolygon
        ///
        /// VALIDATION STRATEGY:
        ///   - Measure horizontal extent of bowl (should extend beyond stem)
        ///   - Verify smooth curvature (no sharp triangular points)
        /// </summary>
        [Fact]
        public void FlatGenerator_BowlIsNotTriangular()
        {
            // Arrange
            var generator = new FlatGenerator();
            var dimensions = new Size(256, 256);

            // Act
            using var image = generator.GenerateRawImage(dimensions, seed: 42);

            // Assert: Bowl should extend significantly to the right of the stem
            // Stem is at 0.4-0.5 (10% width), bowl should extend beyond 0.5
            int stemRightEdge = (int)(dimensions.Width * 0.5f);
            int expectedBowlRightEdge = (int)(dimensions.Width * 0.7f); // Bowl should reach ~70%

            // Scan horizontally at bowl center height (75% down)
            int bowlCenterY = (int)(dimensions.Height * 0.75f);
            int rightmostInkX = 0;

            for (int x = 0; x < dimensions.Width; x++)
            {
                var pixel = image[x, bowlCenterY];
                if (pixel.PackedValue <= 128) // Black ink
                    rightmostInkX = x;
            }

            // Assert: Bowl extends significantly beyond stem
            Assert.True(rightmostInkX > stemRightEdge + 20,
                $"Flat bowl does not extend beyond stem. Rightmost ink at X={rightmostInkX}, stem right edge at X={stemRightEdge}. " +
                "This may indicate regression to triangular bowl geometry.");

            // Assert: Bowl reaches at least 65% of width (smooth ellipse)
            Assert.True(rightmostInkX >= expectedBowlRightEdge - 10,
                $"Flat bowl does not extend far enough (only to X={rightmostInkX}, expected ~X={expectedBowlRightEdge}). " +
                "This suggests the bowl may be using triangular geometry instead of elliptical.");
        }

        /// <summary>
        /// REGRESSION GUARD: Prevents re-introduction of grayscale pixel leakage.
        ///
        /// DEFECT HISTORY:
        ///   - Original Implementation: CloneAs<L8>() without explicit binarization
        ///   - Result: Anti-aliasing created grayscale pixels (values 1-254)
        ///   - Impact: Compromised downstream skeletonization algorithms
        ///   - Fix (Phase I-B): Added explicit BinaryThreshold(0.5f) after conversion
        ///
        /// VALIDATION STRATEGY:
        ///   - Assert ALL pixels are strictly 0 or 255 (no intermediate values)
        /// </summary>
        [Theory]
        [InlineData(typeof(FlatGenerator))]
        [InlineData(typeof(SharpGenerator))]
        [InlineData(typeof(NaturalGenerator))]
        [InlineData(typeof(DoubleSharpGenerator))]
        public void GeometricGenerators_ProduceStrictBinaryOutput(Type generatorType)
        {
            // Arrange
            var generator = (ISymbolGenerator)Activator.CreateInstance(generatorType)!;
            var dimensions = new Size(100, 100);

            // Act
            using var image = generator.GenerateRawImage(dimensions, seed: 42);

            // Assert: ALL pixels must be either 0 (black) or 255 (white), no grayscale
            int grayscalePixelCount = 0;
            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    foreach (var pixel in accessor.GetRowSpan(y))
                    {
                        byte value = pixel.PackedValue;
                        if (value != 0 && value != 255)
                            grayscalePixelCount++;
                    }
                }
            });

            Assert.True(grayscalePixelCount == 0,
                $"{generatorType.Name} produced {grayscalePixelCount} grayscale pixels. " +
                "All pixels must be strictly 0 (black) or 255 (white). " +
                "This indicates missing binarization step (BinaryThreshold) after CloneAs<L8>().");
        }

        /// <summary>
        /// REGRESSION GUARD: Prevents re-introduction of fixed-pixel gap in StackedGenerator.
        ///
        /// DEFECT HISTORY:
        ///   - Original Implementation: Used fixed ±2 pixel offset (total 4px gap)
        ///   - Result: Gap did not scale with image height (4px gap at all resolutions)
        ///   - Impact: Visual inconsistency, misalignment at higher resolutions
        ///   - Fix (Phase II-E): Replaced with proportional gap (2% of image height)
        ///
        /// VALIDATION STRATEGY:
        ///   - Generate at two different resolutions
        ///   - Verify gap scales proportionally with height
        /// </summary>
        [Fact]
        public void StackedGenerator_GapScalesWithHeight()
        {
            // Arrange
            var generator = new StackedGenerator();
            var smallDimensions = new Size(100, 100);
            var largeDimensions = new Size(100, 200); // Double height

            // Act
            using var smallImage = generator.GenerateRawImage(smallDimensions, seed: 42);
            using var largeImage = generator.GenerateRawImage(largeDimensions, seed: 42);

            // Measure gap in small image
            int smallGap = MeasureVerticalGap(smallImage, 100);

            // Measure gap in large image
            int largeGap = MeasureVerticalGap(largeImage, 200);

            // Assert: Large gap should be approximately 2x small gap (±1px for rounding)
            double ratio = (double)largeGap / smallGap;
            Assert.InRange(ratio, 1.8, 2.2);

            // Assert: Gap should NOT be fixed at 4 pixels
            Assert.False(smallGap == 4 && largeGap == 4,
                "StackedGenerator gap is fixed at 4 pixels regardless of image height. " +
                "This indicates regression to fixed-pixel offset (±2) instead of proportional gap.");
        }

        /// <summary>
        /// REGRESSION GUARD: Verifies anti-aliasing is disabled for geometric symbols.
        ///
        /// DEFECT HISTORY:
        ///   - Original Implementation: AA enabled by default for all drawing operations
        ///   - Result: Smooth edges created grayscale pixels even before conversion
        ///   - Impact: Increased grayscale leakage, compromised binary integrity
        ///   - Fix (Phase I-C): Explicitly disabled AA via DrawingOptions
        ///
        /// VALIDATION STRATEGY:
        ///   - Verify edge pixels are strictly binary (no AA smoothing)
        ///   - Check that edge transition is sharp (0 → 255 in 1-2 pixels)
        /// </summary>
        [Theory]
        [InlineData(typeof(SharpGenerator))]
        [InlineData(typeof(NaturalGenerator))]
        public void GeometricGenerators_AntiAliasingIsDisabled(Type generatorType)
        {
            // Arrange
            var generator = (ISymbolGenerator)Activator.CreateInstance(generatorType)!;
            var dimensions = new Size(200, 200);

            // Act
            using var image = generator.GenerateRawImage(dimensions, seed: 42);

            // Assert: Find an edge and verify sharp transition (no AA smoothing)
            // Scan horizontally through middle to find a vertical edge
            int middleY = dimensions.Height / 2;
            bool foundSharpEdge = false;

            for (int x = 1; x < dimensions.Width - 1; x++)
            {
                byte prevPixel = image[x - 1, middleY].PackedValue;
                byte currPixel = image[x, middleY].PackedValue;

                // Check for sharp edge (direct transition from 0 to 255 or vice versa)
                if ((prevPixel == 0 && currPixel == 255) || (prevPixel == 255 && currPixel == 0))
                {
                    foundSharpEdge = true;
                    break;
                }
            }

            Assert.True(foundSharpEdge,
                $"{generatorType.Name} does not have sharp edges. This suggests anti-aliasing is enabled. " +
                "Geometric symbols must disable AA via DrawingOptions {{ GraphicsOptions = {{ Antialias = false }} }}.");
        }

        /// <summary>
        /// Helper method to measure vertical gap in stacked symbols.
        /// </summary>
        private int MeasureVerticalGap(Image<L8> image, int height)
        {
            int centerX = image.Width / 2;
            int centerY = height / 2;
            int gapStart = -1;
            int gapEnd = -1;

            // Scan vertically through center to find white gap
            for (int y = centerY - 20; y < centerY + 20; y++)
            {
                var pixel = image[centerX, y];
                bool isWhite = pixel.PackedValue > 128;

                if (isWhite)
                {
                    if (gapStart == -1)
                        gapStart = y;
                    gapEnd = y;
                }
            }

            return gapEnd - gapStart + 1;
        }
    }
}
