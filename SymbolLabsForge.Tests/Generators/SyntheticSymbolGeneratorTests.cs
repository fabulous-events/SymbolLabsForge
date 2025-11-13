//===============================================================
// File: SyntheticSymbolGeneratorTests.cs
// Author: Gemini
// Date: 2025-11-12
// Purpose: Contains unit tests for the SyntheticSymbolGenerator.
//===============================================================
#nullable enable

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SymbolLabsForge.Contracts;
using SymbolLabsForge.Generators;
using Xunit;

namespace SymbolLabsForge.Tests.Generators
{
    public class SyntheticSymbolGeneratorTests
    {
        private readonly SyntheticSymbolGenerator _generator = new();
        private readonly Size _dimensions = new(128, 128);

        [Fact]
        public void Generate_WholeNote_ShouldProduceNonEmptyImage()
        {
            // Arrange
            var parameters = new SymbolParameters { SymbolType = MusicSymbolType.WholeNote };

            // Act
            var image = _generator.Generate(parameters, _dimensions);

            // Assert
            Assert.NotNull(image);
            Assert.True(image.Width >= _dimensions.Width);
            Assert.True(image.Height >= _dimensions.Height);

            // Basic validation: check if there's at least one white pixel
            bool hasContent = false;
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    if (image[x, y].PackedValue == 255)
                    {
                        hasContent = true;
                        break;
                    }
                }
                if (hasContent) break;
            }
            Assert.True(hasContent, "Generated image should not be empty.");
        }

        [Fact]
        public void Generate_QuarterNote_ShouldProduceNonEmptyImage()
        {
            // Arrange
            var parameters = new SymbolParameters { SymbolType = MusicSymbolType.QuarterNote };

            // Act
            var image = _generator.Generate(parameters, _dimensions);

            // Assert
            Assert.NotNull(image);
            Assert.True(image.Width >= _dimensions.Width);
            Assert.True(image.Height >= _dimensions.Height);

            // Basic validation: check if there's at least one white pixel
            bool hasContent = false;
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    if (image[x, y].PackedValue == 255)
                    {
                        hasContent = true;
                        break;
                    }
                }
                if (hasContent) break;
            }
            Assert.True(hasContent, "Generated image should not be empty.");
        }
    }
}
