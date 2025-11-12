using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SymbolLabsForge.Contracts;

namespace SymbolLabsForge.Generators
{
    // Placeholder for a more complex generator
    public class StackedGenerator : ISymbolGenerator
    {
        public SymbolType SupportedType => SymbolType.Sharp; // Temporarily mapped to Sharp for testing

        public Image<L8> GenerateRawImage(Size dimensions, int? seed)
        {
            return new Image<L8>(dimensions.Width, dimensions.Height);
        }
    }
}
