using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;

namespace SymbolLabsForge.Preprocessing
{
    public interface IPreprocessingStep
    {
        Image<L8> Process(Image<L8> image);
    }

    public class SkeletonizationProcessor : IPreprocessingStep
    {
        public Image<L8> Process(Image<L8> image)
        {
            var clone = image.Clone(); // Work on a copy
            bool changed;
            do
            {
                changed = false;
                changed |= ThinningIteration(clone, 0);
                changed |= ThinningIteration(clone, 1);
            } while (changed);

            return clone;
        }

        private bool ThinningIteration(Image<L8> image, int iter)
        {
            var pixelsToRemove = new List<Point>();
            bool changed = false;

            for (int y = 1; y < image.Height - 1; y++)
            {
                for (int x = 1; x < image.Width - 1; x++)
                {
                    if (image[x, y].PackedValue == 0) continue; // Skip background pixels

                    int a = GetA(image, x, y);
                    int b = GetB(image, x, y);
                    int p2 = image[x, y - 1].PackedValue;
                    int p3 = image[x + 1, y - 1].PackedValue;
                    int p4 = image[x + 1, y].PackedValue;
                    int p5 = image[x + 1, y + 1].PackedValue;
                    int p6 = image[x, y + 1].PackedValue;
                    int p7 = image[x - 1, y + 1].PackedValue;
                    int p8 = image[x - 1, y].PackedValue;
                    int p9 = image[x - 1, y - 1].PackedValue;

                    if (a == 1 && (b >= 2 && b <= 6))
                    {
                        if (iter == 0)
                        {
                            if (p2 == 0 || p4 == 0 || p6 == 0)
                            {
                                if (p4 == 0 || p6 == 0 || p8 == 0)
                                {
                                    pixelsToRemove.Add(new Point(x, y));
                                    changed = true;
                                }
                            }
                        }
                        else // iter == 1
                        {
                            if (p2 == 0 || p4 == 0 || p8 == 0)
                            {
                                if (p2 == 0 || p6 == 0 || p8 == 0)
                                {
                                    pixelsToRemove.Add(new Point(x, y));
                                    changed = true;
                                }
                            }
                        }
                    }
                }
            }

            foreach (var p in pixelsToRemove)
            {
                image[p.X, p.Y] = new L8(0);
            }

            return changed;
        }

        private int GetA(Image<L8> image, int x, int y)
        {
            int count = 0;
            // p2 -> p3
            if (image[x, y - 1].PackedValue == 0 && image[x + 1, y - 1].PackedValue == 255) count++;
            // p3 -> p4
            if (image[x + 1, y - 1].PackedValue == 0 && image[x + 1, y].PackedValue == 255) count++;
            // p4 -> p5
            if (image[x + 1, y].PackedValue == 0 && image[x + 1, y + 1].PackedValue == 255) count++;
            // p5 -> p6
            if (image[x + 1, y + 1].PackedValue == 0 && image[x, y + 1].PackedValue == 255) count++;
            // p6 -> p7
            if (image[x, y + 1].PackedValue == 0 && image[x - 1, y + 1].PackedValue == 255) count++;
            // p7 -> p8
            if (image[x - 1, y + 1].PackedValue == 0 && image[x - 1, y].PackedValue == 255) count++;
            // p8 -> p9
            if (image[x - 1, y].PackedValue == 0 && image[x - 1, y - 1].PackedValue == 255) count++;
            // p9 -> p2
            if (image[x - 1, y - 1].PackedValue == 0 && image[x, y - 1].PackedValue == 255) count++;
            return count;
        }

        private int GetB(Image<L8> image, int x, int y)
        {
            int count = 0;
            if (image[x, y - 1].PackedValue == 255) count++;
            if (image[x + 1, y - 1].PackedValue == 255) count++;
            if (image[x + 1, y].PackedValue == 255) count++;
            if (image[x + 1, y + 1].PackedValue == 255) count++;
            if (image[x, y + 1].PackedValue == 255) count++;
            if (image[x - 1, y + 1].PackedValue == 255) count++;
            if (image[x - 1, y].PackedValue == 255) count++;
            if (image[x - 1, y - 1].PackedValue == 255) count++;
            return count;
        }
    }
}
