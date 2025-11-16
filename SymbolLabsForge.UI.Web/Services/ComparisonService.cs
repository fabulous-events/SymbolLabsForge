//===============================================================
// File: ComparisonService.cs
// Author: Claude (Phase 10.1 - Blazor Server Scaffolding)
// Date: 2025-11-15
// Purpose: Service layer for symbol comparison workflow.
//
// PHASE 10.1: COMPARISON SERVICE
//   - Orchestrates comparison workflow (load canonical, compare, generate diff)
//   - Delegates to SnapshotComparer (Phase 9.1) and ImageDiffGenerator (Phase 9.1)
//   - Handles file I/O and path construction
//   - Validates uploaded images (blank detection, format validation)
//
// WHY THIS MATTERS:
//   - Demonstrates clean service layer architecture
//   - Separates UI concerns from comparison logic
//   - Enables unit testing of comparison workflow
//   - Students learn DI patterns and service orchestration
//
// TEACHING VALUE:
//   - Undergraduate: Service layer pattern, file I/O, validation
//   - Graduate: DI lifetime management, error handling strategies
//   - PhD: Performance optimization, caching strategies
//
// AUDIENCE: Undergraduate / Graduate (service architecture)
//===============================================================
#nullable enable

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SymbolLabsForge.Testing.Utilities;
using Microsoft.Extensions.Logging;

namespace SymbolLabsForge.UI.Web.Services
{
    /// <summary>
    /// Service for comparing uploaded symbols against canonical outputs.
    /// </summary>
    /// <remarks>
    /// <para><b>Phase 10.1: Service Layer Pattern</b></para>
    /// <para>This service orchestrates the comparison workflow:</para>
    /// <list type="number">
    /// <item>Load canonical symbol from SymbolGenerationService</item>
    /// <item>Validate uploaded image (blank detection, format check)</item>
    /// <item>Compare using SnapshotComparer.AreSimilar() (Phase 9.1)</item>
    /// <item>Generate diff image using ImageDiffGenerator.SaveDiff() (Phase 9.1)</item>
    /// <item>Return ComparisonResult with statistics</item>
    /// </list>
    /// </remarks>
    public class ComparisonService
    {
        private readonly SymbolGenerationService _symbolGenerationService;
        private readonly ILogger<ComparisonService> _logger;
        private readonly string _diffsDirectory;
        private readonly string _uploadsDirectory;

        public ComparisonService(
            SymbolGenerationService symbolGenerationService,
            ILogger<ComparisonService> logger,
            IWebHostEnvironment environment)
        {
            _symbolGenerationService = symbolGenerationService;
            _logger = logger;

            // Set up directories for uploads and diffs
            _uploadsDirectory = Path.Combine(environment.WebRootPath, "uploads");
            _diffsDirectory = Path.Combine(environment.WebRootPath, "diffs");

            // Ensure directories exist
            Directory.CreateDirectory(_uploadsDirectory);
            Directory.CreateDirectory(_diffsDirectory);
        }

        /// <summary>
        /// Compares an uploaded image against a canonical symbol.
        /// </summary>
        /// <param name="uploadedImage">The student's uploaded image.</param>
        /// <param name="symbolType">Which canonical symbol to compare against.</param>
        /// <param name="tolerance">Tolerance for comparison (0.0 = exact, 0.01 = 1% difference allowed).</param>
        /// <returns>Comparison result with similarity score, statistics, and diff image path.</returns>
        public async Task<ComparisonResult> CompareSymbolAsync(
            Image<L8> uploadedImage,
            SymbolType symbolType,
            double tolerance)
        {
            _logger.LogInformation("Starting comparison: {SymbolType}, tolerance: {Tolerance}",
                symbolType, tolerance);

            try
            {
                // Step 1: Validate uploaded image
                var validation = ValidateUploadedImage(uploadedImage);
                if (!validation.IsValid)
                {
                    return ComparisonResult.Failure(validation.ErrorMessage!);
                }

                // Step 2: Load canonical symbol from generator
                var canonical = await _symbolGenerationService.GetCanonicalSymbolAsync(symbolType);

                // Step 3: Check dimension compatibility
                if (uploadedImage.Width != canonical.Width || uploadedImage.Height != canonical.Height)
                {
                    _logger.LogWarning("Size mismatch: uploaded {UploadedSize}, canonical {CanonicalSize}",
                        $"{uploadedImage.Width}×{uploadedImage.Height}",
                        $"{canonical.Width}×{canonical.Height}");

                    return ComparisonResult.Failure(
                        $"Size mismatch: Your symbol is {uploadedImage.Width}×{uploadedImage.Height}, but canonical is {canonical.Width}×{canonical.Height}. " +
                        $"Please resize or generate a new symbol with matching dimensions.");
                }

                // Step 4: Compare using SnapshotComparer (Phase 9.1)
                bool areSimilar = SnapshotComparer.AreSimilar(canonical, uploadedImage, tolerance);

                // Step 5: Calculate statistics
                var stats = CalculateStatistics(canonical, uploadedImage);

                // Step 6: Generate diff image (Phase 9.1)
                var diffFileName = $"diff_{symbolType}_{DateTime.UtcNow:yyyyMMddHHmmss}.png";
                var diffPath = Path.Combine(_diffsDirectory, diffFileName);

                // ImageDiffGenerator.SaveDiff writes directly to file (Phase 9.1)
                ImageDiffGenerator.SaveDiff(canonical, uploadedImage, diffPath);

                _logger.LogInformation("Comparison complete: {Result}, similarity: {Similarity}%",
                    areSimilar ? "PASS" : "FAIL", stats.SimilarityPercent);

                return new ComparisonResult
                {
                    Similar = areSimilar,
                    SimilarityPercent = stats.SimilarityPercent,
                    DifferencePercent = stats.DifferencePercent,
                    DiffImagePath = $"/diffs/{diffFileName}",
                    Statistics = stats,
                    ErrorMessage = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Comparison failed for {SymbolType}", symbolType);
                return ComparisonResult.Failure($"Comparison failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates uploaded image for common issues.
        /// </summary>
        private ValidationResult ValidateUploadedImage(Image<L8> image)
        {
            // Check minimum size
            if (image.Width < 5 || image.Height < 5)
            {
                return ValidationResult.Failure(
                    $"Image too small: {image.Width}×{image.Height}. Minimum size: 5×5 pixels.");
            }

            // Check if image is completely blank (all pixels same value)
            if (IsImageBlank(image))
            {
                return ValidationResult.Failure(
                    "Image appears to be blank (all pixels are identical). " +
                    "Please upload a valid symbol or use the Synthetic Symbol Generator.");
            }

            return ValidationResult.Success();
        }

        /// <summary>
        /// Checks if image is completely blank (all pixels have same value).
        /// </summary>
        /// <remarks>
        /// <para><b>Teaching Value:</b></para>
        /// <para>Students learn pixel-by-pixel analysis and early exit optimization.</para>
        /// <para>This prevents silent failures when students upload blank/invalid images.</para>
        /// </remarks>
        private bool IsImageBlank(Image<L8> image)
        {
            byte firstPixel = image[0, 0].PackedValue;

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    if (image[x, y].PackedValue != firstPixel)
                    {
                        return false; // Found variation, not blank
                    }
                }
            }

            return true; // All pixels identical = blank
        }

        /// <summary>
        /// Calculates comparison statistics.
        /// </summary>
        private ComparisonStatistics CalculateStatistics(Image<L8> expected, Image<L8> actual)
        {
            int totalPixels = expected.Width * expected.Height;
            int differentPixels = 0;
            int maxError = 0;
            long sumError = 0;

            for (int y = 0; y < expected.Height; y++)
            {
                for (int x = 0; x < expected.Width; x++)
                {
                    byte expectedPixel = expected[x, y].PackedValue;
                    byte actualPixel = actual[x, y].PackedValue;

                    int error = Math.Abs(expectedPixel - actualPixel);

                    if (error > 0)
                    {
                        differentPixels++;
                        sumError += error;
                        maxError = Math.Max(maxError, error);
                    }
                }
            }

            double meanError = differentPixels > 0 ? (double)sumError / differentPixels : 0.0;
            double differencePercent = (double)differentPixels / totalPixels * 100.0;
            double similarityPercent = 100.0 - differencePercent;

            return new ComparisonStatistics
            {
                TotalPixels = totalPixels,
                DifferentPixels = differentPixels,
                MaxError = maxError,
                MeanError = meanError,
                DifferencePercent = differencePercent,
                SimilarityPercent = similarityPercent
            };
        }
    }

    /// <summary>
    /// Result of symbol comparison operation.
    /// </summary>
    public class ComparisonResult
    {
        public bool Similar { get; init; }
        public double SimilarityPercent { get; init; }
        public double DifferencePercent { get; init; }
        public string? DiffImagePath { get; init; }
        public ComparisonStatistics? Statistics { get; init; }
        public string? ErrorMessage { get; init; }

        public static ComparisonResult Failure(string errorMessage)
        {
            return new ComparisonResult
            {
                Similar = false,
                SimilarityPercent = 0.0,
                DifferencePercent = 100.0,
                ErrorMessage = errorMessage
            };
        }
    }

    /// <summary>
    /// Detailed statistics from comparison operation.
    /// </summary>
    public class ComparisonStatistics
    {
        public int TotalPixels { get; init; }
        public int DifferentPixels { get; init; }
        public int MaxError { get; init; }          // 0-255 (max pixel intensity difference)
        public double MeanError { get; init; }      // Average pixel intensity difference
        public double DifferencePercent { get; init; }
        public double SimilarityPercent { get; init; }
    }

    /// <summary>
    /// Validation result for image uploads.
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; init; }
        public string? ErrorMessage { get; init; }

        public static ValidationResult Success() => new() { IsValid = true };
        public static ValidationResult Failure(string error) => new() { IsValid = false, ErrorMessage = error };
    }

    /// <summary>
    /// Symbol types supported for comparison.
    /// </summary>
    public enum SymbolType
    {
        Sharp,
        Flat,
        Natural,
        DoubleSharp,
        Treble
    }
}
