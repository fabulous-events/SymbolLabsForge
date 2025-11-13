namespace SymbolLabsForge.Configuration
{
    /// <summary>
    /// Contains settings for asset path resolution.
    /// </summary>
    public class AssetSettings
    {
        /// <summary>
        /// Gets or sets the base directory where assets are stored.
        /// This can be a relative or absolute path.
        /// </summary>
        public string BasePath { get; set; } = "assets";
        public string RootDirectory { get; set; }
        public string Images { get; set; }
        public bool SandboxMode { get; set; }
    }
}
