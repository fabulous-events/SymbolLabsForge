using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;

namespace SymbolLabsForge.Tests.BestPractices
{
    public static class ImageExtensions
    {
        public static byte[] ToByteArray<T>(this Image<T> image) where T : unmanaged, IPixel<T>
        {
            using var memoryStream = new MemoryStream();
            image.SaveAsBmp(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
