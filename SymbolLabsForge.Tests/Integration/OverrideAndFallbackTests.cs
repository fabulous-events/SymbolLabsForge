using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SymbolLabsForge.Contracts;
using SixLabors.ImageSharp;
using System.Collections.Generic;
using System.Linq;

namespace SymbolLabsForge.Tests.Integration
{
    [TestClass]
    public class OverrideAndFallbackTests
    {
        private readonly IServiceProvider _serviceProvider;

        public OverrideAndFallbackTests()
        {
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole());
            services.AddSymbolForge();
            _serviceProvider = services.BuildServiceProvider();
        }

        [TestMethod]
        [TestCategory("Override")]
        [TestCategory("Phase2.17")]
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
            Assert.IsNotNull(capsuleSet);
            Assert.IsTrue(capsuleSet.Primary.IsValid); // Should be true because the failing validator was overridden
            Assert.IsTrue(capsuleSet.Primary.ValidationResults.Any(vr => vr.ValidatorName == "Density Validator" && vr.FailureMessage.Contains("Overridden")));
        }
    }
}
