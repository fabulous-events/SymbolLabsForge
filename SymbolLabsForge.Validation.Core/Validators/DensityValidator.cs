//===============================================================
// File: DensityValidator.cs
// Author: Gemini (Original), Claude (Phase 8.3 - Generic Validator)
// Date: 2025-11-14
// Purpose: Generic density validator for any image container.
//
// PHASE 8.3: MODULARIZATION - VALIDATION FRAMEWORK
//   - Converted from non-generic to generic validator
//   - Works with IImageContainer<TMetadata, TMetrics> where TMetrics : IDensityMetrics
//   - Decouples from SymbolCapsule, enabling reuse across projects
//
// VALIDATION LOGIC:
//   - Counts ink pixels (pixel value < 128)
//   - Calculates density fraction (0.0-1.0) and percentage (0-100)
//   - Compares against configurable min/max thresholds
//   - Sets DensityStatus enum (TooLow, Valid, TooHigh)
//
// AUDIENCE: Graduate / PhD (generic programming, constraint-based validation)
//===============================================================
#nullable enable

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SymbolLabsForge.Validation.Contracts;
using SymbolLabsForge.ImageProcessing.Utilities;
using Microsoft.Extensions.Options;

namespace SymbolLabsForge.Validation.Core.Validators
{
    /// <summary>
    /// Settings for DensityValidator configuration.
    /// </summary>
    public class DensityValidatorSettings
    {
        /// <summary>
        /// Minimum density threshold as a fraction (0.0-1.0).
        /// Example: 0.03 = 3% ink pixels minimum.
        /// </summary>
        public float MinDensityThreshold { get; set; } = 0.03f;

        /// <summary>
        /// Maximum density threshold as a fraction (0.0-1.0).
        /// Example: 0.50 = 50% ink pixels maximum.
        /// </summary>
        public float MaxDensityThreshold { get; set; } = 0.50f;
    }

    /// <summary>
    /// Generic density validator for image containers.
    /// Validates pixel density against configurable thresholds.
    /// </summary>
    /// <typeparam name="TMetadata">Metadata type (unused by this validator)</typeparam>
    /// <typeparam name="TMetrics">Metrics type that implements IDensityMetrics</typeparam>
    public class DensityValidator<TMetadata, TMetrics> : IValidator<TMetadata, TMetrics>
        where TMetrics : IDensityMetrics
    {
        public string Name => "Density Validator";

        private readonly float _minDensityThreshold;
        private readonly float _maxDensityThreshold;

        /// <summary>
        /// Initializes a new instance of DensityValidator with configuration.
        /// </summary>
        /// <param name="options">Configuration settings for density thresholds</param>
        public DensityValidator(IOptions<DensityValidatorSettings> options)
        {
            _minDensityThreshold = options.Value.MinDensityThreshold;
            _maxDensityThreshold = options.Value.MaxDensityThreshold;
        }

        /// <summary>
        /// Validates the pixel density of the image container.
        /// </summary>
        /// <param name="container">Image container with image, metadata, and metrics</param>
        /// <param name="metrics">Metrics object where density values will be stored</param>
        /// <returns>ValidationResult indicating pass/fail with narratable error message</returns>
        public ValidationResult Validate(IImageContainer<TMetadata, TMetrics>? container, TMetrics metrics)
        {
            if (container == null || container.Image == null)
            {
                return new ValidationResult(false, Name, "Container or its image cannot be null.");
            }

            int blackPixelCount = 0;
            int totalPixels = container.Image.Width * container.Image.Height;

            if (totalPixels == 0)
            {
                metrics.DensityStatus = DensityStatus.TooLow;
                return new ValidationResult(false, Name, "Image has zero pixels.");
            }

            // Count ink pixels using ProcessPixelRows for performance
            container.Image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    foreach (var pixel in accessor.GetRowSpan(y))
                    {
                        if (PixelUtils.IsInk(pixel.PackedValue))
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

            // Calculate and store density in both representations (fraction and percentage)
            float densityFraction = blackPixelCount / (float)totalPixels;
            metrics.DensityFraction = densityFraction;
            metrics.DensityPercent = densityFraction * 100.0;

            // Compare using fraction (0.0-1.0) for threshold logic
            if (densityFraction < _minDensityThreshold)
            {
                metrics.DensityStatus = DensityStatus.TooLow;
                return new ValidationResult(false, Name,
                    $"Density of {metrics.DensityPercent:F2}% is below the {_minDensityThreshold * 100}% threshold.");
            }

            if (densityFraction > _maxDensityThreshold)
            {
                metrics.DensityStatus = DensityStatus.TooHigh;
                return new ValidationResult(false, Name,
                    $"Density of {metrics.DensityPercent:F2}% is above the {_maxDensityThreshold * 100}% threshold.");
            }

            metrics.DensityStatus = DensityStatus.Valid;
            return new ValidationResult(true, Name);
        }
    }
}
