using SymbolLabsForge.Services;
using SymbolLabsForge.Contracts;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;
using Newtonsoft.Json;
using Xunit;

namespace SymbolLabsForge.Tests.Registry
{
    public class CapsuleRegistryManagerTests
    {
        [Fact]
        public async Task AddEntryAsync_AddsNewEntry_ToEmptyRegistry()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            var manager = new CapsuleRegistryManager(tempFile);
            var capsule = CreateTestCapsule("id-1", "hash-1");

            try
            {
                // Act
                await manager.AddEntryAsync(capsule);

                // Assert
                var jsonContent = await File.ReadAllTextAsync(tempFile);
                var registry = JsonConvert.DeserializeObject<CapsuleRegistry>(jsonContent);
                Assert.Single(registry.Capsules);
                Assert.Equal("id-1", registry.Capsules[0].CapsuleId);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task AddEntryAsync_DoesNotAddDuplicateEntry()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            var manager = new CapsuleRegistryManager(tempFile);
            var capsule1 = CreateTestCapsule("id-1", "hash-1");
            var capsule2 = CreateTestCapsule("id-1", "hash-2"); // Same ID, different hash

            try
            {
                // Act
                await manager.AddEntryAsync(capsule1);
                await manager.AddEntryAsync(capsule2); // Attempt to add duplicate

                // Assert
                var jsonContent = await File.ReadAllTextAsync(tempFile);
                var registry = JsonConvert.DeserializeObject<CapsuleRegistry>(jsonContent);
                Assert.Single(registry.Capsules); // Should still be 1
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        private SymbolCapsule CreateTestCapsule(string id, string hash)
        {
            var image = new Image<L8>(1, 1);
            var metadata = new TemplateMetadata { CapsuleId = id, TemplateHash = hash };
            return new SymbolCapsule(image, metadata, new QualityMetrics(), true, new List<ValidationResult>());
        }
    }
}
