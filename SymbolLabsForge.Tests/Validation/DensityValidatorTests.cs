using Xunit;
using SymbolLabsForge.Validation;
using SymbolLabsForge.Contracts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;

namespace SymbolLabsForge.Tests.Validation
{
    public class DensityValidatorTests
    {
        private readonly DensityValidator _validator = new DensityValidator();

        [Fact]
        public void Validate_WithDensityBelowThreshold_ReturnsFail()
        {
            // Arrange (2% density)
            using var image = CreateTestImage(100, 100, 200);
            var capsule = CreateTestCapsule(image);
            var metrics = new QualityMetrics();

            // Act
            var result = _validator.Validate(capsule, metrics);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal(DensityStatus.TooLow, metrics.DensityStatus);
            Assert.Equal(2.0, metrics.Density, 2);
        }

        [Fact]
        public void Validate_WithDensityWithinThreshold_ReturnsPass()
        {
            // Arrange (10% density)
            using var image = CreateTestImage(100, 100, 1000);
            var capsule = CreateTestCapsule(image);
            var metrics = new QualityMetrics();

            // Act
            var result = _validator.Validate(capsule, metrics);

            // Assert
            Assert.True(result.IsValid);
            Assert.Equal(DensityStatus.Valid, metrics.DensityStatus);
            Assert.Equal(10.0, metrics.Density, 2);
        }

        [Fact]
        public void Validate_WithDensityAboveThreshold_ReturnsFail()
        {
            // Arrange (15% density)
            using var image = CreateTestImage(100, 100, 1500);
            var capsule = CreateTestCapsule(image);
            var metrics = new QualityMetrics();

            // Act
            var result = _validator.Validate(capsule, metrics);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal(DensityStatus.TooHigh, metrics.DensityStatus);
            Assert.Equal(15.0, metrics.Density, 2);
        }

        [Fact]
        public void Validate_WithDensityJustBelowMinThreshold_ReturnsFail()
        {
            // Arrange (4.9% density)
            using var image = CreateTestImage(100, 100, 490);
            var capsule = CreateTestCapsule(image);
            var metrics = new QualityMetrics();

            // Act
            var result = _validator.Validate(capsule, metrics);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal(DensityStatus.TooLow, metrics.DensityStatus);
        }

        [Fact]
        public void Validate_WithDensityAtMinThreshold_ReturnsPass()
        {
            // Arrange (5.0% density)
            using var image = CreateTestImage(100, 100, 500);
            var capsule = CreateTestCapsule(image);
            var metrics = new QualityMetrics();

            // Act
            var result = _validator.Validate(capsule, metrics);

            // Assert
            Assert.True(result.IsValid);
            Assert.Equal(DensityStatus.Valid, metrics.DensityStatus);
        }

        [Fact]
        public void Validate_WithDensityAtMaxThreshold_ReturnsPass()
        {
            // Arrange (12.0% density)
            using var image = CreateTestImage(100, 100, 1200);
            var capsule = CreateTestCapsule(image);
            var metrics = new QualityMetrics();

            // Act
            var result = _validator.Validate(capsule, metrics);

            // Assert
            Assert.True(result.IsValid);
            Assert.Equal(DensityStatus.Valid, metrics.DensityStatus);
        }

        [Fact]
        public void Validate_WithDensityJustAboveMaxThreshold_ReturnsFail()
        {
            // Arrange (12.1% density)
            using var image = CreateTestImage(100, 100, 1210);
            var capsule = CreateTestCapsule(image);
            var metrics = new QualityMetrics();

            // Act
            var result = _validator.Validate(capsule, metrics);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal(DensityStatus.TooHigh, metrics.DensityStatus);
        }

        private Image<L8> CreateTestImage(int width, int height, int blackPixelCount)
        {
            var image = new Image<L8>(width, height);
            // Start with a completely white canvas.
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    image[x, y] = new L8(255); // White pixel
                }
            }

            // Now, add the specified number of black pixels from the top-left.
            int count = 0;
            for (int y = 0; y < height && count < blackPixelCount; y++)
            {
                for (int x = 0; x < width && count < blackPixelCount; x++)
                {
                    image[x, y] = new L8(0); // Black pixel
                    count++;
                }
            }
            return image;
        }

        [Theory]
        [InlineData(10000, DensityStatus.TooHigh)] // All black image should have 100% density
        [InlineData(0, DensityStatus.TooLow)] // All white image should have 0% density
        public void Validate_WithEdgeCaseImages_ReturnsCorrectStatus(int blackPixelCount, DensityStatus expectedStatus)
        {
            // Arrange
            using var image = CreateTestImage(100, 100, blackPixelCount);
            var capsule = CreateTestCapsule(image);
            var metrics = new QualityMetrics();

            // Act
            var result = _validator.Validate(capsule, metrics);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal(expectedStatus, metrics.DensityStatus);
        }

        [Fact]
        public void Validate_WithEmptyImage_FailsGracefully()
        {
            // Arrange
            using var image = CreateTestImage(1, 1, 0); // 1x1 empty image with 0 black pixels
            var capsule = CreateTestCapsule(image);
            var metrics = new QualityMetrics();

            // Act
            var result = _validator.Validate(capsule, metrics);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal(DensityStatus.TooLow, metrics.DensityStatus);
        }

        private SymbolCapsule CreateTestCapsule(Image<L8> image)
        {
            return new SymbolCapsule(
                image,
                new TemplateMetadata(),
                new QualityMetrics(),
                true,
                new List<ValidationResult>()
            );
        }
    }
}