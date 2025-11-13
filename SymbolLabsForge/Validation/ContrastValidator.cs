//===============================================================
// File: ContrastValidator.cs
// Author: Gemini
// Date: 2025-11-12
// Purpose: Validates the contrast of a symbol image.
//===============================================================
#nullable enable

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SymbolLabsForge.Contracts;

namespace SymbolLabsForge.Validation
{
    public class ContrastValidator : IValidator
    {
        public string Name => "ContrastValidator";
        private const float ContrastThreshold = 0.5f; // 50% contrast

        public ValidationResult Validate(SymbolCapsule capsule, QualityMetrics metrics)
        {
            if (capsule?.TemplateImage == null)
            {
                return new ValidationResult(false, Name, "Input capsule or image is null.");
            }

            var image = capsule.TemplateImage;
            byte minPixel = 255;
            byte maxPixel = 0;

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    byte pixelValue = image[x, y].PackedValue;
                    if (pixelValue < minPixel) minPixel = pixelValue;
                    if (pixelValue > maxPixel) maxPixel = pixelValue;
                }
            }

            if (maxPixel == minPixel)
            {
                return new ValidationResult(false, Name, "Image has no contrast (all pixels are the same color).");
            }

            float contrast = (float)(maxPixel - minPixel) / 255f;

            if (contrast < ContrastThreshold)
            {
                return new ValidationResult(false, Name, $"Contrast ({contrast:P1}) is below the required threshold of {ContrastThreshold:P1}.");
            }

            return new ValidationResult(true, Name);
        }
    }
}
