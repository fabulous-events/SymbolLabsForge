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

        [Fact]
        public void Validate_WithValidSymbol_ReturnsPass()
        {
            // Arrange
            using var image = new Image<L8>(100, 100);
            image.Mutate(ctx => {
                ctx.Clear(Color.White);
                ctx.Fill(Color.Black, new SixLabors.ImageSharp.Drawing.RectangularPolygon(25, 25, 50, 50));
            });
            var capsule = new SymbolCapsule(image, new TemplateMetadata { SymbolType = SymbolType.Unknown }, _metrics, false, new List<ValidationResult>());

            // Act
            var result = _validator.Validate(capsule, _metrics);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_WithEmptyImage_ReturnsFail()
        {
            // Arrange
            using var image = new Image<L8>(1, 1);
            image.Mutate(ctx => ctx.Clear(Color.White));
            var capsule = new SymbolCapsule(image, new TemplateMetadata { SymbolType = SymbolType.Unknown }, _metrics, false, new List<ValidationResult>());

            // Act
            var result = _validator.Validate(capsule, _metrics);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Symbol is hollow; center pixel is not ink.", result.FailureMessage);
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
