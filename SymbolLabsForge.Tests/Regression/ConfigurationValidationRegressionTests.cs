//===============================================================
// File: ConfigurationValidationRegressionTests.cs
// Author: Claude (Configuration Validation Phase 4)
// Date: 2025-11-14
// Purpose: Regression guards for fail-fast configuration validation.
//
// CONFIGURATION VALIDATION (Phase 4):
//   - Guards against missing required configuration properties
//   - Guards against invalid range values
//   - Guards against logical errors (min >= max)
//   - Ensures fail-fast behavior at startup via OptionsValidationException
//
// DEFECT HISTORY:
//   - Original Implementation: No validation, allowed empty/missing configuration
//   - Root Cause: No [Required] attributes, no .ValidateOnStart()
//   - Impact: Runtime failures when accessing misconfigured services
//   - Fix: Added [Required] attributes, .ValidateDataAnnotations(), .ValidateOnStart()
//
// VALIDATION STRATEGY:
//   - Test missing required properties (AssetSettings.RootDirectory, ForgePaths.SolutionRoot, etc.)
//   - Test invalid range values (DensityValidatorSettings thresholds)
//   - Test logical errors (MinDensityThreshold >= MaxDensityThreshold)
//   - Verify BuildServiceProvider() throws OptionsValidationException
//
// AUDIENCE: Graduate / PhD (configuration hygiene, fail-fast patterns)
//===============================================================
#nullable enable

using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SymbolLabsForge.Configuration;
using SymbolLabsForge.Validation;
using System.Collections.Generic;

namespace SymbolLabsForge.Tests.Regression
{
    public class ConfigurationValidationRegressionTests
    {
        /// <summary>
        /// REGRESSION GUARD: Prevents startup with missing AssetSettings.RootDirectory.
        ///
        /// DEFECT HISTORY (Configuration Validation Phase 2/3):
        ///   - Original Implementation: AssetSettings.RootDirectory allowed empty string
        ///   - Root Cause: No [Required] attribute, no validation at startup
        ///   - Impact: Runtime failures when accessing assets with invalid paths
        ///   - Fix: Added [Required] attribute and .ValidateOnStart()
        ///
        /// VALIDATION STRATEGY:
        ///   - Provide configuration WITHOUT AssetSettings.RootDirectory
        ///   - Attempt to build service provider
        ///   - Verify OptionsValidationException is thrown at startup
        /// </summary>
        [Fact]
        public void AssetSettings_MissingRootDirectory_FailsFast()
        {
            // Arrange: Configuration without AssetSettings.RootDirectory
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "AssetSettings:BasePath", "assets" },
                    { "AssetSettings:Images", "images" }
                    // RootDirectory is MISSING - should cause validation failure
                })
                .Build();

            var services = new ServiceCollection();
            services.AddSymbolForge(configuration);

            // Act: Build service provider (validation happens on first access due to .ValidateOnStart())
            var provider = services.BuildServiceProvider();

            // Assert: Accessing AssetSettings.Value should throw OptionsValidationException
            var exception = Assert.Throws<OptionsValidationException>(() =>
            {
                var assetSettings = provider.GetRequiredService<IOptions<AssetSettings>>().Value;
            });

            Assert.Contains("RootDirectory", exception.Message);
            Assert.Contains("required", exception.Message, System.StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// REGRESSION GUARD: Prevents startup with missing ForgePathSettings.SolutionRoot.
        ///
        /// DEFECT HISTORY (Configuration Validation Phase 2/3):
        ///   - Original Implementation: ForgePathSettings.SolutionRoot allowed empty string
        ///   - Root Cause: No [Required] attribute, no validation at startup
        ///   - Impact: CLI Program.cs only logged warning (line 50-53), didn't fail fast
        ///   - Fix: Added [Required] attribute with clear error message
        ///
        /// VALIDATION STRATEGY:
        ///   - Provide configuration WITHOUT ForgePaths.SolutionRoot
        ///   - Attempt to build service provider with ForgePathSettings
        ///   - Verify OptionsValidationException is thrown at startup
        /// </summary>
        [Fact]
        public void ForgePathSettings_MissingSolutionRoot_FailsFast()
        {
            // Arrange: Configuration without ForgePaths.SolutionRoot
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "ForgePaths:DocsRoot", "/mnt/e/ISP/docs/" }
                    // SolutionRoot is MISSING - should cause validation failure
                })
                .Build();

            var services = new ServiceCollection();
            services.AddOptions<ForgePathSettings>()
                .Bind(configuration.GetSection(ForgePathSettings.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            // Act: Build service provider (validation happens on first access)
            var provider = services.BuildServiceProvider();

            // Assert: Accessing ForgePathSettings.Value should throw OptionsValidationException
            var exception = Assert.Throws<OptionsValidationException>(() =>
            {
                var forgePathSettings = provider.GetRequiredService<IOptions<ForgePathSettings>>().Value;
            });

            Assert.Contains("SolutionRoot", exception.Message);
            Assert.Contains("required", exception.Message, System.StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// REGRESSION GUARD: Prevents startup with missing ForgePathSettings.DocsRoot.
        ///
        /// DEFECT HISTORY (Configuration Validation Phase 2/3):
        ///   - Original Implementation: ForgePathSettings.DocsRoot allowed empty string
        ///   - Root Cause: No [Required] attribute, no validation at startup
        ///   - Impact: Report generation failed at runtime with path errors
        ///   - Fix: Added [Required] attribute for DocsRoot
        ///
        /// VALIDATION STRATEGY:
        ///   - Provide configuration WITHOUT ForgePaths.DocsRoot
        ///   - Attempt to access ForgePathSettings.Value
        ///   - Verify OptionsValidationException is thrown
        /// </summary>
        [Fact]
        public void ForgePathSettings_MissingDocsRoot_FailsFast()
        {
            // Arrange: Configuration without ForgePaths.DocsRoot
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "ForgePaths:SolutionRoot", "/mnt/e/ISP/Programs/SymbolLabsForge/" }
                    // DocsRoot is MISSING - should cause validation failure
                })
                .Build();

            var services = new ServiceCollection();
            services.AddOptions<ForgePathSettings>()
                .Bind(configuration.GetSection(ForgePathSettings.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            // Act: Build service provider (validation happens on first access)
            var provider = services.BuildServiceProvider();

            // Assert: Accessing ForgePathSettings.Value should throw OptionsValidationException
            var exception = Assert.Throws<OptionsValidationException>(() =>
            {
                var forgePathSettings = provider.GetRequiredService<IOptions<ForgePathSettings>>().Value;
            });

            Assert.Contains("DocsRoot", exception.Message);
            Assert.Contains("required", exception.Message, System.StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// REGRESSION GUARD: Prevents startup with invalid DensityValidatorSettings range.
        ///
        /// DEFECT HISTORY (Configuration Validation Phase 2):
        ///   - Original Implementation: No range validation, allowed thresholds > 1.0
        ///   - Root Cause: No [Range] attributes on threshold properties
        ///   - Impact: Density validator could have illogical thresholds (e.g., 5.0 = 500%)
        ///   - Fix: Added [Range(0.0, 1.0)] attributes with clear error messages
        ///
        /// VALIDATION STRATEGY:
        ///   - Provide configuration with MinDensityThreshold > 1.0
        ///   - Attempt to access DensityValidatorSettings.Value
        ///   - Verify OptionsValidationException is thrown
        /// </summary>
        [Fact]
        public void DensityValidatorSettings_InvalidRange_FailsFast()
        {
            // Arrange: Configuration with invalid MinDensityThreshold (> 1.0)
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Validation:Density:MinDensityThreshold", "1.5" },  // INVALID: > 1.0
                    { "Validation:Density:MaxDensityThreshold", "0.8" }
                })
                .Build();

            var services = new ServiceCollection();
            services.AddOptions<DensityValidatorSettings>()
                .Bind(configuration.GetSection(DensityValidatorSettings.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            // Act: Build service provider (validation happens on first access)
            var provider = services.BuildServiceProvider();

            // Assert: Accessing DensityValidatorSettings.Value should throw OptionsValidationException
            var exception = Assert.Throws<OptionsValidationException>(() =>
            {
                var densitySettings = provider.GetRequiredService<IOptions<DensityValidatorSettings>>().Value;
            });

            Assert.Contains("MinDensityThreshold", exception.Message);
            Assert.Contains("0.0", exception.Message);
            Assert.Contains("1.0", exception.Message);
        }

        /// <summary>
        /// REGRESSION GUARD: Prevents startup with MinDensityThreshold >= MaxDensityThreshold.
        ///
        /// DEFECT HISTORY (Configuration Validation Phase 2):
        ///   - Original Implementation: No logical validation between min and max
        ///   - Root Cause: No IValidatableObject implementation
        ///   - Impact: Density validator could have inverted thresholds (min > max)
        ///   - Fix: Added IValidatableObject.Validate() method
        ///
        /// VALIDATION STRATEGY:
        ///   - Provide configuration with MinDensityThreshold >= MaxDensityThreshold
        ///   - Attempt to access DensityValidatorSettings.Value
        ///   - Verify OptionsValidationException is thrown
        /// </summary>
        [Fact]
        public void DensityValidatorSettings_MinGreaterThanMax_FailsFast()
        {
            // Arrange: Configuration with Min >= Max (logical error)
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Validation:Density:MinDensityThreshold", "0.8" },  // INVALID: Min >= Max
                    { "Validation:Density:MaxDensityThreshold", "0.5" }
                })
                .Build();

            var services = new ServiceCollection();
            services.AddOptions<DensityValidatorSettings>()
                .Bind(configuration.GetSection(DensityValidatorSettings.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            // Act: Build service provider (validation happens on first access)
            var provider = services.BuildServiceProvider();

            // Assert: Accessing DensityValidatorSettings.Value should throw OptionsValidationException
            var exception = Assert.Throws<OptionsValidationException>(() =>
            {
                var densitySettings = provider.GetRequiredService<IOptions<DensityValidatorSettings>>().Value;
            });

            Assert.Contains("MinDensityThreshold", exception.Message);
            Assert.Contains("MaxDensityThreshold", exception.Message);
            Assert.Contains("less than", exception.Message, System.StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// REGRESSION GUARD: Verifies valid configuration passes validation.
        ///
        /// VALIDATION STRATEGY:
        ///   - Provide complete, valid configuration for all settings
        ///   - Build service provider successfully
        ///   - Verify all IOptions<T> can be resolved without exceptions
        /// </summary>
        [Fact]
        public void ValidConfiguration_PassesValidation()
        {
            // Arrange: Complete, valid configuration
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "AssetSettings:RootDirectory", "/tmp/test-assets" },
                    { "AssetSettings:BasePath", "assets" },
                    { "AssetSettings:Images", "images" },
                    { "ForgePaths:SolutionRoot", "/mnt/e/ISP/Programs/SymbolLabsForge/" },
                    { "ForgePaths:DocsRoot", "/mnt/e/ISP/docs/" },
                    { "Validation:Density:MinDensityThreshold", "0.05" },
                    { "Validation:Density:MaxDensityThreshold", "0.12" }
                })
                .Build();

            var services = new ServiceCollection();
            services.AddSymbolForge(configuration);
            services.AddOptions<ForgePathSettings>()
                .Bind(configuration.GetSection(ForgePathSettings.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            // Act: Build service provider (should not throw)
            // Note: ValidateOnStart() triggers validation when first accessed, not during BuildServiceProvider()
            var provider = services.BuildServiceProvider();

            // Assert: All settings should be resolvable (validation happens here due to ValidateOnStart)
            var assetSettings = provider.GetRequiredService<IOptions<AssetSettings>>().Value;
            var forgePathSettings = provider.GetRequiredService<IOptions<ForgePathSettings>>().Value;
            var densitySettings = provider.GetRequiredService<IOptions<DensityValidatorSettings>>().Value;

            Assert.NotNull(assetSettings);
            Assert.Equal("/tmp/test-assets", assetSettings.RootDirectory);

            Assert.NotNull(forgePathSettings);
            Assert.Equal("/mnt/e/ISP/Programs/SymbolLabsForge/", forgePathSettings.SolutionRoot);
            Assert.Equal("/mnt/e/ISP/docs/", forgePathSettings.DocsRoot);

            Assert.NotNull(densitySettings);
            Assert.Equal(0.05f, densitySettings.MinDensityThreshold);
            Assert.Equal(0.12f, densitySettings.MaxDensityThreshold);
        }
    }
}
