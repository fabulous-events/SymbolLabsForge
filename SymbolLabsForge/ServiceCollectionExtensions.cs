//===============================================================
// File: SymbolLabsForge/ServiceCollectionExtensions.cs
// Purpose: [Enhancement v1.5.0] Provides DI auto-registration.
// Requires: Scrutor (NuGet Package)
//===============================================================
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using SymbolLabsForge.Contracts;
using SymbolLabsForge.Preprocessing;
using SymbolLabsForge.Generation;
using SymbolLabsForge.Configuration;
using SymbolLabsForge.Services;

namespace SymbolLabsForge
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSymbolForge(this IServiceCollection services, IConfiguration configuration)
        {
            // Register settings
            services.Configure<AssetSettings>(configuration.GetSection("AssetSettings"));

            // Register the main orchestrator
            services.AddTransient<ISymbolForge, SymbolForge>();

            // Register asset services
            services.AddSingleton<IAssetPathProvider, AssetPathProvider>();

            services.AddSingleton<IPreprocessingStep, SkeletonizationProcessor>();
            services.AddSingleton<IMorphEngine, PixelBlendMorphEngine>();

            // Register all generators and validators from the assembly
            var assembly = typeof(SymbolForge).Assembly;

            services.Scan(scan => scan
                .FromAssemblies(assembly)
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
