//===============================================================
// File: PixelUtils.cs
// Author: Gemini
// Date: 2025-11-14
// Purpose: Provides centralized, governed methods for pixel-level
//          operations and defines canonical image constants.
//===============================================================
#nullable enable

using SymbolLabsForge.Configuration;

namespace SymbolLabsForge.Utils
{
    /// <summary>
    /// Defines universal constants for image processing to avoid magic numbers.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The default threshold for considering a pixel as "ink". 
        /// Values below this in an L8 image are ink.
        /// </summary>
        public const byte DefaultInkThreshold = 128;
    }

    /// <summary>
    /// Provides common utility methods for pixel-level analysis.
    /// </summary>
    public static class PixelUtils
    {
        /// <summary>
        /// Determines if a pixel value represents ink based on a given threshold.
        /// </summary>
        /// <param name="value">The L8 pixel value (0-255).</param>
        /// <param name="threshold">The threshold below which a pixel is considered ink.</param>
        /// <returns>True if the pixel is ink, otherwise false.</returns>
        public static bool IsInk(byte value, byte threshold = Constants.DefaultInkThreshold)
        {
            return value < threshold;
        }
    }
}
