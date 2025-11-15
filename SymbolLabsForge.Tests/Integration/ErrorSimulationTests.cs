using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using Microsoft.Extensions.Logging;
using SymbolLabsForge.Contracts;

namespace SymbolLabsForge.Tests.Integration
{
    public class ErrorSimulationTests
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ISymbolForge _symbolForge;

        public ErrorSimulationTests()
        {
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole());

            // Create a mock configuration
            var testConfig = new Dictionary<string, string?>
            {
                { "Validation:Density:MaxDensityThreshold", "0.95" },
                { "AssetSettings:RootDirectory", Path.GetTempPath() }
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(testConfig)
                .Build();

            services.AddSymbolForge(configuration);
            _serviceProvider = services.BuildServiceProvider();
            _symbolForge = _serviceProvider.GetRequiredService<ISymbolForge>();
        }

        [Fact]
        [Trait("Category", "ErrorSimulation")]
        [Trait("AuditTag", "Phase2.12")]
        public void Generate_WithInvalidDimensions_ThrowsArgumentException()
        {
            // Arrange
            var symbolForge = _serviceProvider.GetRequiredService<ISymbolForge>();
            var request = new SymbolRequest(
                SymbolType.Flat,
                new List<Size> { new Size(0, 0) }, // Invalid dimensions
                new List<OutputForm> { OutputForm.Raw }
            );

            // Act & Assert
            // We expect the underlying ImageSharp library or our generator to throw an exception.
            Assert.ThrowsAny<ArgumentException>(() => symbolForge.Generate(request));
        }
    }
}