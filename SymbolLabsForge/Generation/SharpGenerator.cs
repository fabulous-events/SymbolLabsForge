using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SymbolLabsForge.Contracts;

namespace SymbolLabsForge.Generation
{
    public class SharpGenerator : ISymbolGenerator
    {
        public SymbolType SupportedType => SymbolType.Sharp;

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
                    new PointF(dimensions.Width * GeometryConstants.Sharp.LeftStemLeftX, dimensions.Height * GeometryConstants.Sharp.StemTopY),
                    new PointF(dimensions.Width * GeometryConstants.Sharp.LeftStemRightX, dimensions.Height * GeometryConstants.Sharp.StemTopY),
                    new PointF(dimensions.Width * GeometryConstants.Sharp.LeftStemRightX, dimensions.Height * GeometryConstants.Sharp.StemBottomY),
                    new PointF(dimensions.Width * GeometryConstants.Sharp.LeftStemLeftX, dimensions.Height * GeometryConstants.Sharp.StemBottomY)
                });
                ctx.FillPolygon(drawingOptions, brush, new PointF[] {
                    new PointF(dimensions.Width * GeometryConstants.Sharp.RightStemLeftX, dimensions.Height * GeometryConstants.Sharp.StemTopY),
                    new PointF(dimensions.Width * GeometryConstants.Sharp.RightStemRightX, dimensions.Height * GeometryConstants.Sharp.StemTopY),
                    new PointF(dimensions.Width * GeometryConstants.Sharp.RightStemRightX, dimensions.Height * GeometryConstants.Sharp.StemBottomY),
                    new PointF(dimensions.Width * GeometryConstants.Sharp.RightStemLeftX, dimensions.Height * GeometryConstants.Sharp.StemBottomY)
                });

                // Draw the horizontal crossbars as filled rectangles
                ctx.FillPolygon(drawingOptions, brush, new PointF[] {
                    new PointF(dimensions.Width * GeometryConstants.Sharp.CrossbarLeftX, dimensions.Height * GeometryConstants.Sharp.TopCrossbarTopY),
                    new PointF(dimensions.Width * GeometryConstants.Sharp.CrossbarRightX, dimensions.Height * GeometryConstants.Sharp.TopCrossbarTopY),
                    new PointF(dimensions.Width * GeometryConstants.Sharp.CrossbarRightX, dimensions.Height * GeometryConstants.Sharp.TopCrossbarBottomY),
                    new PointF(dimensions.Width * GeometryConstants.Sharp.CrossbarLeftX, dimensions.Height * GeometryConstants.Sharp.TopCrossbarBottomY)
                });
                ctx.FillPolygon(drawingOptions, brush, new PointF[] {
                    new PointF(dimensions.Width * GeometryConstants.Sharp.CrossbarLeftX, dimensions.Height * GeometryConstants.Sharp.BottomCrossbarTopY),
                    new PointF(dimensions.Width * GeometryConstants.Sharp.CrossbarRightX, dimensions.Height * GeometryConstants.Sharp.BottomCrossbarTopY),
                    new PointF(dimensions.Width * GeometryConstants.Sharp.CrossbarRightX, dimensions.Height * GeometryConstants.Sharp.BottomCrossbarBottomY),
                    new PointF(dimensions.Width * GeometryConstants.Sharp.CrossbarLeftX, dimensions.Height * GeometryConstants.Sharp.BottomCrossbarBottomY)
                });
            });

            // PHASE I-B: Apply explicit binarization after conversion
            var grayscaleImage = rgbaImage.CloneAs<L8>();
            grayscaleImage.Mutate(ctx => ctx.BinaryThreshold(0.5f));
            return grayscaleImage;
        }
    }
}
