using Xunit;
using SymbolLabsForge.Contracts;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using Microsoft.Extensions.Logging;

namespace SymbolLabsForge.Tests.Integration
{
    public class StressTests
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ISymbolForge _symbolForge;

        public StressTests()
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
        [Trait("Category", "Stress")]
        [Trait("AuditTag", "Phase2.11")]
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
            Assert.True(tasks.All(t => t.IsCompletedSuccessfully));
        }
    }
}
