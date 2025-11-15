//===============================================================
// File: GeneratorUtils.cs
// Author: Gemini (Original), Claude (Phase 8.8 - Extracted to ImageProcessing.Utilities)
// Date: 2025-11-15
// Purpose: Common utility methods for procedural symbol generation.
//
// PHASE 8.8: MODULARIZATION - UTILITY EXTRACTION
//   - Extracted from SymbolLabsForge.Utils to ImageProcessing.Utilities
//   - Provides drawing surface abstraction for symbol generators
//   - Simplifies RGBA → L8 conversion workflow
//
// WHY THIS MATTERS:
//   - Symbol generators need to draw complex shapes (stems, noteheads, beams)
//   - Drawing APIs work with RGBA color space
//   - OMR pipeline operates on L8 grayscale images
//   - This utility bridges the gap with a clean, testable interface
//
// TEACHING VALUE:
//   - Undergraduate: Color space conversions (RGBA → L8)
//   - Graduate: API design (callback pattern for flexibility)
//   - Demonstrates separation of concerns (drawing vs. processing)
//
// USAGE EXAMPLE:
//   var noteheadImage = GeneratorUtils.CreateImageFromDrawing(50, 50, ctx => {
//       ctx.Fill(Color.White); // Background
//       ctx.Fill(Color.Black, new EllipsePolygon(25, 25, 20, 15)); // Notehead
//   });
//
// AUDIENCE: Undergraduate / Graduate
//===============================================================
#nullable enable

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;

namespace SymbolLabsForge.ImageProcessing.Utilities
{
    /// <summary>
    /// Utility methods for procedural symbol generation.
    /// Provides drawing surface abstraction for creating L8 images from vector drawing commands.
    /// </summary>
    public static class GeneratorUtils
    {
        /// <summary>
        /// Creates an RGBA32 drawing surface, executes drawing actions, and converts the result to a grayscale L8 image.
        /// </summary>
        /// <param name="width">The width of the image in pixels.</param>
        /// <param name="height">The height of the image in pixels.</param>
        /// <param name="drawingCallback">An action containing the drawing logic (e.g., Fill, DrawLines, DrawPolygon).</param>
        /// <returns>A new Image&lt;L8&gt; with the drawing results converted to grayscale.</returns>
        /// <remarks>
        /// <para><b>Why RGBA → L8 Conversion?</b></para>
        /// <para>ImageSharp's drawing APIs (Fill, DrawLines, etc.) operate on RGBA color spaces.
        /// However, OMR processing pipelines work exclusively with grayscale L8 images for efficiency.
        /// This method provides a clean abstraction for the conversion workflow.</para>
        ///
        /// <para><b>Conversion Details:</b></para>
        /// <para>CloneAs&lt;L8&gt;() uses standard luminance formula: L = 0.299*R + 0.587*G + 0.114*B</para>
        /// <para>For black/white drawings, this effectively maps: Black → 0, White → 255</para>
        ///
        /// <para><b>Teaching Note:</b></para>
        /// <para>The callback pattern (Action&lt;IImageProcessingContext&gt;) allows callers to compose
        /// arbitrary drawing commands without this utility needing to know about specific shapes.
        /// This is a foundational pattern in API design (Strategy Pattern / Dependency Inversion).</para>
        /// </remarks>
        public static Image<L8> CreateImageFromDrawing(int width, int height, Action<IImageProcessingContext> drawingCallback)
        {
            using var rgbaImage = new Image<Rgba32>(width, height);
            rgbaImage.Mutate(drawingCallback);
            return rgbaImage.CloneAs<L8>();
        }
    }
}
