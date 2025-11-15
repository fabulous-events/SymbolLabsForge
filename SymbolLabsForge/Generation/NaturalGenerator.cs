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
                // Draw the vertical lines as filled rectangles
                ctx.FillPolygon(drawingOptions, brush, new PointF[] {
                    new PointF(dimensions.Width * GeometryConstants.Natural.LeftStemLeftX, dimensions.Height * GeometryConstants.Natural.StemTopY),
                    new PointF(dimensions.Width * GeometryConstants.Natural.LeftStemRightX, dimensions.Height * GeometryConstants.Natural.StemTopY),
                    new PointF(dimensions.Width * GeometryConstants.Natural.LeftStemRightX, dimensions.Height * GeometryConstants.Natural.StemBottomY),
                    new PointF(dimensions.Width * GeometryConstants.Natural.LeftStemLeftX, dimensions.Height * GeometryConstants.Natural.StemBottomY)
                });
                ctx.FillPolygon(drawingOptions, brush, new PointF[] {
                    new PointF(dimensions.Width * GeometryConstants.Natural.RightStemLeftX, dimensions.Height * GeometryConstants.Natural.StemTopY),
                    new PointF(dimensions.Width * GeometryConstants.Natural.RightStemRightX, dimensions.Height * GeometryConstants.Natural.StemTopY),
                    new PointF(dimensions.Width * GeometryConstants.Natural.RightStemRightX, dimensions.Height * GeometryConstants.Natural.StemBottomY),
                    new PointF(dimensions.Width * GeometryConstants.Natural.RightStemLeftX, dimensions.Height * GeometryConstants.Natural.StemBottomY)
                });

                // Draw the horizontal crossbars as filled rectangles
                ctx.FillPolygon(drawingOptions, brush, new PointF[] {
                    new PointF(dimensions.Width * GeometryConstants.Natural.CrossbarLeftX, dimensions.Height * GeometryConstants.Natural.TopCrossbarTopY),
                    new PointF(dimensions.Width * GeometryConstants.Natural.CrossbarRightX, dimensions.Height * GeometryConstants.Natural.TopCrossbarTopY),
                    new PointF(dimensions.Width * GeometryConstants.Natural.CrossbarRightX, dimensions.Height * GeometryConstants.Natural.TopCrossbarBottomY),
                    new PointF(dimensions.Width * GeometryConstants.Natural.CrossbarLeftX, dimensions.Height * GeometryConstants.Natural.TopCrossbarBottomY)
                });
                ctx.FillPolygon(drawingOptions, brush, new PointF[] {
                    new PointF(dimensions.Width * GeometryConstants.Natural.CrossbarLeftX, dimensions.Height * GeometryConstants.Natural.BottomCrossbarTopY),
                    new PointF(dimensions.Width * GeometryConstants.Natural.CrossbarRightX, dimensions.Height * GeometryConstants.Natural.BottomCrossbarTopY),
                    new PointF(dimensions.Width * GeometryConstants.Natural.CrossbarRightX, dimensions.Height * GeometryConstants.Natural.BottomCrossbarBottomY),
                    new PointF(dimensions.Width * GeometryConstants.Natural.CrossbarLeftX, dimensions.Height * GeometryConstants.Natural.BottomCrossbarBottomY)
                });
            });

            // PHASE I-B: Apply explicit binarization after conversion
            var grayscaleImage = rgbaImage.CloneAs<L8>();
            grayscaleImage.Mutate(ctx => ctx.BinaryThreshold(0.5f));
            return grayscaleImage;
        }
    }
}
