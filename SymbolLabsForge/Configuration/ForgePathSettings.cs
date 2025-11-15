using System.Text.Json.Serialization;

namespace SymbolLabsForge.Configuration
{
    public class ForgePathSettings
    {
        public const string SectionName = "ForgePaths";

        /// <summary>
        /// The absolute path to the solution root. All other paths are derived from this.
        /// This is the ONLY path that should be set in appsettings.json.
        /// </summary>
        public string SolutionRoot { get; set; } = string.Empty;

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
