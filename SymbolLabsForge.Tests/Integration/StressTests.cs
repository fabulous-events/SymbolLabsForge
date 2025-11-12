using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SymbolLabsForge.Contracts;
using SixLabors.ImageSharp;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace SymbolLabsForge.Tests.Integration
{
    [TestClass]
    public class StressTests
    {
        private readonly IServiceProvider _serviceProvider;

        public StressTests()
        {
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole());
            services.AddSymbolForge();
            _serviceProvider = services.BuildServiceProvider();
        }

        [TestMethod]
        [TestCategory("Stress")]
        [TestCategory("Phase2.11")]
        public async Task Generate_100SymbolsInParallel_DoesNotCrash()
        {
            // Arrange
            var symbolForge = _serviceProvider.GetRequiredService<ISymbolForge>();
            var tasks = new List<Task>();
            int numberOfSymbols = 100;

            // Act
            for (int i = 0; i < numberOfSymbols; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    var request = new SymbolRequest(
                        SymbolType.Flat,
                        new List<Size> { new Size(12, 30) },
                        new List<OutputForm> { OutputForm.Skeletonized }
                    );
                    symbolForge.Generate(request);
                }));
            }

            await Task.WhenAll(tasks);

            // Assert
            // The primary assertion is that no exceptions were thrown and the task completed.
            Assert.IsTrue(tasks.All(t => t.IsCompletedSuccessfully));
        }
    }
}
