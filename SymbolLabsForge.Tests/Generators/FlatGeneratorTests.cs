using Xunit;
using SymbolLabsForge.Generators;
using SymbolLabsForge.Utils;
using SymbolLabsForge.Provenance.Utilities;
using SymbolLabsForge.Testing.Utilities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;

namespace SymbolLabsForge.Tests.Generators
{
    public class FlatGeneratorTests
    {
        private readonly FlatGenerator _generator = new FlatGenerator();

        [Fact]
        public void GenerateRawImage_WithValidDimensions_ReturnsImageOfCorrectSize()
        {
            // Arrange
            var dimensions = new Size(12, 30);

            // Act
            using var image = _generator.GenerateRawImage(dimensions, null);

            // Assert
            Assert.NotNull(image);
            Assert.Equal(dimensions.Width, image.Width);
            Assert.Equal(dimensions.Height, image.Height);
        }

        [Theory]
        [InlineData(0, 30)]
        [InlineData(12, 0)]
        [InlineData(-1, 30)]
        public void GenerateRawImage_WithInvalidDimensions_ThrowsException(int width, int height)
        {
            // Arrange
            var dimensions = new Size(width, height);

            // Act & Assert
            Assert.ThrowsAny<Exception>(() => _generator.GenerateRawImage(dimensions, null));
        }

        [Fact]
        public void GenerateRawImage_IsNotEmptyAndContainsBlackPixels()
        {
            // Arrange
            var dimensions = new Size(12, 30);
            bool containsBlackPixel = false;

            // Act
            using var image = _generator.GenerateRawImage(dimensions, null);

            // Assert
            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    Span<L8> pixelRow = accessor.GetRowSpan(y);
                    for (int x = 0; x < pixelRow.Length; x++)
                    {
                        if (pixelRow[x].PackedValue < 128) // Check for black/dark pixels
                        {
                            containsBlackPixel = true;
                            return;
                        }
                    }
                }
            });

            Assert.True(containsBlackPixel, "The generated image should not be empty.");
        }

        [Fact]
        public void GenerateRawImage_MatchesVerifiedSnapshot()
        {
            // Arrange
            var dimensions = new Size(12, 30);
            var snapshotPath = Path.Combine("TestAssets", "Snapshots", "Generators", "FlatGenerator_ValidDimensions_Expected.png");
            var diffPath = Path.Combine("TestAssets", "Diffs", "Generators", "FlatGenerator_ValidDimensions_Diff.png");

            // Act
            using var actualImage = _generator.GenerateRawImage(dimensions, null);

            // Assert
            if (!File.Exists(snapshotPath))
            {
                // First run: save the generated image as the snapshot
                var snapshotDir = Path.GetDirectoryName(snapshotPath);
                Assert.NotNull(snapshotDir);
                Directory.CreateDirectory(snapshotDir);
                actualImage.Save(snapshotPath);
                Assert.True(true, $"Snapshot created at {snapshotPath}. Please verify it manually.");
                return;
            }

            using var expectedImage = Image.Load<L8>(snapshotPath);

            // PHASE III-G: Tighten tolerance to 0.001 (0.1%) for geometric symbols
            // Post-binarization + AA-disabled = strict binary output requires near-zero tolerance
            var areSimilar = SnapshotComparer.AreSimilar(expectedImage, actualImage, tolerance: 0.001);

            if (!areSimilar)
            {
                ImageDiffGenerator.SaveDiff(expectedImage, actualImage, diffPath);
            }

            Assert.True(areSimilar, $"Image mismatch. See diff image for details: {diffPath}");
        }
    }
}