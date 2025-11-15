//===============================================================
// File: AssetSettings.cs
// Author: Gemini (Original), Claude (Configuration Validation)
// Date: 2025-11-14
// Purpose: Configuration for asset path resolution with fail-fast validation.
//
// CONFIGURATION VALIDATION (Phase 2):
//   - Added [Required] attributes to enforce mandatory properties
//   - Added SectionName constant for consistent configuration binding
//   - RootDirectory is critical for asset resolution and must be provided
//
// DEFECT HISTORY:
//   - Original Implementation: No validation, allowed empty RootDirectory
//   - Root Cause: No fail-fast validation at startup
//   - Impact: Runtime failures when accessing assets with invalid paths
//   - Fix: Added [Required] attributes and SectionName constant
//
// AUDIENCE: Undergraduate / Graduate (configuration hygiene)
//===============================================================
using System.ComponentModel.DataAnnotations;

namespace SymbolLabsForge.Configuration
{
    /// <summary>
    /// Contains settings for asset path resolution.
    /// </summary>
    public class AssetSettings
    {
        public const string SectionName = "AssetSettings";

        /// <summary>
        /// Gets or sets the base directory where assets are stored.
        /// This can be a relative or absolute path.
        /// </summary>
        public string BasePath { get; set; } = "assets";

        /// <summary>
        /// Gets or sets the root directory for all assets.
        /// This is a REQUIRED property and must be set in appsettings.json.
        /// </summary>
        [Required(ErrorMessage = "AssetSettings.RootDirectory is required. Set this in appsettings.json.")]
        public string RootDirectory { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the subdirectory for images within RootDirectory.
        /// </summary>
        public string Images { get; set; } = string.Empty;
    }
}
