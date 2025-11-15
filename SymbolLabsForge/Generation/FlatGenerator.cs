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

            // PHASE I-C: Disable anti-aliasing for geometric symbols
            var drawingOptions = new DrawingOptions
            {
                GraphicsOptions = new GraphicsOptions { Antialias = false }
            };

            rgbaImage.Mutate(ctx =>
            {
                ctx.Fill(Color.White);

                var brush = Brushes.Solid(Color.Black);

                // PHASE II-D: Use centralized geometry constants
                // Draw the vertical stem
                ctx.Fill(drawingOptions, brush, new RectangularPolygon(
                    dimensions.Width * GeometryConstants.Flat.StemLeftX,
                    dimensions.Height * GeometryConstants.Flat.StemTopY,
                    dimensions.Width * GeometryConstants.Flat.StemWidth,
                    dimensions.Height * (GeometryConstants.Flat.StemBottomY - GeometryConstants.Flat.StemTopY)
                ));

                // PHASE I-A + II-D: Draw the bowl using ellipse with centralized constants
                // Create a smooth, backward-curving bowl characteristic of a flat symbol
                var bowlEllipse = new EllipsePolygon(
                    dimensions.Width * GeometryConstants.Flat.BowlCenterX,
                    dimensions.Height * GeometryConstants.Flat.BowlCenterY,
                    dimensions.Width * GeometryConstants.Flat.BowlRadiusX,
                    dimensions.Height * GeometryConstants.Flat.BowlRadiusY
                );
                ctx.Fill(drawingOptions, brush, bowlEllipse);
            });

            // PHASE I-B: Apply explicit binarization after conversion
            var grayscaleImage = rgbaImage.CloneAs<L8>();
            grayscaleImage.Mutate(ctx => ctx.BinaryThreshold(0.5f));
            return grayscaleImage;
        }
    }
}
