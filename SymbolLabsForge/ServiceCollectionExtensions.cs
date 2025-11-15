//===============================================================
// File: SymbolLabsForge/ServiceCollectionExtensions.cs
// Purpose: [Enhancement v1.5.0] Provides DI auto-registration.
// Updated: 2025-11-14 (Claude - Phase 8.5: Configuration.Validation)
// Requires: Scrutor (NuGet Package)
//
// PHASE 8.5: MODULARIZATION - CONFIGURATION VALIDATION
//   - Migrated from .ValidateDataAnnotations().ValidateOnStart() to .ValidateStrictly()
//   - Uses SymbolLabsForge.Configuration.Validation extension methods
//   - Same fail-fast behavior, now using reusable framework
//
// CONFIGURATION VALIDATION (Phase 3):
//   - Added .ValidateDataAnnotations() to enforce [Required] and [Range] attributes
//   - Added .ValidateOnStart() for fail-fast behavior at startup
//   - Prevents runtime failures from invalid configuration
//
// DEFECT HISTORY:
//   - Original Implementation: Simple .Configure() with no validation
//   - Root Cause: No fail-fast checks, allowed invalid/missing configuration
//   - Impact: Runtime failures when accessing misconfigured services
//   - Fix: Enabled validation chain for all IOptions<T> registrations
//
// AUDIENCE: Undergraduate / Graduate (DI configuration hygiene)
//===============================================================
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SymbolLabsForge.Contracts;
using SymbolLabsForge.ImageProcessing.Utilities;
using SymbolLabsForge.Generation;
using SymbolLabsForge.Configuration;
using SymbolLabsForge.Services;
using SymbolLabsForge.Validation;
using SymbolLabsForge.Configuration.Validation;

namespace SymbolLabsForge
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSymbolForge(this IServiceCollection services, IConfiguration configuration)
        {
            // Register settings with fail-fast validation (Phase 8.5: Using Configuration.Validation framework)
            services.AddOptions<AssetSettings>()
                .Bind(configuration.GetSection(AssetSettings.SectionName))
                .ValidateStrictly();  // Equivalent to ValidateDataAnnotations + ValidateOnStart

            services.AddOptions<DensityValidatorSettings>()
                .Bind(configuration.GetSection(DensityValidatorSettings.SectionName))
                .ValidateStrictly();  // Equivalent to ValidateDataAnnotations + ValidateOnStart

            // Register the main orchestrator
            services.AddTransient<ISymbolForge, SymbolForge>();

            // Register asset services
            services.AddSingleton<IAssetPathProvider, AssetPathProvider>();

            services.AddSingleton<IMorphEngine, PixelBlendMorphEngine>();

            // Register all generators and validators from the assembly
            var coreAssembly = typeof(SymbolForge).Assembly;
            var imageProcessingAssembly = typeof(PixelUtils).Assembly; // Phase 8.8: Scan ImageProcessing.Utilities for extracted utilities

            services.Scan(scan => scan
                .FromAssemblies(coreAssembly, imageProcessingAssembly)
                .AddClasses(classes => classes.AssignableTo<ISymbolGenerator>())
                    .AsImplementedInterfaces()
                    .WithTransientLifetime()
                .AddClasses(classes => classes.AssignableTo<IValidator>())
                    .AsImplementedInterfaces()
                    .WithTransientLifetime()
                .AddClasses(classes => classes.AssignableTo<IPreprocessingStep>())
                    .AsImplementedInterfaces()
                    .WithTransientLifetime());

            return services;
        }
    }
}
