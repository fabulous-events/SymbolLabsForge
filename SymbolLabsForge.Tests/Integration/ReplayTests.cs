using SymbolLabsForge.Utils;
using System.Threading.Tasks;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SymbolLabsForge.Contracts;
using System.Collections.Generic;
using Newtonsoft.Json;
using Xunit;

namespace SymbolLabsForge.Tests.Integration
{
    public class ReplayTests
    {
        [Fact]
        [Trait("Category", "Replay")]
        [Trait("AuditTag", "Phase2.18")]
        public async Task CanLoadCapsuleAndReproduceResult()
        {
            // Arrange: Create a temporary capsule on disk to simulate a real artifact
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
            var jsonPath = Path.Combine(tempDir, "replay-test.json");
            var imagePath = Path.Combine(tempDir, "replay-test.png");

            try
            {
                var originalImage = new Image<L8>(10, 10);
                originalImage[5, 5] = new L8(0); // One black pixel
                await originalImage.SaveAsync(imagePath);

                var metadata = new TemplateMetadata { TemplateName = "Flat-10x10", SymbolType = SymbolType.Flat };
                var metrics = new QualityMetrics { Width = 10, Height = 10 };
                var validationResults = new List<ValidationResult> { new ValidationResult(true, "TestValidator") };
                var dto = new { Metadata = metadata, Metrics = metrics, ValidationResults = validationResults };
                await File.WriteAllTextAsync(jsonPath, JsonConvert.SerializeObject(dto));

                // Act: Load the capsule from the file
                var (loadedCapsule, request) = await CapsuleLoader.LoadFromFileAsync(jsonPath);

                // Assert: The loaded capsule's image matches the original
                Assert.NotNull(loadedCapsule);
                Assert.NotNull(request);
                Assert.True(SnapshotComparer.AreSimilar(originalImage, loadedCapsule.TemplateImage));
            }
            finally
            {
                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            }
        }
    }
}
