using SymbolLabsForge.Contracts;
using Xunit;

namespace SymbolLabsForge.Tests.Contracts
{
    public class TemplateMetadataTests
    {
        [Fact]
        public void WithKeyword_CreatesNewInstanceWithModifiedProperty()
        {
            // Arrange
            var original = new TemplateMetadata { TemplateName = "original", SymbolType = SymbolType.Unknown };

            // Act
            var modified = original with { TemplateName = "modified" };

            // Assert
            Assert.NotSame(original, modified);
            Assert.Equal("original", original.TemplateName);
            Assert.Equal("modified", modified.TemplateName);
        }
    }
}
