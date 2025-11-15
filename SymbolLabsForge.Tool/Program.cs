using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SymbolLabsForge;
using SymbolLabsForge.Contracts;
using SymbolLabsForge.Services;
using SymbolLabsForge.ImageProcessing.Utilities;
using SymbolLabsForge.Generators;
using SymbolLabsForge.Validation;
using System.IO;

namespace SymbolLabsForge.Tool
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            var services = new ServiceCollection();

            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            // Configure logging
            services.AddLogging(configure => configure.AddConsole());

            // Register SymbolForge and its dependencies
            services.AddSymbolForge(configuration);

            // CONFIGURATION VALIDATION (Phase 3):
            // ForgePathSettings registration with fail-fast validation.
            // Note: AssetSettings is already registered in AddSymbolForge() - no duplicate needed.
            services.AddOptions<SymbolLabsForge.Configuration.ForgePathSettings>()
                .Bind(configuration.GetSection(SymbolLabsForge.Configuration.ForgePathSettings.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            // Register application-specific services
            services.AddTransient<CapsuleExporter>();
            services.AddSingleton<CapsuleRegistryManager>();

            // Register the forms
            services.AddTransient<FormSymbolRequest>();
            services.AddTransient<FormResultsViewer>();

            using (var serviceProvider = services.BuildServiceProvider())
            {
                Application.Run(serviceProvider.GetRequiredService<FormSymbolRequest>());
            }
        }
    }
}
