using SymbolLabsForge.Contracts;
using System;
using Xunit;

namespace SymbolLabsForge.Tests.Contracts
{
    public class TemplateMetadataTests
    {
        [Fact]
        public void WithKeyword_CreatesNewInstanceWithModifiedProperty()
        {
            // Arrange
            var original = new TemplateMetadata
            {
                TemplateName = "original",
                SymbolType = SymbolType.Unknown,
                GeneratedBy = "TestRunner",
                TemplateHash = "test-hash-12345",
                Provenance = new ProvenanceMetadata
                {
                    SourceImage = "test-source.png",
                    Method = PreprocessingMethod.Raw,
                    ValidationDate = DateTime.UtcNow,
                    ValidatedBy = "TestRunner"
                }
            };

            // Act
            var modified = original with { TemplateName = "modified" };

            // Assert
            Assert.NotSame(original, modified);
            Assert.Equal("original", original.TemplateName);
            Assert.Equal("modified", modified.TemplateName);
        }
    }
}
