using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SymbolLabsForge.Contracts;
using SixLabors.ImageSharp;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using System;
using Microsoft.Extensions.Configuration;

namespace SymbolLabsForge.Tests.Integration
{
    public class OverrideAndFallbackTests
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ISymbolForge _symbolForge;

        public OverrideAndFallbackTests()
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
        [Trait("Category", "Override")]
        [Trait("AuditTag", "Phase2.17")]
        public void Generate_WithValidatorOverride_BypassesValidationAndLogsOverride()
        {
            // Arrange
            var symbolForge = _serviceProvider.GetRequiredService<ISymbolForge>();
            var overrides = new Dictionary<string, (bool, string)>
            {
                { "Density Validator", (true, "Manual override for testing") }
            };
            var request = new SymbolRequest(
                SymbolType.Flat,
                new List<Size> { new Size(10, 10) }, // Known to fail density test
                new List<OutputForm> { OutputForm.Raw },
                ValidatorOverrides: overrides
            );

            // Act
            var capsuleSet = symbolForge.Generate(request);

            // Assert
            Assert.NotNull(capsuleSet);
            Assert.True(capsuleSet.Primary.IsValid); // Should be true because the failing validator was overridden
            Assert.Contains(capsuleSet.Primary.ValidationResults, vr => vr.ValidatorName == "Density Validator" && vr.FailureMessage.Contains("Overridden"));
        }
    }
}
