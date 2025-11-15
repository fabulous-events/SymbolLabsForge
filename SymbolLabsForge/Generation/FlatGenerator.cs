using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SymbolLabsForge.Contracts;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Drawing;
using SymbolLabsForge.Utils;

namespace SymbolLabsForge.Generators
{
    public class FlatGenerator : ISymbolGenerator
    {
        public SymbolType SupportedType => SymbolType.Flat;

        public Image<L8> GenerateRawImage(Size dimensions, int? seed)
        {
            var rgbaImage = new Image<Rgba32>(dimensions.Width, dimensions.Height);
            rgbaImage.Mutate(ctx =>
            {
                ctx.Fill(Color.White);

                var brush = Brushes.Solid(Color.Black);

                // Draw the vertical stem
                ctx.FillPolygon(brush, new PointF[] {
                    new PointF(dimensions.Width * 0.4f, dimensions.Height * 0.1f),
                    new PointF(dimensions.Width * 0.5f, dimensions.Height * 0.1f),
                    new PointF(dimensions.Width * 0.5f, dimensions.Height * 0.9f),
                    new PointF(dimensions.Width * 0.4f, dimensions.Height * 0.9f)
                });

                // Draw the bulb
                ctx.FillPolygon(brush, new PointF[] {
                    new PointF(dimensions.Width * 0.5f, dimensions.Height * 0.6f),
                    new PointF(dimensions.Width * 0.8f, dimensions.Height * 0.7f),
                    new PointF(dimensions.Width * 0.5f, dimensions.Height * 0.9f)
                });
            });
            return rgbaImage.CloneAs<L8>();
        }
    }
}
