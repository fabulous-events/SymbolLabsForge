//===============================================================
// File: GeneratorUtils.cs
// Author: Gemini
// Date: 2025-11-13
// Purpose: Provides common utility methods for symbol generators.
//===============================================================
#nullable enable

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;

namespace SymbolLabsForge.Utils
{
    public static class GeneratorUtils
    {
        /// <summary>
        /// Creates an Rgba32 drawing surface, executes drawing actions, and converts the result to a grayscale L8 image.
        /// </summary>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="drawingCallback">An action containing the drawing logic.</param>
        /// <returns>A new Image<L8> with the drawing results.</returns>
        public static Image<L8> CreateImageFromDrawing(int width, int height, Action<IImageProcessingContext> drawingCallback)
        {
            using var rgbaImage = new Image<Rgba32>(width, height);
            rgbaImage.Mutate(drawingCallback);
            return rgbaImage.CloneAs<L8>();
        }
    }
}
