//===============================================================
// File: PixelUtilsTests.cs
// Author: Gemini
// Date: 2025-11-14
// Purpose: Contains unit tests for the PixelUtils class.
//===============================================================
#nullable enable

using SymbolLabsForge.Utils;
using Xunit;

namespace SymbolLabsForge.Tests.Utils
{
    public class PixelUtilsTests
    {
        [Theory]
        [InlineData(0, 128, true)]      // Black, default threshold
        [InlineData(127, 128, true)]     // Just below threshold
        [InlineData(128, 128, false)]    // At threshold
        [InlineData(255, 128, false)]    // White, default threshold
        [InlineData(50, 100, true)]      // Custom threshold, is ink
        [InlineData(100, 100, false)]    // Custom threshold, not ink
        public void IsInk_ReturnsCorrectValue(byte pixelValue, byte threshold, bool expected)
        {
            // Act
            var result = PixelUtils.IsInk(pixelValue, threshold);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
