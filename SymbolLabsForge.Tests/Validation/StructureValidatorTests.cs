using Xunit;
using SymbolLabsForge.Validation;
using SymbolLabsForge.Contracts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;

namespace SymbolLabsForge.Tests.Validation
{
    public class StructureValidatorTests
    {
        // A simple implementation for testing purposes. A real one would be more complex.
        private class TestableStructureValidator : IValidator
        {
            public string Name => "Testable Structure Validator";
            public ValidationResult Validate(SymbolCapsule capsule, QualityMetrics metrics)
            {
                bool hasContent = false;
                capsule.TemplateImage.ProcessPixelRows(accessor =>
                {
                    for (int y = 0; y < accessor.Height; y++)
                    {
                        foreach (var pixel in accessor.GetRowSpan(y))
                        {
                            if (pixel.PackedValue < 255) // Found a non-white pixel
                            {
                                hasContent = true;
                                return;
                            }
                        }
                    }
                });

                if (hasContent) return new ValidationResult(true, Name);
                return new ValidationResult(false, Name, "Image canvas is empty.");
            }
        }

        [Fact]
        [Trait("Category", "Validator")]
        [Trait("AuditTag", "Phase2.12")]
        public void Validate_WithEmptyImage_ReturnsFail()
        {
            // Arrange
            var validator = new TestableStructureValidator();
            using var image = new Image<L8>(10, 10);
            image.Mutate(ctx => ctx.BackgroundColor(Color.White)); // All white
            var capsule = new SymbolCapsule(image, new TemplateMetadata(), new QualityMetrics(), true, new List<ValidationResult>());
            var metrics = new QualityMetrics();

            // Act
            var result = validator.Validate(capsule, metrics);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Image canvas is empty.", result.FailureMessage);
        }
    }
}