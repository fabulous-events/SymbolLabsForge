using SymbolLabsForge.Contracts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using System.Threading.Tasks;

namespace SymbolLabsForge.Utils
{
    public static class SnapshotComparer
    {
        public static bool AreSimilar(Image<L8> expected, Image<L8> actual, double tolerance = 0.01)
        {
            if (expected.Size != actual.Size) return false;
            // ... (rest of the logic)
            return true;
        }
    }

    public static class ImageDiffGenerator
    {
        public static void SaveDiff(Image<L8> expected, Image<L8> actual, string outputPath)
        {
            // ... (logic)
        }
    }
}
