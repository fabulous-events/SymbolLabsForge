using SymbolLabsForge.Generators;
using SymbolLabsForge.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using Xunit;

namespace SymbolLabsForge.Tests.Generators
{
    public class ClefGeneratorTests
    {
        private readonly ClefGenerator _generator = new ClefGenerator();

        [Fact]
        public void GenerateRawImage_WithValidDimensions_ReturnsImageOfCorrectSize()
        {
            // Arrange
            var dimensions = new Size(30, 80);

            // Act
            using var image = _generator.GenerateRawImage(dimensions, null);

            // Assert
            Assert.NotNull(image);
            Assert.Equal(dimensions.Width, image.Width);
            Assert.Equal(dimensions.Height, image.Height);
        }

        [Fact]
        public void GenerateRawImage_MatchesVerifiedSnapshot()
        {
            // Arrange
            var dimensions = new Size(30, 80);
            var snapshotPath = Path.Combine("TestAssets", "Snapshots", "Generators", "ClefGenerator_ValidDimensions_Expected.png");
            var diffPath = Path.Combine("TestAssets", "Diffs", "Generators", "ClefGenerator_ValidDimensions_Diff.png");

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
            var areSimilar = SnapshotComparer.AreSimilar(expectedImage, actualImage, 0.02); // Higher tolerance for anti-aliased images

            if (!areSimilar)
            {
                ImageDiffGenerator.SaveDiff(expectedImage, actualImage, diffPath);
            }

            Assert.True(areSimilar, $"Image mismatch. See diff image for details: {diffPath}");
        }
    }
}
