using SymbolLabsForge.Contracts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SymbolLabsForge.Configuration;
using SymbolLabsForge.ImageProcessing.Utilities;

namespace SymbolLabsForge.Generation
{
    /// <summary>
    /// A simulated morphing engine that performs pixel-wise linear blending between two source images.
    /// </summary>
    /// <remarks>
    /// <para><b>Phase 9.2 Refactor: Service vs. Utility Separation</b></para>
    /// <para>This class now delegates blending logic to <see cref="PixelBlender.LinearBlend"/>.
    /// PixelBlendMorphEngine retains service layer concerns: DI, file I/O, path construction.
    /// PixelBlender contains pure blending algorithms: no I/O, no dependencies, fully testable.</para>
    ///
    /// <para><b>What This Class Does:</b></para>
    /// <list type="bullet">
    /// <item>Loads source images from disk (from AssetSettings.RootDirectory/Snapshots)</item>
    /// <item>Validates file existence</item>
    /// <item>Delegates blending to PixelBlender.LinearBlend()</item>
    /// <item>Returns morphed image to caller</item>
    /// </list>
    ///
    /// <para><b>Teaching Value:</b></para>
    /// <para>Demonstrates separation of concerns: I/O layer (this class) vs. algorithm layer (PixelBlender).
    /// Students learn how to design testable, reusable utilities by extracting pure functions.</para>
    /// </remarks>
    public class PixelBlendMorphEngine : IMorphEngine
    {
        private readonly string _snapshotDirectory;

        public PixelBlendMorphEngine(IOptions<AssetSettings> assetSettings)
        {
            // The "snapshots" are considered assets for the purpose of morphing
            _snapshotDirectory = Path.Combine(assetSettings.Value.RootDirectory, "Snapshots");
        }

        public async Task<Image<L8>> MorphAsync(MorphRequest request)
        {
            // Service layer: File I/O and path construction
            var fromPath = Path.Combine(_snapshotDirectory, request.Type.ToString(), $"{request.FromStyle}.png");
            var toPath = Path.Combine(_snapshotDirectory, request.Type.ToString(), $"{request.ToStyle}.png");

            if (!File.Exists(fromPath) || !File.Exists(toPath))
            {
                throw new FileNotFoundException("Could not find one or both source style images for morphing.");
            }

            using var fromImage = await Image.LoadAsync<L8>(fromPath);
            using var toImage = await Image.LoadAsync<L8>(toPath);

            // Utility layer: Pure blending algorithm (delegated to PixelBlender)
            // Phase 9.2 Refactor: Blending logic extracted to PixelBlender.LinearBlend()
            // This enables unit testing of blending algorithm without file I/O dependencies
            var output = PixelBlender.LinearBlend(fromImage, toImage, request.InterpolationFactor);

            return output;
        }
    }
}
