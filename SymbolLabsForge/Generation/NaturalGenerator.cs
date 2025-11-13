using SixLabors.ImageSharp;
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

                var pen = Pens.Solid(Color.Black, 1);

                // Draw the vertical lines
                ctx.DrawLine(pen, new PointF(dimensions.Width * 0.35f, dimensions.Height * 0.2f), new PointF(dimensions.Width * 0.35f, dimensions.Height * 0.8f));
                ctx.DrawLine(pen, new PointF(dimensions.Width * 0.65f, dimensions.Height * 0.2f), new PointF(dimensions.Width * 0.65f, dimensions.Height * 0.8f));

                // Draw the horizontal lines
                ctx.DrawLine(pen, new PointF(dimensions.Width * 0.2f, dimensions.Height * 0.45f), new PointF(dimensions.Width * 0.8f, dimensions.Height * 0.45f));
                ctx.DrawLine(pen, new PointF(dimensions.Width * 0.2f, dimensions.Height * 0.55f), new PointF(dimensions.Width * 0.8f, dimensions.Height * 0.55f));
            });
            return rgbaImage.CloneAs<L8>();
        }
    }
}
