//===============================================================
// File: IValidatableSettings.cs
// Author: Claude (Phase 8.5 - Configuration Hygiene Framework)
// Date: 2025-11-14
// Purpose: Marker interface for settings classes with validation.
//
// PHASE 8.5: MODULARIZATION - CONFIGURATION VALIDATION
//   - Marker interface for discoverability and documentation
//   - Signals that a settings class participates in validation framework
//   - Not required for validation to work, but improves code clarity
//
// DESIGN RATIONALE:
//   - Marker interface pattern (no methods required)
//   - Used for:
//     * Documentation (signals validation intent to developers)
//     * Discoverability (can scan assembly for IValidatableSettings implementations)
//     * Convention enforcement (code review checklist)
//
// USAGE:
//   public class MySettings : IValidatableSettings
//   {
//       [Required]
//       public string ImportantProperty { get; set; } = string.Empty;
//   }
//
// AUDIENCE: Graduate / PhD (marker interfaces, design patterns)
//===============================================================
#nullable enable

namespace SymbolLabsForge.Configuration.Validation
{
    /// <summary>
    /// Marker interface for settings classes that participate in validation framework.
    /// Implementing this interface signals that the class uses data annotations
    /// and/or IValidatableObject for configuration validation.
    /// </summary>
    /// <remarks>
    /// IMPLEMENTATION CHECKLIST:
    /// ✓ Add [Required] to mandatory properties (file paths, connection strings, etc.)
    /// ✓ Add [Range] to numeric properties with valid bounds (thresholds, timeouts, etc.)
    /// ✓ Implement IValidatableObject for cross-property validation
    /// ✓ Define const string SectionName for consistent binding
    /// ✓ Register with .ValidateStrictly() in DI configuration
    ///
    /// GOVERNANCE:
    /// - All settings classes in SymbolLabsForge should implement this interface
    /// - Code reviews should verify proper validation attributes
    /// - Integration tests should verify fail-fast behavior
    ///
    /// EXAMPLE:
    /// <code>
    /// public class MySettings : IValidatableSettings
    /// {
    ///     public const string SectionName = "MySettings";
    ///
    ///     [Required(ErrorMessage = "MySettings.DatabasePath is required.")]
    ///     public string DatabasePath { get; set; } = string.Empty;
    ///
    ///     [Range(0.0, 1.0, ErrorMessage = "Threshold must be between 0.0 and 1.0.")]
    ///     public double Threshold { get; set; } = 0.5;
    /// }
    /// </code>
    ///
    /// REGISTRATION:
    /// <code>
    /// services.AddOptions&lt;MySettings&gt;()
    ///     .Bind(configuration.GetSection(MySettings.SectionName))
    ///     .ValidateStrictly();
    /// </code>
    /// </remarks>
    public interface IValidatableSettings
    {
        // Marker interface - no methods required
        // Presence of this interface signals validation framework participation
    }
}
