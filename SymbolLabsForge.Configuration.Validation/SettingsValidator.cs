//===============================================================
// File: SettingsValidator.cs
// Author: Claude (Phase 8.5 - Configuration Hygiene Framework)
// Date: 2025-11-14
// Purpose: Programmatic validation utility for settings classes.
//
// PHASE 8.5: MODULARIZATION - CONFIGURATION VALIDATION
//   - Provides programmatic validation API (useful for testing)
//   - Complements DI-based validation (OptionsBuilder extensions)
//   - Enables standalone validation without full DI container
//
// USAGE SCENARIOS:
//   - Unit testing settings classes
//   - Validating appsettings.json files in CI/CD pipelines
//   - Pre-deployment configuration verification
//   - Diagnostic tools and configuration auditors
//
// AUDIENCE: Graduate / PhD (testing patterns, validation frameworks)
//===============================================================
#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace SymbolLabsForge.Configuration.Validation
{
    /// <summary>
    /// Utility class for programmatic validation of settings objects.
    /// </summary>
    public static class SettingsValidator
    {
        /// <summary>
        /// Validates a settings object using data annotations.
        /// </summary>
        /// <typeparam name="TSettings">The settings type</typeparam>
        /// <param name="settings">The settings instance to validate</param>
        /// <returns>Validation result containing success status and error messages</returns>
        /// <remarks>
        /// This method provides the same validation as OptionsBuilder.ValidateDataAnnotations(),
        /// but can be called programmatically without DI container.
        ///
        /// USAGE:
        /// <code>
        /// var settings = new AssetSettings { RootDirectory = "" };  // Invalid!
        /// var result = SettingsValidator.Validate(settings);
        /// if (!result.IsValid)
        /// {
        ///     Console.WriteLine($"Validation failed: {result.ErrorMessage}");
        /// }
        /// </code>
        /// </remarks>
        public static SettingsValidationResult Validate<TSettings>(TSettings settings) where TSettings : class
        {
            if (settings == null)
            {
                return new SettingsValidationResult(
                    false,
                    "Settings object cannot be null",
                    new List<string> { "Settings object is null" });
            }

            var validationContext = new ValidationContext(settings);
            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

            bool isValid = Validator.TryValidateObject(
                settings,
                validationContext,
                validationResults,
                validateAllProperties: true);

            if (!isValid)
            {
                var errors = validationResults
                    .Select(vr => vr.ErrorMessage ?? "Unknown validation error")
                    .ToList();

                var errorMessage = string.Join(Environment.NewLine, errors);
                return new SettingsValidationResult(false, errorMessage, errors);
            }

            return new SettingsValidationResult(true, string.Empty, new List<string>());
        }

        /// <summary>
        /// Validates a settings object and throws exception if invalid.
        /// </summary>
        /// <typeparam name="TSettings">The settings type</typeparam>
        /// <param name="settings">The settings instance to validate</param>
        /// <exception cref="SettingsValidationException">Thrown if validation fails</exception>
        /// <remarks>
        /// Useful for fail-fast scenarios where invalid configuration should immediately halt execution.
        ///
        /// USAGE:
        /// <code>
        /// var settings = LoadSettings();
        /// SettingsValidator.ValidateOrThrow(settings);  // Throws if invalid
        /// // Proceed with valid settings
        /// </code>
        /// </remarks>
        public static void ValidateOrThrow<TSettings>(TSettings settings) where TSettings : class
        {
            var result = Validate(settings);
            if (!result.IsValid)
            {
                throw new SettingsValidationException(result.ErrorMessage, result.Errors);
            }
        }
    }

    /// <summary>
    /// Result of settings validation containing success status and error messages.
    /// </summary>
    /// <param name="IsValid">True if validation passed, false otherwise</param>
    /// <param name="ErrorMessage">Concatenated error message (empty if valid)</param>
    /// <param name="Errors">List of individual validation error messages</param>
    public record SettingsValidationResult(
        bool IsValid,
        string ErrorMessage,
        IReadOnlyList<string> Errors);

    /// <summary>
    /// Exception thrown when settings validation fails.
    /// </summary>
    public class SettingsValidationException : Exception
    {
        /// <summary>
        /// List of individual validation error messages.
        /// </summary>
        public IReadOnlyList<string> Errors { get; }

        /// <summary>
        /// Initializes a new instance of SettingsValidationException.
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="errors">List of validation errors</param>
        public SettingsValidationException(string message, IReadOnlyList<string> errors)
            : base(message)
        {
            Errors = errors;
        }
    }
}
