using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SymbolLabsForge.Contracts;
using SixLabors.ImageSharp;
using System.Collections.Generic;
using System;

namespace SymbolLabsForge.Tests.Integration
{
    public class ErrorSimulationTests
    {
        private readonly IServiceProvider _serviceProvider;

        public ErrorSimulationTests()
        {
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole());
            services.AddSymbolForge();
            _serviceProvider = services.BuildServiceProvider();
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