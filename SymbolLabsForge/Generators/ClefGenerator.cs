using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SymbolLabsForge.Contracts;

namespace SymbolLabsForge.Generators
{
    public class ClefGenerator : ISymbolGenerator
    {
        public SymbolType SupportedType => SymbolType.Natural; // Mapped to Natural for now

        public Image<L8> GenerateRawImage(Size dimensions, int? seed)
        {
            using var image = new Image<Rgba32>(dimensions.Width, dimensions.Height);
            image.Mutate(ctx => ctx.BackgroundColor(Color.White));

            var graphicsOptions = new GraphicsOptions
            {
                Antialias = true // Clefs look better with anti-aliasing
            };

            var pen = Pens.Solid(Color.Black, 2);

            // G-Clef path - this is a simplified approximation
            var pathBuilder = new PathBuilder();
            pathBuilder.MoveTo(new PointF(dimensions.Width * 0.5f, dimensions.Height * 0.1f));
            pathBuilder.QuadraticBezierTo(
                new PointF(dimensions.Width * 0.9f, dimensions.Height * 0.2f),
                new PointF(dimensions.Width * 0.5f, dimensions.Height * 0.35f));
            pathBuilder.CubicBezierTo(
                new PointF(dimensions.Width * 0.2f, dimensions.Height * 0.45f),
                new PointF(dimensions.Width * 0.8f, dimensions.Height * 0.6f),
                new PointF(dimensions.Width * 0.4f, dimensions.Height * 0.8f));
            pathBuilder.LineTo(new PointF(dimensions.Width * 0.5f, dimensions.Height * 0.9f));

            var path = pathBuilder.Build();

            image.Mutate(ctx => ctx.Draw(new DrawingOptions { GraphicsOptions = graphicsOptions }, pen, path));

            return image.CloneAs<L8>();
        }
    }
}
