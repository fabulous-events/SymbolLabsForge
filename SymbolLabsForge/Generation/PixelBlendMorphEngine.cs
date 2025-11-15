using SymbolLabsForge.Contracts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SymbolLabsForge.Configuration;

namespace SymbolLabsForge.Generation
{
    /// <summary>
    /// A simulated morphing engine that performs a pixel-wise alpha blend between two source images.
    /// </summary>
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
            var fromPath = Path.Combine(_snapshotDirectory, request.Type.ToString(), $"{request.FromStyle}.png");
            var toPath = Path.Combine(_snapshotDirectory, request.Type.ToString(), $"{request.ToStyle}.png");

            if (!File.Exists(fromPath) || !File.Exists(toPath))
            {
                throw new FileNotFoundException("Could not find one or both source style images for morphing.");
            }

            using var fromImage = await Image.LoadAsync<L8>(fromPath);
            using var toImage = await Image.LoadAsync<L8>(toPath);

            // For simplicity, we'll use the dimensions of the "from" image.
            // A more robust implementation would resize/align the images.
            var output = new Image<L8>(fromImage.Width, fromImage.Height);

            for (int y = 0; y < fromImage.Height; y++)
            {
                for (int x = 0; x < fromImage.Width; x++)
                {
                    // Simple linear interpolation between the pixel values
                    byte fromPixel = fromImage[x, y].PackedValue;
                    byte toPixel = toImage[x, y].PackedValue;
                    byte resultPixel = (byte)(fromPixel * (1.0f - request.InterpolationFactor) + toPixel * request.InterpolationFactor);
                    output[x, y] = new L8(resultPixel);
                }
            }

            return output;
        }
    }
}
