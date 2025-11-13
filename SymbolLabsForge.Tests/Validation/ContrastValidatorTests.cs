//===============================================================
// File: ContrastValidatorTests.cs
// Author: Gemini
// Date: 2025-11-12
// Purpose: Contains unit tests for the ContrastValidator.
//===============================================================
#nullable enable

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SymbolLabsForge.Contracts;
using SymbolLabsForge.Validation;
using Xunit;

using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;

namespace SymbolLabsForge.Tests.Validation
{
    public class ContrastValidatorTests
    {
        private readonly ContrastValidator _validator = new();
        private readonly QualityMetrics _metrics = new();

        private Image<L8> CreateTestImage(byte backgroundColor, byte foregroundColor, bool drawRectangle)
        {
            var image = new Image<L8>(100, 100);
            image.Mutate(ctx => {
                ctx.Clear(Color.FromRgba(backgroundColor, backgroundColor, backgroundColor, 255));
                if (drawRectangle)
                {
                    ctx.Fill(new SolidBrush(Color.FromRgba(foregroundColor, foregroundColor, foregroundColor, 255)), new SixLabors.ImageSharp.Drawing.RectangularPolygon(25, 25, 50, 50));
                }
            });
            return image;
        }

        [Fact]
        public void Validate_WithHighContrast_ReturnsPass()
        {
            // Arrange
            using var image = CreateTestImage(0, 255, true); // Black background, white symbol
            var capsule = new SymbolCapsule(image, new TemplateMetadata(), _metrics, false, new System.Collections.Generic.List<ValidationResult>());

            // Act
            var result = _validator.Validate(capsule, _metrics);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_WithLowContrast_ReturnsFail()
        {
            // Arrange
            using var image = CreateTestImage(120, 140, true); // Gray background, slightly different gray symbol
            var capsule = new SymbolCapsule(image, new TemplateMetadata(), _metrics, false, new System.Collections.Generic.List<ValidationResult>());

            // Act
            var result = _validator.Validate(capsule, _metrics);

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Validate_WithNoContrast_ReturnsFail()
        {
            // Arrange
            using var image = CreateTestImage(128, 128, true); // Same color for background and foreground
            var capsule = new SymbolCapsule(image, new TemplateMetadata(), _metrics, false, new System.Collections.Generic.List<ValidationResult>());

            // Act
            var result = _validator.Validate(capsule, _metrics);

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Validate_WithSingleColorImage_ReturnsFail()
        {
            // Arrange
            using var image = CreateTestImage(200, 200, false); // Image is a single solid color
            var capsule = new SymbolCapsule(image, new TemplateMetadata(), _metrics, false, new System.Collections.Generic.List<ValidationResult>());

            // Act
            var result = _validator.Validate(capsule, _metrics);

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Validate_WithNullImage_ReturnsFail()
        {
            // Arrange
            var capsule = new SymbolCapsule(null, new TemplateMetadata(), _metrics, false, new System.Collections.Generic.List<ValidationResult>());

            // Act
            var result = _validator.Validate(capsule, _metrics);

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Validate_WithNullCapsule_ReturnsFail()
        {
            // Act
            var result = _validator.Validate(null, _metrics);

            // Assert
            Assert.False(result.IsValid);
        }
    }
}
