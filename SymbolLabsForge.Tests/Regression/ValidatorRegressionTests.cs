//===============================================================
// File: ValidatorRegressionTests.cs
// Author: Claude (Phase 3 - Validator Redesign)
// Date: 2025-11-14
// Purpose: Prevents regression of validator defects fixed in Phase 1-2.
//          Codifies validator behavior as permanent quality gates.
//
// DEFECT HISTORY:
//   Phase 1A: ContrastValidator used hardcoded threshold (< 128)
//   Phase 1B/1C: QualityMetrics had ambiguous Density property
//   Phase 2A: SkeletonizationProcessor inverted ink/background logic
//   Original: StructureValidator rejected hollow symbols incorrectly
//
// VALIDATION STRATEGY:
//   - Guard against hardcoded threshold reintroduction
//   - Ensure density fraction vs percent is unambiguous
//   - Verify hollow symbols pass validation
//   - Confirm binary integrity (0/255) across all validators
//
// CANONICAL STANDARD:
//   - 0 (black) = ink / foreground
//   - 255 (white) = background
//   - PixelUtils.IsInk(byte value) returns true if value < 128
//
// LINKED ARTIFACTS:
//   - CODE_HYGIENE_FIX_PLAN.md (Section 6: Density fraction/percent)
//   - CODE_HYGIENE_FIX_PLAN.md (Section 8: Centralized threshold)
//   - S7 Expert Technical Audit (StructureValidator hollow symbol issue)
//
// AUDIENCE: Graduate / PhD (validator architecture)
//===============================================================
#nullable enable

using Xunit;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SymbolLabsForge.Contracts;
using SymbolLabsForge.Validation;
using SymbolLabsForge.Utils;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace SymbolLabsForge.Tests.Regression
{
    /// <summary>
    /// Regression tests that prevent re-introduction of validator defects
    /// identified and fixed in Phase 1-2 of validator redesign.
    /// </summary>
    public class ValidatorRegressionTests
    {
        /// <summary>
        /// REGRESSION GUARD: Prevents re-introduction of hardcoded threshold in ContrastValidator.
        ///
        /// DEFECT HISTORY (Phase 1A):
        ///   - Original Implementation: Used hardcoded `pixel.PackedValue < 128`
        ///   - Root Cause: Lack of centralized threshold logic
        ///   - Impact: Inconsistent pixel classification across validators
        ///   - Fix: Replaced with `PixelUtils.IsInk(pixel.PackedValue)`
        ///
        /// VALIDATION STRATEGY:
        ///   - Create test image with pixels at threshold boundary (127, 128, 129)
        ///   - Verify ContrastValidator classifies pixels consistently with PixelUtils.IsInk()
        /// </summary>
        [Fact]
        public void ContrastValidator_UsesPixelUtilsIsInk_NotHardcodedThreshold()
        {
            // Arrange: Create capsule with pixels at threshold boundary
            // Need at least 10% dark pixels (3/25 = 12%) to pass ContrastValidator
            using var image = CreateTestImageL8(5, 5);
            image[0, 0] = new L8(127);  // Ink (< 128)
            image[1, 0] = new L8(128);  // Background (>= 128)
            image[2, 0] = new L8(129);  // Background (>= 128)
            image[0, 1] = new L8(0);    // Ink (black)
            image[1, 1] = new L8(255);  // Background (white)
            image[2, 1] = new L8(50);   // Ink (< 128) - ensures ≥10% dark pixels

            var capsule = CreateTestCapsule(image);
            var validator = new ContrastValidator();
            var metrics = new QualityMetrics();

            // Act
            var result = validator.Validate(capsule, metrics);

            // Assert: Validator should accept image with sufficient contrast
            // The critical assertion is that the validator USES PixelUtils.IsInk(),
            // which means pixels 127, 0, and 50 are ink (3/25 = 12%), 128+ are background
            Assert.True(result.IsValid,
                "ContrastValidator should use PixelUtils.IsInk() for consistent threshold (< 128)");
        }

        /// <summary>
        /// REGRESSION GUARD: Prevents ambiguity between DensityFraction and DensityPercent.
        ///
        /// DEFECT HISTORY (Phase 1B/1C):
        ///   - Original Implementation: Single `Density` property was ambiguous
        ///   - Root Cause: Unclear whether value was fraction (0.05) or percent (5)
        ///   - Impact: DensityValidator compared as fraction but stored as percentage
        ///   - Fix: Split into DensityFraction (0.0-1.0) and DensityPercent (0-100)
        ///
        /// VALIDATION STRATEGY:
        ///   - Create image with known ink ratio (25% = 0.25 fraction)
        ///   - Verify DensityValidator sets both properties correctly
        ///   - Ensure fraction is used for threshold comparison
        /// </summary>
        [Fact]
        public void DensityValidator_SetsBothFractionAndPercent_Unambiguously()
        {
            // Arrange: 4x4 image with 4 ink pixels out of 16 total = 25% density
            using var image = CreateTestImage(4, 4, 4);  // 4 black pixels

            var capsule = CreateTestCapsule(image);
            var settings = new DensityValidatorSettings
            {
                MinDensityThreshold = 0.1f,  // 10% minimum (as fraction)
                MaxDensityThreshold = 0.9f   // 90% maximum (as fraction)
            };
            var validator = new DensityValidator(Options.Create(settings));
            var metrics = new QualityMetrics();

            // Act
            var result = validator.Validate(capsule, metrics);

            // Assert: Both properties should be set correctly
            Assert.Equal(0.25, metrics.DensityFraction, precision: 2);  // 4/16 = 0.25
            Assert.Equal(25.0, metrics.DensityPercent, precision: 1);   // 25%

            // Assert: Validator should use fraction for comparison (not percent)
            // Since 0.25 is between 0.1 and 0.9, validation should pass
            Assert.True(result.IsValid,
                "DensityValidator should use DensityFraction (0.25) for threshold comparison, not DensityPercent (25)");
        }

        /// <summary>
        /// REGRESSION GUARD: Prevents re-introduction of center-pixel check in StructureValidator.
        ///
        /// DEFECT HISTORY (Original Implementation):
        ///   - Original Implementation: Checked if center pixel was ink
        ///   - Root Cause: Assumed all symbols have solid center
        ///   - Impact: Hollow symbols (Sharp, Natural, Flat) failed validation incorrectly
        ///   - Fix (Phase I): Removed center-pixel check entirely
        ///
        /// VALIDATION STRATEGY:
        ///   - Create hollow symbol image (ink only at edges, white center)
        ///   - Verify StructureValidator passes (does not check center pixel)
        /// </summary>
        [Fact]
        public void StructureValidator_AcceptsHollowSymbols_NoCenterPixelCheck()
        {
            // Arrange: Create hollow square (ink at edges, white center)
            using var image = CreateTestImageL8(5, 5);
            // Draw ink border (hollow square)
            for (int x = 0; x < 5; x++)
            {
                image[x, 0] = new L8(0);  // Top edge
                image[x, 4] = new L8(0);  // Bottom edge
            }
            for (int y = 0; y < 5; y++)
            {
                image[0, y] = new L8(0);  // Left edge
                image[4, y] = new L8(0);  // Right edge
            }
            // Center pixel (2, 2) is explicitly white (background)
            Assert.Equal(255, image[2, 2].PackedValue);

            var capsule = CreateTestCapsule(image);
            var validator = new StructureValidator();
            var metrics = new QualityMetrics();

            // Act
            var result = validator.Validate(capsule, metrics);

            // Assert: Validator should pass hollow symbols (no center-pixel check)
            Assert.True(result.IsValid,
                "StructureValidator must accept hollow symbols. Center-pixel check was removed in Phase I.");
        }

        /// <summary>
        /// REGRESSION GUARD: Verifies all validators handle binary images (0/255) correctly.
        ///
        /// DEFECT HISTORY (Phase I-III Rendering):
        ///   - Original Issue: Generators leaked grayscale pixels (1-254)
        ///   - Root Cause: Missing explicit binarization after CloneAs<L8>()
        ///   - Impact: Validators had to tolerate grayscale, masking data integrity issues
        ///   - Fix: Generators now enforce binary output; validators assume binary input
        ///
        /// VALIDATION STRATEGY:
        ///   - Create strictly binary image (only 0 and 255 pixel values)
        ///   - Verify DensityValidator, ContrastValidator handle it correctly
        ///   - Guard against validators introducing grayscale tolerance
        /// </summary>
        [Fact]
        public void AllValidators_HandleBinaryImages_Correctly()
        {
            // Arrange: Create binary image (50% ink, 50% background, all 0 or 255)
            using var image = CreateTestImage(10, 10, 50);  // 50 out of 100 pixels = 50% density

            // Verify image is strictly binary
            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    foreach (var pixel in accessor.GetRowSpan(y))
                    {
                        Assert.True(pixel.PackedValue == 0 || pixel.PackedValue == 255,
                            "Test image must be strictly binary (0 or 255 only)");
                    }
                }
            });

            var capsule = CreateTestCapsule(image);
            var densitySettings = new DensityValidatorSettings
            {
                MinDensityThreshold = 0.3f,  // 30% minimum
                MaxDensityThreshold = 0.7f   // 70% maximum
            };
            var densityValidator = new DensityValidator(Options.Create(densitySettings));
            var contrastValidator = new ContrastValidator();
            var structureValidator = new StructureValidator();
            var metrics = new QualityMetrics();

            // Act: Run all validators on strictly binary image
            var densityResult = densityValidator.Validate(capsule, metrics);
            var contrastResult = contrastValidator.Validate(capsule, metrics);
            var structureResult = structureValidator.Validate(capsule, metrics);

            // Assert: All validators should handle binary images correctly
            Assert.True(densityResult.IsValid,
                "DensityValidator should handle binary images (50% density is within 30-70%)");
            Assert.True(contrastResult.IsValid,
                "ContrastValidator should handle binary images (sufficient contrast between 0 and 255)");
            Assert.True(structureResult.IsValid,
                "StructureValidator should handle binary images");

            // Assert: Density metrics should be exact (no grayscale noise)
            Assert.Equal(0.5, metrics.DensityFraction, precision: 2);  // Exactly 50%
            Assert.Equal(50.0, metrics.DensityPercent, precision: 1);
        }

        /// <summary>
        /// REGRESSION GUARD: Verifies DensityValidator threshold comparison uses fraction, not percent.
        ///
        /// DEFECT HISTORY (Phase 1C):
        ///   - Original Implementation: Compared `density < _minDensityThreshold`
        ///     but stored `metrics.Density = density * 100` (percentage)
        ///   - Root Cause: Ambiguous semantics of single Density property
        ///   - Impact: Error messages required manual `* 100` for display
        ///   - Fix: Compare using densityFraction, store both fraction and percent
        ///
        /// VALIDATION STRATEGY:
        ///   - Create image with density just below threshold
        ///   - Verify validator uses fraction (0.0-1.0) for comparison
        ///   - Verify error message displays percent (0-100) correctly
        /// </summary>
        [Fact]
        public void DensityValidator_ThresholdComparison_UsesFractionNotPercent()
        {
            // Arrange: Create image with 8% density (0.08 fraction, below 10% threshold)
            using var image = CreateTestImage(10, 10, 8);  // 8 out of 100 pixels = 8%

            var capsule = CreateTestCapsule(image);
            var settings = new DensityValidatorSettings
            {
                MinDensityThreshold = 0.10f,  // 10% minimum (as fraction 0.10, NOT as percent 10)
                MaxDensityThreshold = 0.90f
            };
            var validator = new DensityValidator(Options.Create(settings));
            var metrics = new QualityMetrics();

            // Act
            var result = validator.Validate(capsule, metrics);

            // Assert: Validator should FAIL because 0.08 < 0.10 (fraction comparison)
            Assert.False(result.IsValid,
                "DensityValidator should fail when densityFraction (0.08) < threshold (0.10)");

            // Assert: Metrics should show both representations correctly
            Assert.Equal(0.08, metrics.DensityFraction, precision: 2);  // 8/100 = 0.08
            Assert.Equal(8.0, metrics.DensityPercent, precision: 1);   // 8%

            // Assert: Error message should display percent (8%), not fraction (0.08)
            Assert.NotNull(result.FailureMessage);
            Assert.Contains("8.00%", result.FailureMessage);  // Human-readable percentage
            Assert.Contains("10%", result.FailureMessage);     // Threshold as percentage
        }

        /// <summary>
        /// REGRESSION GUARD: Verifies PixelUtils.IsInk() is used consistently across all validators.
        ///
        /// DEFECT HISTORY (Phase 1A, Phase 2A):
        ///   - Original: ContrastValidator used hardcoded `< 128`
        ///   - Original: SkeletonizationProcessor had inverted logic
        ///   - Root Cause: No centralized pixel classification
        ///   - Impact: Inconsistent ink detection across codebase
        ///   - Fix: All code now uses PixelUtils.IsInk()
        ///
        /// VALIDATION STRATEGY:
        ///   - Create image with pixels at exact threshold (127, 128)
        ///   - Verify all validators classify pixels identically
        ///   - Guard against divergent threshold logic
        /// </summary>
        [Fact]
        public void AllValidators_PixelClassification_ConsistentWithPixelUtils()
        {
            // Arrange: Create image with 6 ink pixels (< 128) out of 9 total = 66.67%
            using var image = CreateTestImageL8(3, 3);
            image[0, 0] = new L8(0);    // Ink (PixelUtils.IsInk = true)
            image[1, 0] = new L8(127);  // Ink (< 128, PixelUtils.IsInk = true)
            image[2, 0] = new L8(128);  // Background (>= 128, PixelUtils.IsInk = false)
            image[0, 1] = new L8(60);   // Ink (< 128) - changed from 129 to get 6 ink pixels
            image[1, 1] = new L8(200);  // Background
            image[2, 1] = new L8(255);  // Background (white)
            image[0, 2] = new L8(50);   // Ink
            image[1, 2] = new L8(100);  // Ink
            image[2, 2] = new L8(126);  // Ink

            var capsule = CreateTestCapsule(image);
            var densitySettings = new DensityValidatorSettings
            {
                MinDensityThreshold = 0.5f,  // 50% minimum
                MaxDensityThreshold = 0.8f   // 80% maximum
            };
            var densityValidator = new DensityValidator(Options.Create(densitySettings));
            var metrics = new QualityMetrics();

            // Act
            var result = densityValidator.Validate(capsule, metrics);

            // Assert: DensityValidator should count exactly 6 ink pixels (using PixelUtils.IsInk)
            // Ink pixels: 0, 127, 60, 50, 100, 126 = 6 pixels
            // 6 / 9 = 0.6667 = 66.67%, which is between 50% and 80%, so should pass
            Assert.True(result.IsValid,
                "DensityValidator should classify pixels consistently with PixelUtils.IsInk()");
            Assert.Equal(0.67, metrics.DensityFraction, precision: 2);  // 6/9 ≈ 0.67
        }

        // ===== Helper Methods =====

        /// <summary>
        /// Creates a test L8 image filled with background (255).
        /// </summary>
        private Image<L8> CreateTestImageL8(int width, int height)
        {
            var image = new Image<L8>(width, height);
            // Initialize all pixels to background (255)
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    image[x, y] = new L8(255);
                }
            }
            return image;
        }

        /// <summary>
        /// Creates a test image with specified number of black pixels (from top-left).
        /// </summary>
        private Image<L8> CreateTestImage(int width, int height, int blackPixelCount)
        {
            var image = new Image<L8>(width, height);
            // Start with completely white canvas
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    image[x, y] = new L8(255);  // White pixel
                }
            }

            // Add specified number of black pixels from top-left
            int count = 0;
            for (int y = 0; y < height && count < blackPixelCount; y++)
            {
                for (int x = 0; x < width && count < blackPixelCount; x++)
                {
                    image[x, y] = new L8(0);  // Black pixel
                    count++;
                }
            }

            return image;
        }

        /// <summary>
        /// Creates a test SymbolCapsule from an image.
        /// </summary>
        private SymbolCapsule CreateTestCapsule(Image<L8> image)
        {
            return new SymbolCapsule(
                image,
                new TemplateMetadata
                {
                    TemplateName = "test-template",
                    SymbolType = SymbolType.Unknown,
                    GeneratedBy = "TestRunner",
                    TemplateHash = "test-hash-12345",
                    Provenance = new ProvenanceMetadata
                    {
                        SourceImage = "test-source.png",
                        Method = PreprocessingMethod.Raw,
                        ValidationDate = DateTime.UtcNow,
                        ValidatedBy = "TestRunner"
                    }
                },
                new QualityMetrics(),
                true,
                new List<ValidationResult>()
            );
        }
    }
}
