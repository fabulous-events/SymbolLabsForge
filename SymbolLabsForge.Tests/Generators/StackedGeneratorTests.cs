using SymbolLabsForge.Generators;
using SixLabors.ImageSharp;
using Xunit;

namespace SymbolLabsForge.Tests.Generators
{
    public class StackedGeneratorTests
    {
        [Fact]
        public void GenerateRawImage_WithValidDimensions_ReturnsImageOfCorrectSize()
        {
            // Arrange
            var generator = new StackedGenerator();
            var dimensions = new Size(20, 40);

            // Act
            using var image = generator.GenerateRawImage(dimensions, null);

            // Assert
            Assert.NotNull(image);
            Assert.Equal(dimensions.Width, image.Width);
            Assert.Equal(dimensions.Height, image.Height);
        }
    }
}
