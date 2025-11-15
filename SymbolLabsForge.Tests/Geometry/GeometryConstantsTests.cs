//===============================================================
// File: GeometryConstantsTests.cs
// Author: Claude (Phase II-Tests)
// Date: 2025-11-14
// Purpose: Validates all geometry constants are within valid ranges.
//          Ensures no invalid values are introduced during future updates.
//
// TEACHING VALUE:
//   - Demonstrates fail-fast validation for centralized constants
//   - Shows how to prevent invalid geometry at compile/test time
//   - Provides regression protection for constant modifications
//
// AUDIENCE: Undergraduate / Graduate
//===============================================================
#nullable enable

using Xunit;

namespace SymbolLabsForge.Tests.Geometry
{
    /// <summary>
    /// Validates that all geometry constants defined in GeometryConstants.cs
    /// are within valid ranges (0.0 to 1.0 for ratios, logical ordering preserved).
    /// </summary>
    public class GeometryConstantsTests
    {
        [Fact]
        public void Flat_GeometryConstants_AreValid()
        {
            // Act & Assert
            Assert.True(GeometryConstants.Flat.AreValid(),
                "Flat geometry constants failed validation. Check GeometryConstants.Flat for invalid values.");
        }

        [Fact]
        public void Flat_StemConstants_AreWithinBounds()
        {
            // Assert: All stem constants should be ratios in [0, 1]
            Assert.InRange(GeometryConstants.Flat.StemLeftX, 0.0f, 1.0f);
            Assert.InRange(GeometryConstants.Flat.StemWidth, 0.0f, 1.0f);
            Assert.InRange(GeometryConstants.Flat.StemTopY, 0.0f, 1.0f);
            Assert.InRange(GeometryConstants.Flat.StemBottomY, 0.0f, 1.0f);

            // Assert: Logical ordering
            Assert.True(GeometryConstants.Flat.StemTopY < GeometryConstants.Flat.StemBottomY,
                "Stem top must be above stem bottom");
            Assert.True(GeometryConstants.Flat.StemWidth > 0,
                "Stem width must be positive");
        }

        [Fact]
        public void Flat_BowlConstants_AreWithinBounds()
        {
            // Assert: Bowl constants should be ratios in [0, 1]
            Assert.InRange(GeometryConstants.Flat.BowlCenterX, 0.0f, 1.0f);
            Assert.InRange(GeometryConstants.Flat.BowlCenterY, 0.0f, 1.0f);
            Assert.InRange(GeometryConstants.Flat.BowlRadiusX, 0.0f, 1.0f);
            Assert.InRange(GeometryConstants.Flat.BowlRadiusY, 0.0f, 1.0f);

            // Assert: Radii must be positive
            Assert.True(GeometryConstants.Flat.BowlRadiusX > 0,
                "Bowl horizontal radius must be positive");
            Assert.True(GeometryConstants.Flat.BowlRadiusY > 0,
                "Bowl vertical radius must be positive");
        }

        [Fact]
        public void Sharp_GeometryConstants_AreValid()
        {
            // Act & Assert
            Assert.True(GeometryConstants.Sharp.AreValid(),
                "Sharp geometry constants failed validation. Check GeometryConstants.Sharp for invalid values.");
        }

        [Fact]
        public void Sharp_VerticalStemsAreOrdered()
        {
            // Assert: Left stem comes before right stem
            Assert.True(GeometryConstants.Sharp.LeftStemLeftX < GeometryConstants.Sharp.LeftStemRightX,
                "Left stem left edge must be before right edge");
            Assert.True(GeometryConstants.Sharp.LeftStemRightX < GeometryConstants.Sharp.RightStemLeftX,
                "Left stem must be entirely before right stem");
            Assert.True(GeometryConstants.Sharp.RightStemLeftX < GeometryConstants.Sharp.RightStemRightX,
                "Right stem left edge must be before right edge");
        }

        [Fact]
        public void Sharp_CrossbarsAreOrdered()
        {
            // Assert: Crossbars have correct top-to-bottom ordering
            Assert.True(GeometryConstants.Sharp.TopCrossbarTopY < GeometryConstants.Sharp.TopCrossbarBottomY,
                "Top crossbar top must be above bottom");
            Assert.True(GeometryConstants.Sharp.TopCrossbarBottomY < GeometryConstants.Sharp.BottomCrossbarTopY,
                "Top crossbar must be entirely above bottom crossbar");
            Assert.True(GeometryConstants.Sharp.BottomCrossbarTopY < GeometryConstants.Sharp.BottomCrossbarBottomY,
                "Bottom crossbar top must be above bottom");
        }

        [Fact]
        public void Natural_GeometryConstants_AreValid()
        {
            // Act & Assert
            Assert.True(GeometryConstants.Natural.AreValid(),
                "Natural geometry constants failed validation. Check GeometryConstants.Natural for invalid values.");
        }

        [Fact]
        public void Natural_VerticalStemsAreOrdered()
        {
            // Assert: Left stem comes before right stem (same as Sharp)
            Assert.True(GeometryConstants.Natural.LeftStemLeftX < GeometryConstants.Natural.LeftStemRightX,
                "Left stem left edge must be before right edge");
            Assert.True(GeometryConstants.Natural.LeftStemRightX < GeometryConstants.Natural.RightStemLeftX,
                "Left stem must be entirely before right stem");
            Assert.True(GeometryConstants.Natural.RightStemLeftX < GeometryConstants.Natural.RightStemRightX,
                "Right stem left edge must be before right edge");
        }

        [Fact]
        public void DoubleSharp_GeometryConstants_AreValid()
        {
            // Act & Assert
            Assert.True(GeometryConstants.DoubleSharp.AreValid(),
                "DoubleSharp geometry constants failed validation. Check GeometryConstants.DoubleSharp for invalid values.");
        }

        [Fact]
        public void DoubleSharp_AllCoordinatesAreWithinBounds()
        {
            // Assert: All coordinates should be in [0, 1]
            Assert.InRange(GeometryConstants.DoubleSharp.Stroke1TopLeftX, 0.0f, 1.0f);
            Assert.InRange(GeometryConstants.DoubleSharp.Stroke1TopLeftY, 0.0f, 1.0f);
            Assert.InRange(GeometryConstants.DoubleSharp.Stroke2BottomLeftX, 0.0f, 1.0f);
            Assert.InRange(GeometryConstants.DoubleSharp.Stroke2BottomLeftY, 0.0f, 1.0f);
        }

        [Fact]
        public void Common_GeometryConstants_AreValid()
        {
            // Act & Assert
            Assert.True(GeometryConstants.Common.AreValid(),
                "Common geometry constants failed validation. Check GeometryConstants.Common for invalid values.");
        }

        [Fact]
        public void Common_StackedComponentGapRatio_IsReasonable()
        {
            // Assert: Gap should be small but positive (typically 1-5% of image height)
            Assert.InRange(GeometryConstants.Common.StackedComponentGapRatio, 0.001f, 0.1f);
        }

        [Fact]
        public void Common_StackedComponentSizeRatio_IsReasonable()
        {
            // Assert: Component size should be between 10% and 50% of total dimension
            Assert.InRange(GeometryConstants.Common.StackedComponentSizeRatio, 0.1f, 0.5f);
        }
    }
}
