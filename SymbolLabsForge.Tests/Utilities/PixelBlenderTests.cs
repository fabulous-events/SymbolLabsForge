//===============================================================
// File: PixelBlenderTests.cs
// Author: Claude (Phase 9.2 - PixelBlender Utility Tests)
// Date: 2025-11-15
// Purpose: Comprehensive unit tests for PixelBlender pure blending algorithms.
//
// PHASE 9.2: PIXEL BLENDER TESTING
//   - Validates all 6 blend modes: Linear, Alpha, Additive, Multiply, Screen, Overlay
//   - Tests edge cases: null inputs, dimension mismatches, invalid parameters
//   - Verifies mathematical correctness of blending formulas
//   - Ensures proper clamping and overflow handling
//
// WHY THIS MATTERS:
//   - Pure utility testing (no I/O, no DI dependencies)
//   - Validates mathematical correctness of graphics algorithms
//   - Ensures blend modes behave as documented
//   - Prevents regressions in core image processing logic
//
// TEACHING VALUE:
//   - Undergraduate: Unit testing pure functions, mathematical validation
//   - Graduate: Blend mode mathematics, edge case coverage
//   - PhD: Performance benchmarking opportunities (deferred to Phase 9.3)
//
// AUDIENCE: Undergraduate / Graduate (testing, image processing)
//===============================================================
#nullable enable

using Xunit;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SymbolLabsForge.ImageProcessing.Utilities;
using System;

namespace SymbolLabsForge.Tests.Utilities
{
    public class PixelBlenderTests
    {
        #region LinearBlend Tests

        [Fact]
        public void LinearBlend_Factor0_ReturnsFrom()
        {
            // Arrange
            var from = CreateSolidImage(10, 10, 100);
            var to = CreateSolidImage(10, 10, 200);

            // Act
            using var result = PixelBlender.LinearBlend(from, to, factor: 0.0f);

            // Assert
            Assert.Equal(100, result[0, 0].PackedValue); // Should be 100% from
            Assert.Equal(100, result[5, 5].PackedValue);

            // Cleanup
            from.Dispose();
            to.Dispose();
        }

        [Fact]
        public void LinearBlend_Factor1_ReturnsTo()
        {
            // Arrange
            var from = CreateSolidImage(10, 10, 100);
            var to = CreateSolidImage(10, 10, 200);

            // Act
            using var result = PixelBlender.LinearBlend(from, to, factor: 1.0f);

            // Assert
            Assert.Equal(200, result[0, 0].PackedValue); // Should be 100% to
            Assert.Equal(200, result[5, 5].PackedValue);

            // Cleanup
            from.Dispose();
            to.Dispose();
        }

        [Fact]
        public void LinearBlend_Factor0_5_ReturnsMiddle()
        {
            // Arrange
            var from = CreateSolidImage(10, 10, 100);
            var to = CreateSolidImage(10, 10, 200);

            // Act
            using var result = PixelBlender.LinearBlend(from, to, factor: 0.5f);

            // Assert: 100 * 0.5 + 200 * 0.5 = 150
            Assert.Equal(150, result[0, 0].PackedValue);
            Assert.Equal(150, result[5, 5].PackedValue);

            // Cleanup
            from.Dispose();
            to.Dispose();
        }

        [Fact]
        public void LinearBlend_NullFrom_ThrowsArgumentNullException()
        {
            // Arrange
            Image<L8>? from = null;
            var to = CreateSolidImage(10, 10, 100);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => PixelBlender.LinearBlend(from!, to, 0.5f));

            // Cleanup
            to.Dispose();
        }

        [Fact]
        public void LinearBlend_NullTo_ThrowsArgumentNullException()
        {
            // Arrange
            var from = CreateSolidImage(10, 10, 100);
            Image<L8>? to = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => PixelBlender.LinearBlend(from, to!, 0.5f));

            // Cleanup
            from.Dispose();
        }

        [Fact]
        public void LinearBlend_DifferentDimensions_ThrowsArgumentException()
        {
            // Arrange
            var from = CreateSolidImage(10, 10, 100);
            var to = CreateSolidImage(20, 20, 200);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => PixelBlender.LinearBlend(from, to, 0.5f));

            // Cleanup
            from.Dispose();
            to.Dispose();
        }

        [Fact]
        public void LinearBlend_FactorNegative_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var from = CreateSolidImage(10, 10, 100);
            var to = CreateSolidImage(10, 10, 200);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => PixelBlender.LinearBlend(from, to, -0.1f));

            // Cleanup
            from.Dispose();
            to.Dispose();
        }

        [Fact]
        public void LinearBlend_FactorGreaterThan1_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var from = CreateSolidImage(10, 10, 100);
            var to = CreateSolidImage(10, 10, 200);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => PixelBlender.LinearBlend(from, to, 1.1f));

            // Cleanup
            from.Dispose();
            to.Dispose();
        }

        #endregion

        #region AlphaBlend Tests

        [Fact]
        public void AlphaBlend_Alpha0_ReturnsBackground()
        {
            // Arrange
            var background = CreateSolidImage(10, 10, 100);
            var foreground = CreateSolidImage(10, 10, 200);

            // Act
            using var result = PixelBlender.AlphaBlend(background, foreground, alpha: 0.0f);

            // Assert
            Assert.Equal(100, result[0, 0].PackedValue); // Should be 100% background
            Assert.Equal(100, result[5, 5].PackedValue);

            // Cleanup
            background.Dispose();
            foreground.Dispose();
        }

        [Fact]
        public void AlphaBlend_Alpha1_ReturnsForeground()
        {
            // Arrange
            var background = CreateSolidImage(10, 10, 100);
            var foreground = CreateSolidImage(10, 10, 200);

            // Act
            using var result = PixelBlender.AlphaBlend(background, foreground, alpha: 1.0f);

            // Assert
            Assert.Equal(200, result[0, 0].PackedValue); // Should be 100% foreground
            Assert.Equal(200, result[5, 5].PackedValue);

            // Cleanup
            background.Dispose();
            foreground.Dispose();
        }

        [Fact]
        public void AlphaBlend_Alpha0_5_ReturnsMiddle()
        {
            // Arrange
            var background = CreateSolidImage(10, 10, 100);
            var foreground = CreateSolidImage(10, 10, 200);

            // Act
            using var result = PixelBlender.AlphaBlend(background, foreground, alpha: 0.5f);

            // Assert: foreground * 0.5 + background * 0.5 = 200 * 0.5 + 100 * 0.5 = 150
            Assert.Equal(150, result[0, 0].PackedValue);
            Assert.Equal(150, result[5, 5].PackedValue);

            // Cleanup
            background.Dispose();
            foreground.Dispose();
        }

        #endregion

        #region AdditiveBlend Tests

        [Fact]
        public void AdditiveBlend_NoOverflow_ReturnsSum()
        {
            // Arrange
            var base1 = CreateSolidImage(10, 10, 50);
            var add = CreateSolidImage(10, 10, 60);

            // Act
            using var result = PixelBlender.AdditiveBlend(base1, add);

            // Assert: 50 + 60 = 110
            Assert.Equal(110, result[0, 0].PackedValue);
            Assert.Equal(110, result[5, 5].PackedValue);

            // Cleanup
            base1.Dispose();
            add.Dispose();
        }

        [Fact]
        public void AdditiveBlend_Overflow_ClipsAt255()
        {
            // Arrange
            var base1 = CreateSolidImage(10, 10, 200);
            var add = CreateSolidImage(10, 10, 100);

            // Act
            using var result = PixelBlender.AdditiveBlend(base1, add);

            // Assert: 200 + 100 = 300, but clipped at 255
            Assert.Equal(255, result[0, 0].PackedValue);
            Assert.Equal(255, result[5, 5].PackedValue);

            // Cleanup
            base1.Dispose();
            add.Dispose();
        }

        #endregion

        #region MultiplyBlend Tests

        [Fact]
        public void MultiplyBlend_ProducesDarkerResult()
        {
            // Arrange
            var base1 = CreateSolidImage(10, 10, 200);
            var multiply = CreateSolidImage(10, 10, 100);

            // Act
            using var result = PixelBlender.MultiplyBlend(base1, multiply);

            // Assert: (200 * 100) / 255 = 78.43... ≈ 78
            Assert.Equal(78, result[0, 0].PackedValue);
            Assert.Equal(78, result[5, 5].PackedValue);

            // Cleanup
            base1.Dispose();
            multiply.Dispose();
        }

        [Fact]
        public void MultiplyBlend_BlackPixel_ReturnsBlack()
        {
            // Arrange
            var base1 = CreateSolidImage(10, 10, 200);
            var multiply = CreateSolidImage(10, 10, 0); // Black

            // Act
            using var result = PixelBlender.MultiplyBlend(base1, multiply);

            // Assert: (200 * 0) / 255 = 0
            Assert.Equal(0, result[0, 0].PackedValue);
            Assert.Equal(0, result[5, 5].PackedValue);

            // Cleanup
            base1.Dispose();
            multiply.Dispose();
        }

        [Fact]
        public void MultiplyBlend_WhitePixel_PreservesBase()
        {
            // Arrange
            var base1 = CreateSolidImage(10, 10, 150);
            var multiply = CreateSolidImage(10, 10, 255); // White

            // Act
            using var result = PixelBlender.MultiplyBlend(base1, multiply);

            // Assert: (150 * 255) / 255 = 150
            Assert.Equal(150, result[0, 0].PackedValue);
            Assert.Equal(150, result[5, 5].PackedValue);

            // Cleanup
            base1.Dispose();
            multiply.Dispose();
        }

        #endregion

        #region ScreenBlend Tests

        [Fact]
        public void ScreenBlend_ProducesLighterResult()
        {
            // Arrange
            var base1 = CreateSolidImage(10, 10, 100);
            var screen = CreateSolidImage(10, 10, 100);

            // Act
            using var result = PixelBlender.ScreenBlend(base1, screen);

            // Assert: 255 - ((255 - 100) * (255 - 100)) / 255 = 255 - (155 * 155) / 255 = 255 - 94.22... ≈ 161
            Assert.Equal(161, result[0, 0].PackedValue);
            Assert.Equal(161, result[5, 5].PackedValue);

            // Cleanup
            base1.Dispose();
            screen.Dispose();
        }

        [Fact]
        public void ScreenBlend_WhitePixel_ReturnsWhite()
        {
            // Arrange
            var base1 = CreateSolidImage(10, 10, 100);
            var screen = CreateSolidImage(10, 10, 255); // White

            // Act
            using var result = PixelBlender.ScreenBlend(base1, screen);

            // Assert: 255 - ((255 - 100) * (255 - 255)) / 255 = 255 - 0 = 255
            Assert.Equal(255, result[0, 0].PackedValue);
            Assert.Equal(255, result[5, 5].PackedValue);

            // Cleanup
            base1.Dispose();
            screen.Dispose();
        }

        #endregion

        #region OverlayBlend Tests

        [Fact]
        public void OverlayBlend_DarkTones_UsesMultiply()
        {
            // Arrange
            var base1 = CreateSolidImage(10, 10, 64); // Dark tone (< 128)
            var overlay = CreateSolidImage(10, 10, 100);

            // Act
            using var result = PixelBlender.OverlayBlend(base1, overlay);

            // Assert: Dark tone uses multiply mode (2 * 64 * 100) / 255 = 50.19... ≈ 50
            Assert.Equal(50, result[0, 0].PackedValue);
            Assert.Equal(50, result[5, 5].PackedValue);

            // Cleanup
            base1.Dispose();
            overlay.Dispose();
        }

        [Fact]
        public void OverlayBlend_LightTones_UsesScreen()
        {
            // Arrange
            var base1 = CreateSolidImage(10, 10, 200); // Light tone (>= 128)
            var overlay = CreateSolidImage(10, 10, 100);

            // Act
            using var result = PixelBlender.OverlayBlend(base1, overlay);

            // Assert: Light tone uses screen mode (255 - 2 * (255 - 200) * (255 - 100) / 255)
            // = 255 - 2 * 55 * 155 / 255 = 255 - 66.86... ≈ 189
            Assert.Equal(189, result[0, 0].PackedValue);
            Assert.Equal(189, result[5, 5].PackedValue);

            // Cleanup
            base1.Dispose();
            overlay.Dispose();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Creates a solid-color test image.
        /// </summary>
        private Image<L8> CreateSolidImage(int width, int height, byte value)
        {
            var image = new Image<L8>(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    image[x, y] = new L8(value);
                }
            }

            return image;
        }

        #endregion
    }
}
