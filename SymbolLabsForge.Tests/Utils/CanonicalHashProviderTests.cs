// Located in: SymbolLabsForge.Tests/Utils/CanonicalHashProviderTests.cs
#nullable enable

using SymbolLabsForge.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace SymbolLabsForge.Tests.Utils
{
    [Collection("NonParallel")]
    public class CanonicalHashProviderTests
    {
        [Fact]
        public void ComputeSha256_IsDeterministicAndCorrect()
        {
            // Arrange
            // Create a simple, 100% predictable image.
            using var image = new Image<L8>(16, 16, new L8(42));
            
            // This is the known-good, stable hash for the 16x16 solid image,
            // produced by the thread-safe "Copy-Local" hashing pattern.
            const string expectedHash = "29d72884c6c8156eb0fb6bc6348c0a03896e7daf24cadd7a1b302093dafdc06b";

            // Act
            var actualHash = CanonicalHashProvider.ComputeSha256(image);

            // Assert
            Assert.Equal(expectedHash, actualHash);
        }
    }
}