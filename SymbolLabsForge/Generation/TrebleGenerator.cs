using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SymbolLabsForge.Contracts;

namespace SymbolLabsForge.Generation
{
    public class TrebleGenerator : ISymbolGenerator
    {
        public SymbolType SupportedType => SymbolType.Treble;

        public Image<L8> GenerateRawImage(Size dimensions, int? seed)
        {
            var rgbaImage = new Image<Rgba32>(dimensions.Width, dimensions.Height);
            rgbaImage.Mutate(ctx =>
            {
                ctx.Fill(Color.White);

                var brush = Brushes.Solid(Color.Black);

                // Draw a simplified, solid treble clef shape
                ctx.FillPolygon(brush, new PointF[] {
                    new PointF(dimensions.Width * 0.5f, dimensions.Height * 0.05f),
                    new PointF(dimensions.Width * 0.7f, dimensions.Height * 0.1f),
                    new PointF(dimensions.Width * 0.5f, dimensions.Height * 0.15f),
                    new PointF(dimensions.Width * 0.3f, dimensions.Height * 0.1f)
                });

                ctx.FillPolygon(brush, new PointF[] {
                    new PointF(dimensions.Width * 0.5f, dimensions.Height * 0.1f),
                    new PointF(dimensions.Width * 0.55f, dimensions.Height * 0.1f),
                    new PointF(dimensions.Width * 0.55f, dimensions.Height * 0.9f),
                    new PointF(dimensions.Width * 0.45f, dimensions.Height * 0.9f)
                });

                ctx.FillPolygon(brush, new PointF[] {
                    new PointF(dimensions.Width * 0.3f, dimensions.Height * 0.7f),
                    new PointF(dimensions.Width * 0.7f, dimensions.Height * 0.6f),
                    new PointF(dimensions.Width * 0.7f, dimensions.Height * 0.7f),
                    new PointF(dimensions.Width * 0.3f, dimensions.Height * 0.8f)
                });
            });

            return rgbaImage.CloneAs<L8>();
        }
    }
}
