using Xunit;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using System;

namespace SymbolLabsForge.Tests.BestPractices
{
    public class ImageApiUsageTests
    {
        [Fact]
        [Trait("Category", "BestPractice")]
        public void Drawing_OnL8Image_HasUnexpectedBehavior()
        {
            // Arrange: Create a white grayscale L8 image.
            using var image = new Image<L8>(10, 10);
            image.Mutate(ctx => ctx.Fill(Color.White));

            // Act: Attempt a drawing operation.
            image.Mutate(ctx => ctx.DrawLine(Color.Black, 1, new PointF(0, 0), new PointF(5, 5)));

            // Assert: The pixels on the line are not black (0), demonstrating
            // that drawing on L8 does not behave as expected.
            Assert.NotEqual(0, image[3, 3].PackedValue);
        }

        [Fact]
        [Trait("Category", "BestPractice")]
        public void Drawing_OnRgba32AndCloningToL8_Succeeds()
        {
            // Arrange: Create an Rgba32 image for drawing.
            using var rgbaImage = new Image<Rgba32>(10, 10);

            // Act: Perform the drawing operation on the Rgba32 surface.
            var options = new DrawingOptions { GraphicsOptions = { Antialias = false } };
            rgbaImage.Mutate(ctx =>
            {
                ctx.Fill(Color.White); // White background
                ctx.Draw(options, new SolidPen(Color.Black, 1), new PathBuilder().AddLine(new PointF(0, 0), new PointF(5, 5)).Build());
            });

            // Convert to L8 for processing.
            using var l8Image = rgbaImage.CloneAs<L8>();

            // Assert: The operation succeeded and the resulting L8 image is valid.
            Assert.NotNull(l8Image);
            Assert.Equal(10, l8Image.Width);

            // Check a pixel to confirm the drawing was successful before conversion.
            // A black line was drawn, so the pixel should be black (0).
            Assert.Equal(0, l8Image[3, 3].PackedValue);
            // A pixel not on the line should be white (255).
            Assert.Equal(255, l8Image[8, 8].PackedValue);
        }
    }
}
