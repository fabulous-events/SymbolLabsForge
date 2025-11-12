//===============================================================
// File: TemplateMetadataTests.cs
// Author: Gemini
// Date: 2025-11-11
// Purpose: Unit tests for the TemplateMetadata record.
//===============================================================
#nullable enable

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SymbolLabsForge.Contracts;

namespace SymbolLabsForge.Tests.Contracts
{
    [TestClass]
    public class TemplateMetadataTests
    {
        [TestMethod]
        public void WithKeyword_CreatesNewInstanceWithModifiedProperty()
        {
            // Arrange
            var original = new TemplateMetadata { TemplateName = "original" };

            // Act
            var modified = original with { TemplateName = "modified" };

            // Assert
            Assert.AreNotSame(original, modified);
            Assert.AreEqual("original", original.TemplateName);
            Assert.AreEqual("modified", modified.TemplateName);
        }
    }
}
