using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SymbolLabsForge.Contracts;

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
            // Placeholder for Zhang-Suen skeletonization.
            // For now, it will just binarize the image.
            // A full Zhang-Suen implementation will be added here.
            image.Mutate(ctx => ctx.BinaryThreshold(0.5f));
            return image;
        }
    }
}
