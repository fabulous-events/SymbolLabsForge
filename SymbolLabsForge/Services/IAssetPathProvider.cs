namespace SymbolLabsForge.Services
{
    /// <summary>
    /// Provides an interface for resolving paths to application assets.
    /// </summary>
    public interface IAssetPathProvider
    {
        /// <summary>
        /// Gets the full, absolute path for a given asset file name.
        /// </summary>
        /// <param name="assetFileName">The name of the asset file (e.g., "template.json").</param>
        /// <returns>The absolute path to the asset file.</returns>
        string GetPath(string assetFileName);
    }
}
