using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SymbolLabsForge.Contracts;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using SymbolLabsForge;
using SixLabors.ImageSharp;
using Microsoft.Extensions.Configuration;

namespace SymbolLabsForge.Tests.Integration
{
    using SymbolLabsForge;
    public class OverrideAndFallbackTests
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ISymbolForge _symbolForge;

        private readonly ITestOutputHelper _output;

        public OverrideAndFallbackTests(ITestOutputHelper output)
        {
            _output = output;
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSymbolForge(new ConfigurationBuilder().Build())
                .BuildServiceProvider();

            _symbolForge = serviceProvider.GetRequiredService<ISymbolForge>();
        }

        [Fact]
        public void Generate_WithValidatorOverride_BypassesValidationAndLogsOverride()
        {
            // Arrange
            var request = new SymbolRequest(
                SymbolType.Flat,
                new List<Size> { new Size(20, 40) },
                new List<OutputForm> { OutputForm.Binarized },
                null,
                null,
                new Dictionary<string, (bool Overridden, string Reason)>
                {
                    { "Density Validator", (true, "Manual override for testing") },
                    { "ContrastValidator", (true, "Manual override for testing") },
                    { "StructureValidator", (true, "Manual override for testing") }
                }
            );

            // Act
            var capsuleSet = _symbolForge.Generate(request);
            var primaryCapsule = capsuleSet.Primary;

            // Assert
            foreach (var result in primaryCapsule.ValidationResults)
            {
                _output.WriteLine($"Validator: {result.ValidatorName}, IsValid: {result.IsValid}, Message: {result.FailureMessage}");
            }
            Assert.True(primaryCapsule.IsValid);
            Assert.Contains(primaryCapsule.ValidationResults, r => r.ValidatorName == "Density Validator" && r.IsValid);
        }
    }
}
