//===============================================================
// File: GeometryConstants.cs
// Author: Claude (Phase II Remediation)
// Date: 2025-11-14
// Purpose: Centralized geometry specifications for all symbol generators.
//          Eliminates magic numbers and provides a single source of truth
//          for symbol proportions, ensuring maintainability and testability.
//
// GOVERNANCE:
//   - All values are ratios (0.0 to 1.0) relative to image dimensions
//   - Changes to constants must be validated by multi-resolution tests
//   - Rationale for each constant is documented inline
//   - Values derived from musical typography standards where applicable
//===============================================================
#nullable enable

namespace SymbolLabsForge
{
    /// <summary>
    /// Centralized geometry constants for all symbol generators.
    /// All values are proportional ratios relative to image dimensions,
    /// ensuring symbols scale correctly across all resolutions.
    /// </summary>
    public static class GeometryConstants
    {
        /// <summary>
        /// Geometry specifications for the Flat (♭) symbol.
        /// Consists of a vertical stem and a curved bowl at the bottom.
        /// </summary>
        public static class Flat
        {
            // Stem geometry
            public const float StemLeftX = 0.4f;        // Left edge of vertical stem
            public const float StemWidth = 0.1f;        // Stem thickness (10% of width)
            public const float StemTopY = 0.1f;         // Top of stem (10% from top)
            public const float StemBottomY = 0.9f;      // Bottom of stem (10% from bottom)

            // Bowl geometry (ellipse)
            public const float BowlCenterX = 0.6f;      // Horizontal center of bowl (right of stem)
            public const float BowlCenterY = 0.75f;     // Vertical center of bowl (lower portion)
            public const float BowlRadiusX = 0.25f;     // Horizontal radius (creates backward curve)
            public const float BowlRadiusY = 0.2f;      // Vertical radius (height of bowl)

            /// <summary>
            /// Validates that all Flat geometry constants are within valid ranges.
            /// Called during unit tests to ensure no invalid values are introduced.
            /// </summary>
            public static bool AreValid()
            {
                return StemLeftX >= 0 && StemLeftX <= 1 &&
                       StemWidth > 0 && StemWidth <= 1 &&
                       StemTopY >= 0 && StemTopY < StemBottomY &&
                       StemBottomY <= 1 &&
                       BowlCenterX >= 0 && BowlCenterX <= 1 &&
                       BowlCenterY >= 0 && BowlCenterY <= 1 &&
                       BowlRadiusX > 0 && BowlRadiusX <= 1 &&
                       BowlRadiusY > 0 && BowlRadiusY <= 1;
            }
        }

        /// <summary>
        /// Geometry specifications for the Sharp (♯) symbol.
        /// Consists of two vertical lines crossed by two horizontal lines.
        /// </summary>
        public static class Sharp
        {
            // Vertical lines (two parallel stems)
            public const float LeftStemLeftX = 0.4f;       // Left edge of left vertical line
            public const float LeftStemRightX = 0.5f;      // Right edge of left vertical line
            public const float RightStemLeftX = 0.6f;      // Left edge of right vertical line
            public const float RightStemRightX = 0.7f;     // Right edge of right vertical line
            public const float StemTopY = 0.1f;            // Top of vertical lines
            public const float StemBottomY = 0.9f;         // Bottom of vertical lines

            // Horizontal crossbars
            public const float CrossbarLeftX = 0.2f;       // Left edge of crossbars
            public const float CrossbarRightX = 0.8f;      // Right edge of crossbars
            public const float TopCrossbarTopY = 0.4f;     // Top edge of upper crossbar
            public const float TopCrossbarBottomY = 0.5f;  // Bottom edge of upper crossbar
            public const float BottomCrossbarTopY = 0.7f;  // Top edge of lower crossbar
            public const float BottomCrossbarBottomY = 0.8f; // Bottom edge of lower crossbar

            /// <summary>
            /// Validates Sharp geometry constants.
            /// </summary>
            public static bool AreValid()
            {
                return LeftStemLeftX >= 0 && LeftStemLeftX < LeftStemRightX &&
                       LeftStemRightX < RightStemLeftX &&
                       RightStemLeftX < RightStemRightX && RightStemRightX <= 1 &&
                       StemTopY >= 0 && StemTopY < StemBottomY && StemBottomY <= 1 &&
                       CrossbarLeftX >= 0 && CrossbarLeftX < CrossbarRightX && CrossbarRightX <= 1 &&
                       TopCrossbarTopY >= 0 && TopCrossbarTopY < TopCrossbarBottomY &&
                       TopCrossbarBottomY < BottomCrossbarTopY &&
                       BottomCrossbarTopY < BottomCrossbarBottomY && BottomCrossbarBottomY <= 1;
            }
        }

        /// <summary>
        /// Geometry specifications for the Natural (♮) symbol.
        /// Similar to Sharp but with different vertical stem positioning.
        /// </summary>
        public static class Natural
        {
            // Vertical lines (positioned differently than Sharp)
            public const float LeftStemLeftX = 0.3f;       // Left edge of left vertical line
            public const float LeftStemRightX = 0.4f;      // Right edge of left vertical line
            public const float RightStemLeftX = 0.6f;      // Left edge of right vertical line
            public const float RightStemRightX = 0.7f;     // Right edge of right vertical line
            public const float StemTopY = 0.1f;            // Top of vertical lines
            public const float StemBottomY = 0.9f;         // Bottom of vertical lines

            // Horizontal crossbars (same as Sharp)
            public const float CrossbarLeftX = 0.2f;
            public const float CrossbarRightX = 0.8f;
            public const float TopCrossbarTopY = 0.4f;
            public const float TopCrossbarBottomY = 0.5f;
            public const float BottomCrossbarTopY = 0.7f;
            public const float BottomCrossbarBottomY = 0.8f;

            /// <summary>
            /// Validates Natural geometry constants.
            /// </summary>
            public static bool AreValid()
            {
                return LeftStemLeftX >= 0 && LeftStemLeftX < LeftStemRightX &&
                       LeftStemRightX < RightStemLeftX &&
                       RightStemLeftX < RightStemRightX && RightStemRightX <= 1 &&
                       StemTopY >= 0 && StemTopY < StemBottomY && StemBottomY <= 1 &&
                       CrossbarLeftX >= 0 && CrossbarLeftX < CrossbarRightX && CrossbarRightX <= 1 &&
                       TopCrossbarTopY >= 0 && TopCrossbarTopY < TopCrossbarBottomY &&
                       TopCrossbarBottomY < BottomCrossbarTopY &&
                       BottomCrossbarTopY < BottomCrossbarBottomY && BottomCrossbarBottomY <= 1;
            }
        }

        /// <summary>
        /// Geometry specifications for the DoubleSharp (𝄪) symbol.
        /// Rendered as an 'X' shape formed by two diagonal strokes.
        /// </summary>
        public static class DoubleSharp
        {
            // First diagonal stroke (top-left to bottom-right)
            public const float Stroke1TopLeftX = 0.2f;
            public const float Stroke1TopLeftY = 0.2f;
            public const float Stroke1TopRightX = 0.3f;
            public const float Stroke1TopRightY = 0.2f;
            public const float Stroke1BottomRightX = 0.8f;
            public const float Stroke1BottomRightY = 0.7f;
            public const float Stroke1BottomLeftX = 0.7f;
            public const float Stroke1BottomLeftY = 0.8f;

            // Second diagonal stroke (bottom-left to top-right)
            public const float Stroke2BottomLeftX = 0.2f;
            public const float Stroke2BottomLeftY = 0.7f;
            public const float Stroke2BottomRightX = 0.3f;
            public const float Stroke2BottomRightY = 0.8f;
            public const float Stroke2TopRightX = 0.8f;
            public const float Stroke2TopRightY = 0.3f;
            public const float Stroke2TopLeftX = 0.7f;
            public const float Stroke2TopLeftY = 0.2f;

            /// <summary>
            /// Validates DoubleSharp geometry constants.
            /// </summary>
            public static bool AreValid()
            {
                // Simplified validation - all values should be in [0, 1]
                return Stroke1TopLeftX >= 0 && Stroke1TopLeftX <= 1 &&
                       Stroke1TopLeftY >= 0 && Stroke1TopLeftY <= 1 &&
                       Stroke2BottomLeftX >= 0 && Stroke2BottomLeftX <= 1 &&
                       Stroke2BottomLeftY >= 0 && Stroke2BottomLeftY <= 1;
            }
        }

        /// <summary>
        /// Common spacing and sizing ratios used across multiple generators.
        /// PHASE II-E: Proportional spacing for stacked/composite symbols.
        /// </summary>
        public static class Common
        {
            /// <summary>
            /// Vertical gap between stacked components as a ratio of image height.
            /// Replaces fixed 4-pixel gap in StackedGenerator.
            /// Value: 0.02 = 2% of image height
            /// Example: 100px height = 2px gap, 256px height = 5.12px gap (scales correctly)
            /// </summary>
            public const float StackedComponentGapRatio = 0.02f;

            /// <summary>
            /// Default component size ratio for stacked symbols.
            /// Each component is 1/4 of the total dimension.
            /// </summary>
            public const float StackedComponentSizeRatio = 0.25f; // 1/4

            /// <summary>
            /// Validates common geometry constants.
            /// </summary>
            public static bool AreValid()
            {
                return StackedComponentGapRatio > 0 && StackedComponentGapRatio < 1 &&
                       StackedComponentSizeRatio > 0 && StackedComponentSizeRatio <= 1;
            }
        }
    }
}
