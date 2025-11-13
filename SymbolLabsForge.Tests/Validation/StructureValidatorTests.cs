//===============================================================
// File: StructureValidatorTests.cs
// Author: Gemini
// Date: 2025-11-12
// Purpose: Contains unit tests for the StructureValidator.
//===============================================================
#nullable enable

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SymbolLabsForge.Contracts;
using SymbolLabsForge.Validation;
using Xunit;
using System.Collections.Generic;

using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;

namespace SymbolLabsForge.Tests.Validation
{
    public class StructureValidatorTests
    {
        private readonly StructureValidator _validator = new();
        private readonly QualityMetrics _metrics = new();

        private Image<L8> CreateTestImage(bool isEmpty)
        {
            var image = new Image<L8>(100, 100);
            image.Mutate(ctx => {
                ctx.Clear(Color.White);
                if (!isEmpty)
                {
                    ctx.Fill(Color.Black, new SixLabors.ImageSharp.Drawing.RectangularPolygon(25, 25, 50, 50));
                }
            });
            return image;
        }

        [Fact]
        public void Validate_WithValidSymbol_ReturnsPass()
        {
            // Arrange
            using var image = CreateTestImage(false);
            var capsule = new SymbolCapsule(image, new TemplateMetadata(), _metrics, false, new List<ValidationResult>());

            // Act
            var result = _validator.Validate(capsule, _metrics);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_WithEmptyImage_ReturnsFail()
        {
            // Arrange
            using var image = CreateTestImage(true);
            var capsule = new SymbolCapsule(image, new TemplateMetadata(), _metrics, false, new List<ValidationResult>());

            // Act
            var result = _validator.Validate(capsule, _metrics);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Image canvas is empty.", result.FailureMessage);
        }

        [Fact]
        public void Validate_WithNullImage_ReturnsFail()
        {
            // Arrange
            var capsule = new SymbolCapsule(null, new TemplateMetadata(), _metrics, false, new List<ValidationResult>());

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
