//===============================================================
// File: ContrastValidator.cs
// Author: Gemini (Original), Claude (Phase 8.3 - Generic Validator)
// Date: 2025-11-14
// Purpose: Generic contrast validator for any image container.
//
// PHASE 8.3: MODULARIZATION - VALIDATION FRAMEWORK
//   - Converted from non-generic to generic validator
//   - Works with IImageContainer<TMetadata, TMetrics>
//   - Decouples from SymbolCapsule, enabling reuse across projects
//
// VALIDATION LOGIC:
//   - Ensures image has balanced dark/light pixels (not all white or all black)
//   - Requires at least 10% dark pixels AND 10% light pixels
//   - Prevents degenerate cases (completely blank or completely filled)
//
// AUDIENCE: Graduate / PhD (generic programming, image quality metrics)
//===============================================================
#nullable enable

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SymbolLabsForge.Validation.Contracts;
using SymbolLabsForge.ImageProcessing.Utilities;

namespace SymbolLabsForge.Validation.Core.Validators
{
    /// <summary>
    /// Generic contrast validator for image containers.
    /// Validates that image has balanced dark and light pixels.
    /// </summary>
    /// <typeparam name="TMetadata">Metadata type (unused by this validator)</typeparam>
    /// <typeparam name="TMetrics">Metrics type (unused by this validator)</typeparam>
    public class ContrastValidator<TMetadata, TMetrics> : IValidator<TMetadata, TMetrics>
    {
        public string Name => "ContrastValidator";
        private const float MinPixelRatioThreshold = 0.1f; // 10%

        /// <summary>
        /// Validates the contrast of the image container.
        /// </summary>
        /// <param name="container">Image container with image, metadata, and metrics</param>
        /// <param name="metrics">Metrics object (not modified by this validator)</param>
        /// <returns>ValidationResult indicating pass/fail with narratable error message</returns>
        public ValidationResult Validate(IImageContainer<TMetadata, TMetrics>? container, TMetrics metrics)
        {
            if (container == null || container.Image == null)
            {
                return new ValidationResult(false, Name, "Container or its image cannot be null.");
            }

            int totalPixels = container.Image.Width * container.Image.Height;
            if (totalPixels == 0)
            {
                return new ValidationResult(false, Name, "Image has zero pixels.");
            }

            int darkPixels = 0;
            container.Image.ProcessPixelRows(accessor =>
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
                return new ValidationResult(false, Name,
                    $"Image lacks dark pixels. Dark pixel ratio ({darkRatio:P1}) is below the required threshold of {MinPixelRatioThreshold:P1}.");
            }

            if (lightRatio < MinPixelRatioThreshold)
            {
                return new ValidationResult(false, Name,
                    $"Image lacks light pixels. Light pixel ratio ({lightRatio:P1}) is below the required threshold of {MinPixelRatioThreshold:P1}.");
            }

            return new ValidationResult(true, Name);
        }
    }
}
