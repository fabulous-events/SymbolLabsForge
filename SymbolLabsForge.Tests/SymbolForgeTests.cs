using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SymbolLabsForge.Contracts;
using SymbolLabsForge.Generation;
using SymbolLabsForge.Preprocessing;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SymbolLabsForge.Utils;
using Xunit;
using System.Threading.Tasks;

namespace SymbolLabsForge.Tests
{
    public class SymbolForgeTests
    {
        private class MockSymbolGenerator : ISymbolGenerator
        {
            public SymbolType SupportedType => SymbolType.Flat;

            public Image<L8> GenerateRawImage(Size dimensions, int? seed)
            {
                return new Image<L8>(dimensions.Width, dimensions.Height);
            }
        }

        private class MockValidator : IValidator
        {
            public string Name => "MockValidator";

            public ValidationResult Validate(SymbolCapsule capsule, QualityMetrics metrics)
            {
                return new ValidationResult(true, Name);
            }
        }

        private class MockPreprocessingStep : IPreprocessingStep
        {
            public Image<L8> Process(Image<L8> image)
            {
                return image;
            }
        }

        private class MockMorphEngine : IMorphEngine
        {
            public Task<Image<L8>> MorphAsync(MorphRequest request)
            {
                return Task.FromResult(new Image<L8>(100, 100));
            }
        }

        private readonly SymbolForge _forge;

        public SymbolForgeTests()
        {
            var generators = new[] { new MockSymbolGenerator() };
            var validators = new[] { new MockValidator() };
            var skeletonizationProcessor = new MockPreprocessingStep();
            var morphEngine = new MockMorphEngine();
            var logger = new LoggerFactory().CreateLogger<SymbolForge>();

            _forge = new SymbolForge(generators, validators, skeletonizationProcessor, morphEngine, logger);
        }

        [Fact]
        public void Generate_WithSingleDimension_ReturnsPrimaryCapsule()
        {
            // Arrange
            var request = new SymbolRequest(
                SymbolType.Flat,
                new List<Size> { new Size(100, 100) },
                new List<OutputForm> { OutputForm.Raw });

            // Act
            var result = _forge.Generate(request);

            // Assert
            Assert.NotNull(result.Primary);
            Assert.Empty(result.Variants);
        }

        [Fact]
        public void Generate_WithMultipleDimensions_ReturnsPrimaryAndVariants()
        {
            // Arrange
            var request = new SymbolRequest(
                SymbolType.Flat,
                new List<Size> { new Size(100, 100), new Size(200, 200) },
                new List<OutputForm> { OutputForm.Raw });

            // Act
            var result = _forge.Generate(request);

            // Assert
            Assert.NotNull(result.Primary);
            Assert.Single(result.Variants);
        }

        [Fact]
        public void Generate_WithEdgeCases_ReturnsEdgeCaseVariants()
        {
            // Arrange
            var request = new SymbolRequest(
                SymbolType.Flat,
                new List<Size> { new Size(100, 100) },
                new List<OutputForm> { OutputForm.Raw },
                EdgeCasesToGenerate: new List<EdgeCaseType> { EdgeCaseType.Rotated });

            // Act
            var result = _forge.Generate(request);

            // Assert
            Assert.NotNull(result.Primary);
            Assert.Single(result.Variants);
            Assert.Contains("edge_Rotated", result.Variants.First().Metadata.TemplateName);
        }

        [Fact]
        public void Generate_ComputesCorrectHash()
        {
            // Arrange
            var request = new SymbolRequest(
                SymbolType.Flat,
                new List<Size> { new Size(100, 100) },
                new List<OutputForm> { OutputForm.Raw });

            // Act
            var result = _forge.Generate(request);

            // Assert
            using var ms = new MemoryStream();
            result.Primary.TemplateImage.SaveAsBmp(ms);
            var imageBytes = ms.ToArray();
            var expectedHash = HashUtil.ComputeSha256(imageBytes);

            Assert.Equal(expectedHash, result.Primary.Metadata.TemplateHash);
        }
    }
}
