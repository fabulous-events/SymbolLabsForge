using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SymbolLabsForge.Contracts;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Drawing;

namespace SymbolLabsForge.Generators
{
    public class FlatGenerator : ISymbolGenerator
    {
        public SymbolType SupportedType => SymbolType.Flat;

        public Image<L8> GenerateRawImage(Size dimensions, int? seed)
        {
            // Create as Rgba32 first (L8 doesn't support drawing operations properly)
            using var rgbaImage = new Image<Rgba32>(dimensions.Width, dimensions.Height);

            // Set white background
            rgbaImage.Mutate(ctx => ctx.BackgroundColor(Color.White));

            // Disable anti-aliasing for crisp 1-bit rendering
            var drawingOptions = new DrawingOptions
            {
                GraphicsOptions = new GraphicsOptions
                {
                    Antialias = false // Prevents gray pixels that inflate density
                }
            };

            // Draw Flat symbol (vertical stem with rounded bulb at bottom)
            // Added 2px top padding (Y=2 instead of Y=0) to prevent clipping
            rgbaImage.Mutate(ctx =>
            {
                // Vertical stem (left side, 2px wide, with top padding)
                ctx.Fill(drawingOptions, Color.Black, new RectangleF(4, 2, 2, dimensions.Height - 10));

                // Use PathBuilder for smooth bowl curve
                var bowlPath = new PathBuilder()
                    .MoveTo(new PointF(5, dimensions.Height - 10))              // Start at stem connection
                    .CubicBezierTo(                      // Smooth curve for bowl
                        new PointF(8, dimensions.Height - 12),      // Control point 1
                        new PointF(10, dimensions.Height - 10),     // Control point 2
                        new PointF(10, dimensions.Height - 7))      // End of curve
                    .LineTo(new PointF(10, dimensions.Height - 6))              // Bottom of bowl
                    .CubicBezierTo(                      // Return curve
                        new PointF(10, dimensions.Height - 5),
                        new PointF(8, dimensions.Height - 4),
                        new PointF(5, dimensions.Height - 6))       // Back to stem
                    .CloseFigure()
                    .Build();

                ctx.Fill(drawingOptions, Color.Black, bowlPath);
            });

            // Convert to L8 grayscale
            return rgbaImage.CloneAs<L8>();
        }
    }
}
