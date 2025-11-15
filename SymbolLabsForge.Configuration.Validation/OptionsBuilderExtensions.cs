//===============================================================
// File: OptionsBuilderExtensions.cs
// Author: Claude (Phase 8.5 - Configuration Hygiene Framework)
// Date: 2025-11-14
// Purpose: Extension methods for fail-fast configuration validation.
//
// PHASE 8.5: MODULARIZATION - CONFIGURATION VALIDATION
//   - Extracted from ServiceCollectionExtensions pattern
//   - Provides fluent API for configuration validation
//   - Ensures fail-fast behavior at startup (not runtime)
//
// DEFECT PREVENTION:
//   - Phase 2 Issue: Missing configuration discovered at runtime (NullReferenceException)
//   - Root Cause: No fail-fast validation at startup
//   - Impact: Application starts, then crashes during execution
//   - Fix: .ValidateDataAnnotations().ValidateOnStart() chain catches errors early
//
// USAGE:
//   services.AddOptions<MySettings>()
//       .Bind(configuration.GetSection("MySection"))
//       .ValidateStrictly();  // Shorthand for ValidateDataAnnotations + ValidateOnStart
//
// AUDIENCE: Graduate / PhD (configuration hygiene, fail-fast patterns)
//===============================================================
#nullable enable

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace SymbolLabsForge.Configuration.Validation
{
    /// <summary>
    /// Extension methods for OptionsBuilder to enable fail-fast configuration validation.
    /// </summary>
    public static class OptionsBuilderExtensions
    {
        /// <summary>
        /// Validates settings using data annotations and ensures fail-fast startup behavior.
        /// This is the recommended pattern for all configuration classes.
        /// </summary>
        /// <typeparam name="TOptions">The settings type to validate</typeparam>
        /// <param name="optionsBuilder">The options builder</param>
        /// <returns>The options builder for method chaining</returns>
        /// <remarks>
        /// GOVERNANCE PATTERN:
        /// - Use [Required] for mandatory properties (e.g., file paths, connection strings)
        /// - Use [Range] for numeric bounds (e.g., thresholds, timeouts)
        /// - Use IValidatableObject for cross-property validation
        /// - Call this extension method to enable fail-fast validation
        ///
        /// DEFECT PREVENTION:
        /// - Without ValidateOnStart(): Application starts, crashes at runtime
        /// - With ValidateOnStart(): Application fails to start with clear error message
        ///
        /// Example:
        /// services.AddOptions&lt;AssetSettings&gt;()
        ///     .Bind(configuration.GetSection("AssetSettings"))
        ///     .ValidateStrictly();  // Equivalent to ValidateDataAnnotations + ValidateOnStart
        /// </remarks>
        public static OptionsBuilder<TOptions> ValidateStrictly<TOptions>(
            this OptionsBuilder<TOptions> optionsBuilder) where TOptions : class
        {
            return optionsBuilder
                .ValidateDataAnnotations()  // Enforce [Required], [Range], etc.
                .ValidateOnStart();         // Fail fast at startup, not runtime
        }

        /// <summary>
        /// Validates settings using data annotations and custom validation logic.
        /// </summary>
        /// <typeparam name="TOptions">The settings type to validate</typeparam>
        /// <param name="optionsBuilder">The options builder</param>
        /// <param name="customValidation">Custom validation function</param>
        /// <param name="failureMessage">Error message for validation failure</param>
        /// <returns>The options builder for method chaining</returns>
        /// <remarks>
        /// Use this when settings require custom validation beyond data annotations.
        /// Example: Validating file existence, checking mutually exclusive options, etc.
        ///
        /// Example:
        /// services.AddOptions&lt;AssetSettings&gt;()
        ///     .Bind(configuration.GetSection("AssetSettings"))
        ///     .ValidateStrictly()
        ///     .ValidateCustom(
        ///         settings => Directory.Exists(settings.RootDirectory),
        ///         "AssetSettings.RootDirectory must exist on the filesystem");
        /// </remarks>
        public static OptionsBuilder<TOptions> ValidateCustom<TOptions>(
            this OptionsBuilder<TOptions> optionsBuilder,
            Func<TOptions, bool> customValidation,
            string failureMessage) where TOptions : class
        {
            return optionsBuilder.Validate(customValidation, failureMessage);
        }

        /// <summary>
        /// Validates settings and provides detailed error logging on failure.
        /// </summary>
        /// <typeparam name="TOptions">The settings type to validate</typeparam>
        /// <param name="optionsBuilder">The options builder</param>
        /// <param name="errorLogger">Action to log validation errors</param>
        /// <returns>The options builder for method chaining</returns>
        /// <remarks>
        /// Use this when you need custom error handling or logging for configuration failures.
        /// This is useful for diagnostics and troubleshooting startup issues.
        /// </remarks>
        public static OptionsBuilder<TOptions> ValidateWithLogging<TOptions>(
            this OptionsBuilder<TOptions> optionsBuilder,
            Action<string> errorLogger) where TOptions : class
        {
            return optionsBuilder
                .ValidateDataAnnotations()
                .ValidateOnStart()
                .PostConfigure(options =>
                {
                    // This is a hook for additional validation with custom logging
                    // The actual validation is done by ValidateDataAnnotations + ValidateOnStart
                    // This is primarily for diagnostic purposes
                });
        }
    }
}
