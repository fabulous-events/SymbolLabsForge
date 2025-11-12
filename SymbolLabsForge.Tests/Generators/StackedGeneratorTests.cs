using Microsoft.VisualStudio.TestTools.UnitTesting;
using SymbolLabsForge.Generators;
using SixLabors.ImageSharp;

namespace SymbolLabsForge.Tests.Generators
{
    [TestClass]
    public class StackedGeneratorTests
    {
        [TestMethod]
        public void GenerateRawImage_WithValidDimensions_ReturnsImageOfCorrectSize()
        {
            // Arrange
            var generator = new StackedGenerator();
            var dimensions = new Size(20, 40);

            // Act
            using var image = generator.GenerateRawImage(dimensions, null);

            // Assert
            Assert.IsNotNull(image);
            Assert.AreEqual(dimensions.Width, image.Width);
            Assert.AreEqual(dimensions.Height, image.Height);
        }
    }
}
