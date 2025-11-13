using Microsoft.Extensions.Options;
using SymbolLabsForge.Configuration;
using System;
using System.IO;
using System.Reflection;

namespace SymbolLabsForge.Services
{
    /// <summary>
    /// Resolves paths to application assets based on the execution environment.
    /// </summary>
    public class AssetPathProvider : IAssetPathProvider
    {
        private readonly string _baseAssetPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetPathProvider"/> class.
        /// </summary>
        /// <param name="settings">The asset settings, typically injected via IOptions.</param>
        public AssetPathProvider(IOptions<AssetSettings> settings)
        {
            // Determine the absolute base path for assets.
            // If the configured path is absolute, use it directly.
            // Otherwise, combine it with the application's base directory.
            var configuredPath = settings.Value.BasePath;
            if (Path.IsPathRooted(configuredPath))
            {
                _baseAssetPath = configuredPath;
            }
            else
            {
                var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                _baseAssetPath = Path.Combine(exePath ?? AppContext.BaseDirectory, configuredPath);
            }

            if (!Directory.Exists(_baseAssetPath))
            {
                // Create the directory if it doesn't exist to avoid errors on first use.
                Directory.CreateDirectory(_baseAssetPath);
            }
        }

        /// <summary>
        /// Gets the full, absolute path for a given asset file name.
        /// </summary>
        /// <param name="assetFileName">The name of the asset file (e.g., "template.json").</param>
        /// <returns>The absolute path to the asset file.</returns>
        public string GetPath(string assetFileName)
        {
            if (string.IsNullOrWhiteSpace(assetFileName))
            {
                throw new ArgumentException("Asset file name cannot be null or whitespace.", nameof(assetFileName));
            }

            return Path.Combine(_baseAssetPath, assetFileName);
        }
    }
}
