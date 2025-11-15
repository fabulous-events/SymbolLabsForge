//===============================================================
// File: ContrastValidator.cs
// Author: Gemini
// Date: 2025-11-12
// Updated: 2025-11-14 (Claude - Validator Redesign Phase 1A)
// Purpose: Validates the contrast of a symbol image.
//
// VALIDATOR REDESIGN PHASE 1A:
//   - Replaced hardcoded threshold (< 128) with centralized PixelUtils.IsInk()
//   - Ensures consistent pixel classification across entire validation layer
//===============================================================
#nullable enable

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SymbolLabsForge.Contracts;
using SymbolLabsForge.Utils;

namespace SymbolLabsForge.Validation
{
    /// <summary>
    /// LEGACY: Non-generic validator kept for reference only.
    /// Use ContrastValidatorAdapter (wraps generic ContrastValidator) instead.
    /// Will be removed in Phase 9.
    /// </summary>
    [Obsolete("Use ContrastValidatorAdapter instead. This class will be removed in Phase 9.")]
    internal class LegacyContrastValidator
    {
        public string Name => "Legacy ContrastValidator";
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
                        if (PixelUtils.IsInk(pixel.PackedValue))
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
