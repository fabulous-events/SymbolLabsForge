using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SymbolLabsForge.Contracts;
using SymbolLabsForge.Utils;

namespace SymbolLabsForge.Generators
{
    public class StackedGenerator : ISymbolGenerator
    {
        public SymbolType SupportedType => SymbolType.DoubleSharp;

        public Image<L8> GenerateRawImage(Size dimensions, int? seed)
        {
            return GeneratorUtils.CreateImageFromDrawing(dimensions.Width, dimensions.Height, ctx =>
            {
                ctx.Fill(Color.White);

                // PHASE II-E: Use centralized geometry constants
                // Define the size of the small rectangles (proportional to image dimensions)
                int rectWidth = (int)(dimensions.Width * GeometryConstants.Common.StackedComponentSizeRatio);
                int rectHeight = (int)(dimensions.Height * GeometryConstants.Common.StackedComponentSizeRatio);

                // Calculate positions for two vertically stacked rectangles, centered
                var centerX = dimensions.Width / 2;
                var centerY = dimensions.Height / 2;

                // PHASE II-E FIX: Use proportional gap instead of fixed 4-pixel offset
                // Gap scales with image height (e.g., 2% of 100px = 2px, 2% of 256px = 5.12px)
                int halfGap = (int)(dimensions.Height * GeometryConstants.Common.StackedComponentGapRatio / 2);

                var topRect = new Rectangle(centerX - rectWidth / 2, centerY - rectHeight - halfGap, rectWidth, rectHeight);
                var bottomRect = new Rectangle(centerX - rectWidth / 2, centerY + halfGap, rectWidth, rectHeight);

                // Draw the rectangles
                ctx.Fill(Color.Black, topRect);
                ctx.Fill(Color.Black, bottomRect);
            });
        }
    }
}
