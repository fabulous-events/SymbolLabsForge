//===============================================================
// File: SyntheticSymbolGenerator.cs
// Author: Gemini
// Date: 2025-11-12
// Purpose: Procedurally generates synthetic music symbols.
//===============================================================
#nullable enable

using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SymbolLabsForge.Contracts;
using SymbolLabsForge.ImageProcessing.Utilities;

namespace SymbolLabsForge.Generators
{
    public class SyntheticSymbolGenerator
    {
        public Image<L8> Generate(SymbolParameters parameters, Size dimensions)
        {
            Image<L8> skeletonizedImage;

            // Create an Rgba32 image for drawing, as L8 surfaces do not support these operations directly.
            using (var imageRgba32 = new Image<Rgba32>(dimensions.Width, dimensions.Height))
            {
                // White background, black drawing for mask generation
                imageRgba32.Mutate(ctx => ctx.Fill(Color.White));

                switch (parameters.SymbolType)
                {
                    case MusicSymbolType.WholeNote:
                        DrawWholeNote(imageRgba32, parameters);
                        break;
                    case MusicSymbolType.QuarterNote:
                        DrawQuarterNote(imageRgba32, parameters);
                        break;
                    // Other cases will be added here
                    default:
                        throw new NotImplementedException($"Generator for {parameters.SymbolType} is not implemented.");
                }

                // Apply transformations
                imageRgba32.Mutate(ctx => ctx.Rotate(parameters.Rotation));

                // Convert the Rgba32 drawing to an L8 mask for processing
                using (var l8Image = imageRgba32.CloneAs<L8>())
                {
                    // Skeletonize the L8 image
                    var skeletonizer = new SkeletonizationProcessor();
                    skeletonizedImage = skeletonizer.Process(l8Image);
                }
            }

            return skeletonizedImage;
        }

        private void DrawWholeNote(Image<Rgba32> image, SymbolParameters parameters)
        {
            var center = new PointF(image.Width / 2, image.Height / 2);
            var noteHeadWidth = image.Width / 3f;
            var noteHeadHeight = image.Height / 5f;

            // Draw the outline of the note head
            image.Mutate(ctx => ctx.Draw(Pens.Solid(Color.Black, parameters.StrokeThickness), new EllipsePolygon(center.X, center.Y, noteHeadWidth, noteHeadHeight)));
            
            // Tilt the note head slightly
            image.Mutate(ctx => ctx.Rotate(-20));
        }

        private void DrawQuarterNote(Image<Rgba32> image, SymbolParameters parameters)
        {
            var center = new PointF(image.Width / 2, image.Height / 2);
            var noteHeadWidth = image.Width / 3.5f;
            var noteHeadHeight = image.Height / 5f;

            // Draw the filled note head
            var noteHead = new EllipsePolygon(center.X, center.Y, noteHeadWidth, noteHeadHeight);
            image.Mutate(ctx => ctx.Fill(Color.Black, noteHead));
            
            // Tilt the note head slightly
            image.Mutate(ctx => ctx.Rotate(-20));

            // Draw the stem
            var stemHeight = image.Height * 0.6f;
            var stemStart = new PointF(center.X + noteHeadWidth / 2, center.Y);
            var stemEnd = new PointF(stemStart.X, stemStart.Y - stemHeight);
            image.Mutate(ctx => ctx.DrawLine(Pens.Solid(Color.Black, parameters.StrokeThickness), stemStart, stemEnd));
        }
    }
}
