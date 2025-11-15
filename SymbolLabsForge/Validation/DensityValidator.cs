using SymbolLabsForge.Contracts;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

using SymbolLabsForge.Utils;

namespace SymbolLabsForge.Validation
{
    public class DensityValidator : IValidator
    {
        public string Name => "Density Validator";

        private readonly float _minDensityThreshold;
        private readonly float _maxDensityThreshold;

        public DensityValidator(IOptions<DensityValidatorSettings> options)
        {
            _minDensityThreshold = options.Value.MinDensityThreshold;
            _maxDensityThreshold = options.Value.MaxDensityThreshold;
        }

        public ValidationResult Validate(SymbolCapsule? capsule, QualityMetrics metrics)
        {
            if (capsule == null || capsule.TemplateImage == null)
            {
                return new ValidationResult(false, Name, "Capsule or its image cannot be null.");
            }

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
                        if (PixelUtils.IsInk(pixel.PackedValue)) // Use centralized helper
                        {
                            blackPixelCount++;
                        }
                    }
                }
            });

            if (blackPixelCount == 0)
            {
                metrics.DensityStatus = DensityStatus.TooLow;
                return new ValidationResult(false, Name, "Image is completely white.");
            }

            float density = blackPixelCount / (float)totalPixels;
            metrics.Density = density * 100; // Store as percentage

            if (density < _minDensityThreshold)
            {
                metrics.DensityStatus = DensityStatus.TooLow;
                return new ValidationResult(false, Name, $"Density of {metrics.Density:F2}% is below the {_minDensityThreshold * 100}% threshold.");
            }

            if (density > _maxDensityThreshold)
            {
                metrics.DensityStatus = DensityStatus.TooHigh;
                return new ValidationResult(false, Name, $"Density of {metrics.Density:F2}% is above the {_maxDensityThreshold * 100}% threshold.");
            }

            metrics.DensityStatus = DensityStatus.Valid;
            return new ValidationResult(true, Name);
        }
    }
}
