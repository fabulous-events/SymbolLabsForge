using Microsoft.VisualStudio.TestTools.UnitTesting;
using SymbolLabsForge.Generators;
using SymbolLabsForge.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;

namespace SymbolLabsForge.Tests.Generators
{
    [TestClass]
    public class ClefGeneratorTests
    {
        private readonly ClefGenerator _generator = new ClefGenerator();

        [TestMethod]
        public void GenerateRawImage_WithValidDimensions_ReturnsImageOfCorrectSize()
        {
            // Arrange
            var dimensions = new Size(30, 80);

            // Act
            using var image = _generator.GenerateRawImage(dimensions, null);

            // Assert
            Assert.IsNotNull(image);
            Assert.AreEqual(dimensions.Width, image.Width);
            Assert.AreEqual(dimensions.Height, image.Height);
        }

        [TestMethod]
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
                actualImage.Save(snapshotPath);
                Assert.IsTrue(true, $"Snapshot created at {snapshotPath}. Please verify it manually.");
                return;
            }

            using var expectedImage = Image.Load<L8>(snapshotPath);
            var areSimilar = SnapshotComparer.AreSimilar(expectedImage, actualImage, 0.02); // Higher tolerance for anti-aliased images

            if (!areSimilar)
            {
                ImageDiffGenerator.SaveDiff(expectedImage, actualImage, diffPath);
            }

            Assert.IsTrue(areSimilar, $"Image mismatch. See diff image for details: {diffPath}");
        }
    }
}
