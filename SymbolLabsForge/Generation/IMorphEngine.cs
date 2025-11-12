using SymbolLabsForge.Contracts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Threading.Tasks;

namespace SymbolLabsForge.Generation
{
    public interface IMorphEngine
    {
        Task<Image<L8>> MorphAsync(MorphRequest request);
    }
}
