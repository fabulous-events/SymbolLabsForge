using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SymbolLabsForge.Contracts;

namespace SymbolLabsForge.Generators
{
    public class StackedGenerator : ISymbolGenerator
    {
        public SymbolType SupportedType => SymbolType.DoubleSharp;

        public Image<L8> GenerateRawImage(Size dimensions, int? seed)
        {
            var image = new Image<L8>(dimensions.Width, dimensions.Height);
            
            // Define the size of the small rectangles
            int rectWidth = dimensions.Width / 4;
            int rectHeight = dimensions.Height / 4;

            // Calculate positions for two vertically stacked rectangles, centered
            var centerX = dimensions.Width / 2;
            var centerY = dimensions.Height / 2;

            var topRect = new Rectangle(centerX - rectWidth / 2, centerY - rectHeight - 2, rectWidth, rectHeight);
            var bottomRect = new Rectangle(centerX - rectWidth / 2, centerY + 2, rectWidth, rectHeight);

            // Draw the rectangles
            image.Mutate(ctx => ctx
                .Fill(Color.White, topRect)
                .Fill(Color.White, bottomRect));

            return image;
        }
    }
}
