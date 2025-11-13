using Xunit;
using SymbolLabsForge.Contracts;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using Microsoft.Extensions.Logging;

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
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>())
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