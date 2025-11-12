using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SymbolLabsForge.Contracts;
using SixLabors.ImageSharp;
using System.Collections.Generic;

namespace SymbolLabsForge.Tests.Integration
{
    [TestClass]
    public class SymbolForgeEndToEndTests
    {
        private readonly IServiceProvider _serviceProvider;

        public SymbolForgeEndToEndTests()
        {
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole());
            services.AddSymbolForge(); // Use the extension method to register all services
            _serviceProvider = services.BuildServiceProvider();
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("Flat")]
        public void Generate_FlatSymbol_EndToEnd_Succeeds()
        {
            Generate_AllSymbolTypes_EndToEnd_Succeeds(SymbolType.Flat, 12, 30);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("Sharp")]
        public void Generate_SharpSymbol_EndToEnd_Succeeds()
        {
            Generate_AllSymbolTypes_EndToEnd_Succeeds(SymbolType.Sharp, 20, 40);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("Natural")]
        public void Generate_NaturalSymbol_EndToEnd_Succeeds()
        {
            Generate_AllSymbolTypes_EndToEnd_Succeeds(SymbolType.Natural, 30, 80);
        }

        private void Generate_AllSymbolTypes_EndToEnd_Succeeds(SymbolType symbolType, int width, int height)
        {
            // Arrange
            var symbolForge = _serviceProvider.GetRequiredService<ISymbolForge>();
            var request = new SymbolRequest(
                symbolType,
                new List<Size> { new Size(width, height) },
                new List<OutputForm> { OutputForm.Skeletonized }
            );

            // Act
            var capsuleSet = symbolForge.Generate(request);

            // Assert
            Assert.IsNotNull(capsuleSet);
            Assert.IsNotNull(capsuleSet.Primary);
            // We expect this to be true for placeholders, but real tests might expect false
            // until all processors are fully implemented.
            Assert.IsTrue(capsuleSet.Primary.IsValid); 
            Assert.IsNotNull(capsuleSet.Primary.ValidationResults);
        }
    }
}
