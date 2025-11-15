using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SymbolLabsForge.Contracts;

namespace SymbolLabsForge.Generation
{
    public class NaturalGenerator : ISymbolGenerator
    {
        public SymbolType SupportedType => SymbolType.Natural;

        public Image<L8> GenerateRawImage(Size dimensions, int? seed)
        {
            var rgbaImage = new Image<Rgba32>(dimensions.Width, dimensions.Height);
            rgbaImage.Mutate(ctx =>
            {
                ctx.Fill(Color.White);

                var brush = Brushes.Solid(Color.Black);

                // Draw the vertical lines as filled rectangles
                ctx.FillPolygon(brush, new PointF[] {
                    new PointF(dimensions.Width * 0.3f, dimensions.Height * 0.1f),
                    new PointF(dimensions.Width * 0.4f, dimensions.Height * 0.1f),
                    new PointF(dimensions.Width * 0.4f, dimensions.Height * 0.9f),
                    new PointF(dimensions.Width * 0.3f, dimensions.Height * 0.9f)
                });
                ctx.FillPolygon(brush, new PointF[] {
                    new PointF(dimensions.Width * 0.6f, dimensions.Height * 0.1f),
                    new PointF(dimensions.Width * 0.7f, dimensions.Height * 0.1f),
                    new PointF(dimensions.Width * 0.7f, dimensions.Height * 0.9f),
                    new PointF(dimensions.Width * 0.6f, dimensions.Height * 0.9f)
                });

                // Draw the horizontal lines as filled rectangles
                ctx.FillPolygon(brush, new PointF[] {
                    new PointF(dimensions.Width * 0.2f, dimensions.Height * 0.4f),
                    new PointF(dimensions.Width * 0.8f, dimensions.Height * 0.4f),
                    new PointF(dimensions.Width * 0.8f, dimensions.Height * 0.5f),
                    new PointF(dimensions.Width * 0.2f, dimensions.Height * 0.5f)
                });
                ctx.FillPolygon(brush, new PointF[] {
                    new PointF(dimensions.Width * 0.2f, dimensions.Height * 0.7f),
                    new PointF(dimensions.Width * 0.8f, dimensions.Height * 0.7f),
                    new PointF(dimensions.Width * 0.8f, dimensions.Height * 0.8f),
                    new PointF(dimensions.Width * 0.2f, dimensions.Height * 0.8f)
                });
            });
            return rgbaImage.CloneAs<L8>();
        }
    }
}
