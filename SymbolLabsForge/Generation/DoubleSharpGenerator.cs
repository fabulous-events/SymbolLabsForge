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

            // PHASE I-C: Disable anti-aliasing for geometric symbols
            var drawingOptions = new DrawingOptions
            {
                GraphicsOptions = new GraphicsOptions { Antialias = false }
            };

            rgbaImage.Mutate(ctx =>
            {
                ctx.Fill(Color.White);

                var brush = Brushes.Solid(Color.Black);

                // PHASE II-F: Use centralized geometry constants
                // Draw an 'X' shape with two diagonal strokes
                ctx.FillPolygon(drawingOptions, brush, new PointF[] {
                    new PointF(dimensions.Width * GeometryConstants.DoubleSharp.Stroke1TopLeftX, dimensions.Height * GeometryConstants.DoubleSharp.Stroke1TopLeftY),
                    new PointF(dimensions.Width * GeometryConstants.DoubleSharp.Stroke1TopRightX, dimensions.Height * GeometryConstants.DoubleSharp.Stroke1TopRightY),
                    new PointF(dimensions.Width * GeometryConstants.DoubleSharp.Stroke1BottomRightX, dimensions.Height * GeometryConstants.DoubleSharp.Stroke1BottomRightY),
                    new PointF(dimensions.Width * GeometryConstants.DoubleSharp.Stroke1BottomLeftX, dimensions.Height * GeometryConstants.DoubleSharp.Stroke1BottomLeftY)
                });

                ctx.FillPolygon(drawingOptions, brush, new PointF[] {
                    new PointF(dimensions.Width * GeometryConstants.DoubleSharp.Stroke2BottomLeftX, dimensions.Height * GeometryConstants.DoubleSharp.Stroke2BottomLeftY),
                    new PointF(dimensions.Width * GeometryConstants.DoubleSharp.Stroke2BottomRightX, dimensions.Height * GeometryConstants.DoubleSharp.Stroke2BottomRightY),
                    new PointF(dimensions.Width * GeometryConstants.DoubleSharp.Stroke2TopRightX, dimensions.Height * GeometryConstants.DoubleSharp.Stroke2TopRightY),
                    new PointF(dimensions.Width * GeometryConstants.DoubleSharp.Stroke2TopLeftX, dimensions.Height * GeometryConstants.DoubleSharp.Stroke2TopLeftY)
                });
            });

            // PHASE I-B: Apply explicit binarization after conversion
            var grayscaleImage = rgbaImage.CloneAs<L8>();
            grayscaleImage.Mutate(ctx => ctx.BinaryThreshold(0.5f));
            return grayscaleImage;
        }
    }
}
