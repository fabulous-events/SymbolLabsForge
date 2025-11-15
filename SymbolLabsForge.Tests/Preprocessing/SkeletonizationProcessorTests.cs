//===============================================================
// File: SkeletonizationProcessorTests.cs
// Author: Claude (Phase 2B)
// Date: 2025-11-14
// Purpose: Validates Zhang-Suen thinning algorithm with known test vectors.
//          Regression guards for Phase 2A critical bug fix.
//
// DEFECT HISTORY (Phase 2A):
//   - Original SkeletonizationProcessor had inverted ink/background logic
//   - ROOT CAUSE: Inconsistent pixel representation (0 treated as both ink and background)
//   - IMPACT: Zhang-Suen algorithm produced incorrect/unreliable skeletons
//   - FIX: Refactored to follow canonical standard (0 = ink, 255 = background)
//
// VALIDATION STRATEGY:
//   - Test patterns with known expected skeletons
//   - Verify algorithm preserves connectivity
//   - Confirm single-pixel-wide output for thick shapes
//   - Test edge cases (empty, single pixel, already-thin)
//
// CANONICAL STANDARD:
//   - 0 (black) = ink / foreground
//   - 255 (white) = background
//   - Consistent with Phase I-III rendering hygiene
//
// AUDIENCE: Graduate / PhD (algorithm validation)
//===============================================================
#nullable enable

using Xunit;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SymbolLabsForge.ImageProcessing.Utilities;
using SymbolLabsForge.Utils;
using System.Linq;

namespace SymbolLabsForge.Tests.Preprocessing
{
    public class SkeletonizationProcessorTests
    {
        private readonly SkeletonizationProcessor _processor = new();

        /// <summary>
        /// Test Vector 1: Single pixel should remain unchanged.
        /// </summary>
        [Fact]
        public void Process_SinglePixel_RemainsUnchanged()
        {
            // Arrange: 3x3 image with single center ink pixel
            var image = CreateTestImage(3, 3, new byte[] {
                255, 255, 255,
                255,   0, 255,
                255, 255, 255
            });

            // Act
            using var result = _processor.Process(image);

            // Assert: Center pixel should still be ink
            Assert.Equal(0, result[1, 1].PackedValue);
            // Assert: All other pixels remain background
            Assert.Equal(255, result[0, 0].PackedValue);
            Assert.Equal(255, result[2, 2].PackedValue);
        }

        /// <summary>
        /// Test Vector 2: Horizontal line (already skeleton) should remain unchanged.
        /// </summary>
        [Fact]
        public void Process_HorizontalLine_RemainsUnchanged()
        {
            // Arrange: 5x3 image with horizontal line through center
            var image = CreateTestImage(5, 3, new byte[] {
                255, 255, 255, 255, 255,
                  0,   0,   0,   0,   0,  // Horizontal ink line
                255, 255, 255, 255, 255
            });

            // Act
            using var result = _processor.Process(image);

            // Assert: All center row pixels should still be ink
            for (int x = 0; x < 5; x++)
            {
                Assert.True(PixelUtils.IsInk(result[x, 1].PackedValue),
                    $"Horizontal line pixel at ({x}, 1) should remain ink after skeletonization");
            }

            // Assert: Top and bottom rows remain background
            for (int x = 0; x < 5; x++)
            {
                Assert.False(PixelUtils.IsInk(result[x, 0].PackedValue),
                    $"Top row pixel at ({x}, 0) should remain background");
                Assert.False(PixelUtils.IsInk(result[x, 2].PackedValue),
                    $"Bottom row pixel at ({x}, 2) should remain background");
            }
        }

        /// <summary>
        /// Test Vector 3: Vertical line (already skeleton) should remain unchanged.
        /// </summary>
        [Fact]
        public void Process_VerticalLine_RemainsUnchanged()
        {
            // Arrange: 3x5 image with vertical line through center
            var image = CreateTestImage(3, 5, new byte[] {
                255,   0, 255,
                255,   0, 255,
                255,   0, 255,
                255,   0, 255,
                255,   0, 255
            });

            // Act
            using var result = _processor.Process(image);

            // Assert: All center column pixels should still be ink
            for (int y = 0; y < 5; y++)
            {
                Assert.True(PixelUtils.IsInk(result[1, y].PackedValue),
                    $"Vertical line pixel at (1, {y}) should remain ink after skeletonization");
            }
        }

        /// <summary>
        /// Test Vector 4: 3x3 solid block should thin to minimal skeleton.
        /// Expected: Center pixel and possibly cross pattern depending on iteration order.
        /// </summary>
        [Fact]
        public void Process_SolidBlock_ThinsToSkeleton()
        {
            // Arrange: 5x5 image with 3x3 solid block in center
            var image = CreateTestImage(5, 5, new byte[] {
                255, 255, 255, 255, 255,
                255,   0,   0,   0, 255,
                255,   0,   0,   0, 255,
                255,   0,   0,   0, 255,
                255, 255, 255, 255, 255
            });

            // Act
            using var result = _processor.Process(image);

            // Assert: Center pixel must remain ink (skeleton center)
            Assert.True(PixelUtils.IsInk(result[2, 2].PackedValue),
                "Center pixel of 3x3 block should remain ink after skeletonization");

            // Assert: Result should be thinner than original
            int originalInkCount = CountInkPixels(image);
            int skeletonInkCount = CountInkPixels(result);
            Assert.True(skeletonInkCount < originalInkCount,
                $"Skeleton ({skeletonInkCount} pixels) should have fewer ink pixels than original ({originalInkCount} pixels)");
            Assert.True(skeletonInkCount >= 1,
                "Skeleton should have at least 1 ink pixel (cannot be empty)");
        }

        /// <summary>
        /// Test Vector 5: Empty image (all background) should remain empty.
        /// </summary>
        [Fact]
        public void Process_EmptyImage_RemainsEmpty()
        {
            // Arrange: 5x5 image with all background
            var image = CreateTestImage(5, 5, Enumerable.Repeat((byte)255, 25).ToArray());

            // Act
            using var result = _processor.Process(image);

            // Assert: All pixels should remain background
            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    Assert.False(PixelUtils.IsInk(result[x, y].PackedValue),
                        $"Pixel at ({x}, {y}) should remain background in empty image");
                }
            }
        }

        /// <summary>
        /// Test Vector 6: Diagonal line should remain connected and thin.
        /// </summary>
        [Fact]
        public void Process_DiagonalLine_RemainsConnected()
        {
            // Arrange: 5x5 image with diagonal line from top-left to bottom-right
            var image = CreateTestImage(5, 5, new byte[] {
                  0, 255, 255, 255, 255,
                255,   0, 255, 255, 255,
                255, 255,   0, 255, 255,
                255, 255, 255,   0, 255,
                255, 255, 255, 255,   0
            });

            // Act
            using var result = _processor.Process(image);

            // Assert: Diagonal pixels should remain ink (already skeleton)
            Assert.True(PixelUtils.IsInk(result[0, 0].PackedValue), "Diagonal pixel (0,0) should remain ink");
            Assert.True(PixelUtils.IsInk(result[1, 1].PackedValue), "Diagonal pixel (1,1) should remain ink");
            Assert.True(PixelUtils.IsInk(result[2, 2].PackedValue), "Diagonal pixel (2,2) should remain ink");
            Assert.True(PixelUtils.IsInk(result[3, 3].PackedValue), "Diagonal pixel (3,3) should remain ink");
            Assert.True(PixelUtils.IsInk(result[4, 4].PackedValue), "Diagonal pixel (4,4) should remain ink");
        }

        /// <summary>
        /// Test Vector 7: L-shape pattern should thin to connected skeleton.
        /// </summary>
        [Fact]
        public void Process_LShape_MaintainsConnectivity()
        {
            // Arrange: 7x7 image with L-shape pattern (thick lines)
            var image = CreateTestImage(7, 7, new byte[] {
                255, 255, 255, 255, 255, 255, 255,
                255,   0,   0,   0, 255, 255, 255,
                255,   0,   0,   0, 255, 255, 255,
                255,   0,   0,   0, 255, 255, 255,
                255,   0,   0,   0,   0,   0, 255,
                255,   0,   0,   0,   0,   0, 255,
                255, 255, 255, 255, 255, 255, 255
            });

            // Act
            using var result = _processor.Process(image);

            // Assert: Skeleton should be thinner than original
            int originalInkCount = CountInkPixels(image);
            int skeletonInkCount = CountInkPixels(result);
            Assert.True(skeletonInkCount < originalInkCount,
                $"L-shape skeleton ({skeletonInkCount} pixels) should be thinner than original ({originalInkCount} pixels)");

            // Assert: Skeleton should maintain connectivity (corner pixel must be ink)
            Assert.True(PixelUtils.IsInk(result[3, 4].PackedValue) || PixelUtils.IsInk(result[3, 3].PackedValue),
                "L-shape skeleton should maintain connectivity at the corner");
        }

        /// <summary>
        /// Test Vector 8: Regression guard for Phase 2A canonical standard.
        /// Verifies that removed pixels are set to 255 (background), not 0.
        /// </summary>
        [Fact]
        public void Process_RemovedPixels_SetToBackground255()
        {
            // Arrange: 5x5 image with 3x3 solid block
            var image = CreateTestImage(5, 5, new byte[] {
                255, 255, 255, 255, 255,
                255,   0,   0,   0, 255,
                255,   0,   0,   0, 255,
                255,   0,   0,   0, 255,
                255, 255, 255, 255, 255
            });

            // Act
            using var result = _processor.Process(image);

            // Assert: Any pixel that was removed should be exactly 255 (not 0)
            // We verify this by checking that all non-ink pixels are exactly 255
            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    byte pixelValue = result[x, y].PackedValue;
                    if (!PixelUtils.IsInk(pixelValue))
                    {
                        Assert.Equal(255, pixelValue);
                    }
                }
            }
        }

        /// <summary>
        /// Test Vector 9: Binary integrity check - all pixels must be 0 or 255 after processing.
        /// Regression guard for Phase I-III binary output requirement.
        /// </summary>
        [Fact]
        public void Process_OutputIsBinary_AllPixels0Or255()
        {
            // Arrange: Various test patterns
            var testImages = new[]
            {
                CreateTestImage(3, 3, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 }),
                CreateTestImage(5, 5, Enumerable.Repeat((byte)255, 25).ToArray()),
                CreateTestImage(7, 7, new byte[] {
                    255, 255, 255, 255, 255, 255, 255,
                    255, 0, 0, 0, 0, 0, 255,
                    255, 0, 255, 255, 255, 0, 255,
                    255, 0, 255, 0, 255, 0, 255,
                    255, 0, 255, 255, 255, 0, 255,
                    255, 0, 0, 0, 0, 0, 255,
                    255, 255, 255, 255, 255, 255, 255
                })
            };

            foreach (var image in testImages)
            {
                // Act
                using var result = _processor.Process(image);

                // Assert: All pixels must be strictly 0 or 255
                for (int y = 0; y < result.Height; y++)
                {
                    for (int x = 0; x < result.Width; x++)
                    {
                        byte pixelValue = result[x, y].PackedValue;
                        Assert.True(pixelValue == 0 || pixelValue == 255,
                            $"Pixel at ({x}, {y}) has invalid value {pixelValue}. Must be 0 or 255 for binary integrity.");
                    }
                }

                image.Dispose();
            }
        }

        // ===== Helper Methods =====

        /// <summary>
        /// Creates a test image from a byte array (row-major order).
        /// </summary>
        private Image<L8> CreateTestImage(int width, int height, byte[] pixels)
        {
            Assert.Equal(width * height, pixels.Length);

            var image = new Image<L8>(width, height);
            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < height; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    for (int x = 0; x < width; x++)
                    {
                        row[x] = new L8(pixels[y * width + x]);
                    }
                }
            });

            return image;
        }

        /// <summary>
        /// Counts ink pixels in an image.
        /// </summary>
        private int CountInkPixels(Image<L8> image)
        {
            int count = 0;
            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    foreach (var pixel in accessor.GetRowSpan(y))
                    {
                        if (PixelUtils.IsInk(pixel.PackedValue))
                            count++;
                    }
                }
            });
            return count;
        }
    }
}
