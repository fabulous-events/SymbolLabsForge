using SymbolLabsForge.Contracts;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;

namespace SymbolLabsForge.Validation
{
    public class DensityValidator : IValidator
    {
        public string Name => "Density Validator";

        private const float MinDensityThreshold = 0.05f; // 5%
        private const float MaxDensityThreshold = 0.12f; // 12%

        public ValidationResult Validate(SymbolCapsule capsule, QualityMetrics metrics)
        {
            int blackPixelCount = 0;
            int totalPixels = capsule.TemplateImage.Width * capsule.TemplateImage.Height;

            if (totalPixels == 0)
            {
                metrics.DensityStatus = DensityStatus.TooLow;
                return new ValidationResult(false, Name, "Image has zero pixels.");
            }

            capsule.TemplateImage.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    foreach (var pixel in accessor.GetRowSpan(y))
                    {
                        if (pixel.PackedValue < 128) // Consider non-white pixels as "ink"
                        {
                            blackPixelCount++;
                        }
                    }
                }
            });

            float density = blackPixelCount / (float)totalPixels;
            metrics.Density = density * 100; // Store as percentage

            if (density < MinDensityThreshold)
            {
                metrics.DensityStatus = DensityStatus.TooLow;
                return new ValidationResult(false, Name, $"Density of {metrics.Density:F2}% is below the {MinDensityThreshold * 100}% threshold.");
            }

            if (density > MaxDensityThreshold)
            {
                metrics.DensityStatus = DensityStatus.TooHigh;
                return new ValidationResult(false, Name, $"Density of {metrics.Density:F2}% is above the {MaxDensityThreshold * 100}% threshold.");
            }

            metrics.DensityStatus = DensityStatus.Valid;
            return new ValidationResult(true, Name);
        }
    }
}
