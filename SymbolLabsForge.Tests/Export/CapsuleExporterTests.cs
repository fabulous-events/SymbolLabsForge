using Xunit;
using SymbolLabsForge.Services;
using SymbolLabsForge.Contracts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SymbolLabsForge.Tests.Export
{
    public class CapsuleExporterTests
    {
        [Fact]
        public async Task ExportAsync_CreatesFilesAndValidatesMetadata()
        {
            // Arrange
            var exporter = new CapsuleExporter();
            var capsule = CreateTestCapsule("flat-test", "id-123", "hash-abc", "Forge v1.0");
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);

            try
            {
                // Act
                await exporter.ExportAsync(capsule, tempDir, "skeleton");

                // Assert
                var expectedPngPath = Path.Combine(tempDir, "flat-test-skeleton.png");
                var expectedJsonPath = Path.Combine(tempDir, "flat-test-skeleton.json");

                Assert.True(File.Exists(expectedPngPath));
                Assert.True(File.Exists(expectedJsonPath));

                // --- Metadata Validation ---
                var jsonContent = await File.ReadAllTextAsync(expectedJsonPath);
                var jsonObject = JObject.Parse(jsonContent);

                Assert.Equal("id-123", jsonObject["Metadata"]?["CapsuleId"]?.ToString());
                Assert.Equal("hash-abc", jsonObject["Metadata"]?["TemplateHash"]?.ToString());
                Assert.Equal("Forge v1.0", jsonObject["Metadata"]?["GeneratedBy"]?.ToString());
                Assert.NotNull(jsonObject["Metadata"]?["GeneratedOn"]);
                Assert.Equal(10, jsonObject["Metrics"]?["Width"]?.ToObject<int>());
                Assert.Equal("Valid", jsonObject["Metrics"]?["DensityStatus"]?.ToString());
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        private SymbolCapsule CreateTestCapsule(string name, string id, string hash, string generatorVersion)
        {
            var image = new Image<L8>(10, 20);
            var metadata = new TemplateMetadata
            {
                TemplateName = name,
                CapsuleId = id,
                TemplateHash = hash,
                GeneratedBy = generatorVersion,
                GeneratedOn = System.DateTime.UtcNow.ToString()
            };
            var metrics = new QualityMetrics
            {
                Width = 10,
                Height = 20,
                DensityStatus = DensityStatus.Valid
            };
            return new SymbolCapsule(image, metadata, metrics, true, new List<ValidationResult>());
        }
    }
}