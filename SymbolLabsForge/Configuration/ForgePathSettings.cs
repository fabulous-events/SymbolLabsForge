//===============================================================
// File: ForgePathSettings.cs
// Author: Gemini (Original), Claude (Configuration Validation)
// Date: 2025-11-14
// Purpose: Centralized path configuration with fail-fast validation.
//
// CONFIGURATION VALIDATION (Phase 2):
//   - Added [Required] attributes to enforce mandatory properties
//   - SolutionRoot and DocsRoot are CRITICAL for all derived paths
//   - Without these, all computed paths (AssetRoot, BuildReport, etc.) are invalid
//
// DEFECT HISTORY:
//   - Original Implementation: No validation, allowed empty SolutionRoot/DocsRoot
//   - Root Cause: No fail-fast validation at startup
//   - Impact: CLI Program.cs only logged warning (line 50-53), didn't fail fast
//   - Fix: Added [Required] attributes with clear error messages
//
// ARCHITECTURE:
//   - SolutionRoot and DocsRoot are the ONLY properties set in appsettings.json
//   - All other paths (AssetRoot, BuildReport, etc.) are computed from these
//   - This ensures single source of truth for path resolution
//
// AUDIENCE: Undergraduate / Graduate (configuration hygiene)
//===============================================================
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SymbolLabsForge.Configuration
{
    public class ForgePathSettings
    {
        public const string SectionName = "ForgePaths";

        /// <summary>
        /// The absolute path to the solution root. All other paths are derived from this.
        /// This is a REQUIRED property and must be set in appsettings.json.
        /// </summary>
        [Required(ErrorMessage = "ForgePaths.SolutionRoot is required. Set this in appsettings.json to the absolute path of the SymbolLabsForge solution folder.")]
        public string SolutionRoot { get; set; } = string.Empty;

        /// <summary>
        /// The absolute path to the docs root. Required for report and log generation.
        /// This is a REQUIRED property and must be set in appsettings.json.
        /// </summary>
        [Required(ErrorMessage = "ForgePaths.DocsRoot is required. Set this in appsettings.json to the absolute path of the docs folder.")]
        public string DocsRoot { get; set; } = string.Empty;

        [JsonIgnore]
        public string AssetRoot => Path.Combine(SolutionRoot, "SymbolLabsForge", "assets");

        [JsonIgnore]
        public string BuildReport => Path.Combine(DocsRoot, "reports", "build-diagnostic.log");

        [JsonIgnore]
        public string SessionComplianceLog => Path.Combine(DocsRoot, "logs", "SessionComplianceLog.md");

        [JsonIgnore]
        public string ReplayBundleAuditReport => Path.Combine(DocsRoot, "reports", "ReplayBundleAudit.md");

        [JsonIgnore]
        public string SyntheticSymbolOutput => Path.Combine(AssetRoot, "synthetic");
    }
}
