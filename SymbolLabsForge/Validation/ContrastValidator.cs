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
        private const float MinPixelRatioThreshold = 0.1f; // 10%

        public ValidationResult Validate(SymbolCapsule? capsule, QualityMetrics metrics)
        {
            if (capsule == null || capsule.TemplateImage == null)
            {
                return new ValidationResult(false, Name, "Capsule or its image cannot be null.");
            }

            int totalPixels = capsule.TemplateImage.Width * capsule.TemplateImage.Height;
            if (totalPixels == 0)
            {
                return new ValidationResult(false, Name, "Image has zero pixels.");
            }

            int darkPixels = 0;
            capsule.TemplateImage.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    foreach (var pixel in accessor.GetRowSpan(y))
                    {
                        if (pixel.PackedValue < 128)
                        {
                            darkPixels++;
                        }
                    }
                }
            });

            float darkRatio = (float)darkPixels / totalPixels;
            float lightRatio = 1 - darkRatio;

            if (darkRatio < MinPixelRatioThreshold)
            {
                return new ValidationResult(false, Name, $"Image lacks dark pixels. Dark pixel ratio ({darkRatio:P1}) is below the required threshold of {MinPixelRatioThreshold:P1}.");
            }

            if (lightRatio < MinPixelRatioThreshold)
            {
                return new ValidationResult(false, Name, $"Image lacks light pixels. Light pixel ratio ({lightRatio:P1}) is below the required threshold of {MinPixelRatioThreshold:P1}.");
            }

            return new ValidationResult(true, Name);
        }
    }
}
