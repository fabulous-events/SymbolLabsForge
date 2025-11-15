using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SymbolLabsForge.Contracts;

namespace SymbolLabsForge.Generation
{
    public class DoubleSharpGenerator : ISymbolGenerator
    {
        public SymbolType SupportedType => SymbolType.DoubleSharp;

        public Image<L8> GenerateRawImage(Size dimensions, int? seed)
        {
            var rgbaImage = new Image<Rgba32>(dimensions.Width, dimensions.Height);
            rgbaImage.Mutate(ctx =>
            {
                ctx.Fill(Color.White);

                var brush = Brushes.Solid(Color.Black);

                // Draw a simple 'x' shape
                ctx.FillPolygon(brush, new PointF[] {
                    new PointF(dimensions.Width * 0.2f, dimensions.Height * 0.2f),
                    new PointF(dimensions.Width * 0.3f, dimensions.Height * 0.2f),
                    new PointF(dimensions.Width * 0.8f, dimensions.Height * 0.7f),
                    new PointF(dimensions.Width * 0.7f, dimensions.Height * 0.8f)
                });

                ctx.FillPolygon(brush, new PointF[] {
                    new PointF(dimensions.Width * 0.2f, dimensions.Height * 0.7f),
                    new PointF(dimensions.Width * 0.3f, dimensions.Height * 0.8f),
                    new PointF(dimensions.Width * 0.8f, dimensions.Height * 0.3f),
                    new PointF(dimensions.Width * 0.7f, dimensions.Height * 0.2f)
                });
            });
            return rgbaImage.CloneAs<L8>();
        }
    }
}
