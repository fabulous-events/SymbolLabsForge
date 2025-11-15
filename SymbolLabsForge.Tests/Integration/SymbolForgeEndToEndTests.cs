using Xunit;
using SymbolLabsForge.Contracts;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;
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

            // Create a mock configuration for the test
            var testConfig = new Dictionary<string, string?>
            {
                { "Validation:Density:MaxDensityThreshold", "0.95" }, // 95%
                { "AssetSettings:RootDirectory", Path.GetTempPath() }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(testConfig)
                .Build();

            services.AddSymbolForge(configuration);
            _serviceProvider = services.BuildServiceProvider();
            _symbolForge = _serviceProvider.GetRequiredService<ISymbolForge>();
        }

        public static IEnumerable<object[]> SymbolTestData()
        {
            foreach (var symbolType in Enum.GetValues(typeof(SymbolType)))
            {
                if ((SymbolType)symbolType == SymbolType.Unknown) continue;
                yield return new object[] { symbolType, 100, 100 };
                yield return new object[] { symbolType, 256, 256 };
            }
        }

        [Theory]
        [MemberData(nameof(SymbolTestData))]
        public void Generate_AllSymbolTypes_EndToEnd_Succeeds(SymbolType symbolType, int width, int height)
        {
            // Arrange
            var request = new SymbolRequest(
                symbolType,
                new List<Size> { new Size(width, height) },
                new List<OutputForm> { OutputForm.Binarized }
            );

            // Act
            var capsuleSet = _symbolForge.Generate(request);
            var primaryCapsule = capsuleSet.Primary;

            // Assert
            Assert.True(primaryCapsule.IsValid, $"Validation failed for {symbolType} ({width}x{height}):\n{string.Join("\n", primaryCapsule.ValidationResults.Where(r => !r.IsValid).Select(r => $"{r.ValidatorName}: {r.FailureMessage}"))}");
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
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            };
            File.WriteAllText(jsonPath, JsonSerializer.Serialize(data, jsonOptions));
        }
    }
}
