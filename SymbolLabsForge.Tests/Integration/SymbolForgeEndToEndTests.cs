using Xunit;
using SymbolLabsForge.Contracts;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace SymbolLabsForge.Tests.Integration
{
    public class SymbolForgeEndToEndTests
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ISymbolForge _symbolForge;

        public SymbolForgeEndToEndTests()
        {
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole());

            // Create a mock configuration
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>())
                .Build();

            services.AddSymbolForge(configuration);
            _serviceProvider = services.BuildServiceProvider();
            _symbolForge = _serviceProvider.GetRequiredService<ISymbolForge>();
        }

        [Theory]
        [Trait("Category", "Integration")]
        [InlineData(SymbolType.Flat, 12, 30)]
        [InlineData(SymbolType.Sharp, 20, 40)]
        [InlineData(SymbolType.Natural, 30, 80)]
        public void Generate_AllSymbolTypes_EndToEnd_Succeeds(SymbolType symbolType, int width, int height)
        {
            // Arrange
            var symbolForge = _serviceProvider.GetRequiredService<ISymbolForge>();
            var request = new SymbolRequest(
                symbolType,
                new List<Size> { new Size(width, height) },
                new List<OutputForm> { OutputForm.Skeletonized },
                1337 // Make generation deterministic
            );

            // Act
            var capsuleSet = symbolForge.Generate(request);

            // Assert
            Assert.NotNull(capsuleSet);
            Assert.NotNull(capsuleSet.Primary);
            
            if (!capsuleSet.Primary.IsValid)
            {
                // Save failing artifact for inspection
                SaveFailureArtifact(capsuleSet.Primary, symbolType, width, height);
            }

            var validationFailures = capsuleSet.Primary.ValidationResults
                .Where(r => !r.IsValid)
                .Select(r => $"{r.ValidatorName}: {r.FailureMessage}");

            Assert.True(capsuleSet.Primary.IsValid, $"Validation failed for {symbolType} ({width}x{height}):\n{string.Join("\n", validationFailures)}");
            Assert.NotEmpty(capsuleSet.Primary.ValidationResults);
        }

        private void SaveFailureArtifact(SymbolCapsule capsule, SymbolType symbolType, int width, int height)
        {
            var baseDir = Path.Combine("TestAssets", "FailureCases");
            Directory.CreateDirectory(baseDir);

            var baseFileName = $"{symbolType}_{width}x{height}";
            var imagePath = Path.Combine(baseDir, $"{baseFileName}.png");
            var jsonPath = Path.Combine(baseDir, $"{baseFileName}.json");

            // Save image
            capsule.TemplateImage.Save(imagePath);

            // Save metadata
            var data = new
            {
                capsule.Metadata,
                capsule.Metrics,
                capsule.ValidationResults
            };
            File.WriteAllText(jsonPath, JsonConvert.SerializeObject(data, Formatting.Indented));
        }
    }
}
