using Xunit;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;

namespace SymbolLabsForge.Tests.BestPractices
{
    public class ResourceManagementTests
    {
        /// <summary>
        /// A test helper that correctly creates and returns an image,
        /// transferring ownership to the caller.
        /// </summary>
        private Image<Rgba32> CreateAndReturnImage()
        {
            // This method allocates and returns. The caller MUST dispose.
            return new Image<Rgba32>(10, 10);
        }

        /// <summary>
        /// A test helper that incorrectly disposes an image before returning it,
        /// violating the ownership transfer contract.
        /// </summary>
        private Image<Rgba32> CreateAndDisposeImage()
        {
            var image = new Image<Rgba32>(10, 10);
            image.Dispose(); // WRONG: Disposing before returning
            return image;
        }

        [Fact]
        [Trait("Category", "BestPractice")]
        public void DisposalContract_CallerOwnsAndDisposesImage_Succeeds()
        {
            // Arrange: Get an image from a method that returns it.
            var image = CreateAndReturnImage();

            // Act & Assert: The caller should be able to use and dispose of the image.
            // If the creator had disposed it, this would throw an ObjectDisposedException.
            using (image)
            {
                Assert.NotNull(image);
                image.Mutate(ctx => ctx.Brightness(0.5f)); // Perform an operation
            }
        }

        [Fact]
        [Trait("Category", "BestPractice")]
        public void DisposalContract_CreatorDisposesImage_CallerReceivesDisposedObject()
        {
            // Arrange: Get an image from a method that incorrectly disposes it.
            var image = CreateAndDisposeImage();

            // Act & Assert: Assert that any attempt to use the image throws an ObjectDisposedException.
            Assert.Throws<ObjectDisposedException>(() =>
            {
                image.Mutate(ctx => ctx.Brightness(0.5f));
            });

            // It's good practice to still dispose, even if it's already disposed.
            image.Dispose();
        }
    }
}
