//===============================================================
// File: DoubleSharpGeneratorTests.cs
// Author: Claude (Phase III-G)
// Date: 2025-11-14
// Purpose: Snapshot tests for DoubleSharpGenerator with tight tolerance.
//          Enforces pixel-perfect binary output after Phase I/II fixes.
//
// PHASE III-G: Tolerance set to 0.001 (0.1%) for geometric symbols
//===============================================================
#nullable enable

using Xunit;
using SymbolLabsForge.Generation;
using SymbolLabsForge.Utils;
using SymbolLabsForge.Provenance.Utilities;
using SymbolLabsForge.Testing.Utilities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;

namespace SymbolLabsForge.Tests.Generators
{
    public class DoubleSharpGeneratorTests
    {
        private readonly DoubleSharpGenerator _generator = new();

        [Fact]
        public void GenerateRawImage_WithValidDimensions_ReturnsImageOfCorrectSize()
        {
            // Arrange
            var dimensions = new Size(30, 30);

            // Act
            using var image = _generator.GenerateRawImage(dimensions, null);

            // Assert
            Assert.NotNull(image);
            Assert.Equal(dimensions.Width, image.Width);
            Assert.Equal(dimensions.Height, image.Height);
        }

        [Fact]
        public void GenerateRawImage_IsNotEmptyAndContainsBlackPixels()
        {
            // Arrange
            var dimensions = new Size(30, 30);
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
                        if (pixelRow[x].PackedValue < 128)
                        {
                            containsBlackPixel = true;
                            return;
                        }
                    }
                }
            });

            Assert.True(containsBlackPixel, "The generated DoubleSharp image should not be empty.");
        }

        [Fact]
        public void GenerateRawImage_MatchesVerifiedSnapshot()
        {
            // Arrange
            var dimensions = new Size(30, 30);
            var snapshotPath = Path.Combine("TestAssets", "Snapshots", "Generators", "DoubleSharpGenerator_ValidDimensions_Expected.png");
            var diffPath = Path.Combine("TestAssets", "Diffs", "Generators", "DoubleSharpGenerator_ValidDimensions_Diff.png");

            // Act
            using var actualImage = _generator.GenerateRawImage(dimensions, null);

            // Assert
            if (!File.Exists(snapshotPath))
            {
                var snapshotDir = Path.GetDirectoryName(snapshotPath);
                Assert.NotNull(snapshotDir);
                Directory.CreateDirectory(snapshotDir);
                actualImage.Save(snapshotPath);
                Assert.True(true, $"Snapshot created at {snapshotPath}. Please verify it manually.");
                return;
            }

            using var expectedImage = Image.Load<L8>(snapshotPath);

            // PHASE III-G: Tighten tolerance to 0.001 (0.1%) for geometric symbols
            // DoubleSharp symbol uses binary geometry with no AA - enforce near-zero tolerance
            var areSimilar = SnapshotComparer.AreSimilar(expectedImage, actualImage, tolerance: 0.001);

            if (!areSimilar)
            {
                ImageDiffGenerator.SaveDiff(expectedImage, actualImage, diffPath);
            }

            Assert.True(areSimilar, $"Image mismatch. See diff image for details: {diffPath}");
        }
    }
}
