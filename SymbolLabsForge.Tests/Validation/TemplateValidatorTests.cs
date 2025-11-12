//===============================================================
// File: TemplateValidatorTests.cs
// Author: Gemini
// Date: 2025-11-11
// Purpose: Unit tests for the TemplateValidator.
//===============================================================
#nullable enable

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SymbolLabsForge.Contracts;
using SymbolLabsForge.Validation;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace SymbolLabsForge.Tests.Validation
{
    [TestClass]
    public class TemplateValidatorTests
    {
        private const string TestAssetsDir = "TestAssets";

        [TestInitialize]
        public void TestInitialize()
        {
            // Ensure test assets directory exists and create a dummy template for the valid path test.
            if (!Directory.Exists(TestAssetsDir))
            {
                Directory.CreateDirectory(TestAssetsDir);
            }

            var dummyPath = Path.Combine(TestAssetsDir, "dummy_template.txt");
            if (!File.Exists(dummyPath))
            {
                File.WriteAllText(dummyPath, "dummy content");
            }
        }

        [TestMethod]
        public void ValidateMetadata_WithValidMetadata_DoesNotThrow()
        {
            var metadata = new TemplateMetadata { TemplateName = "valid_name" };
            TemplateValidator.ValidateMetadata(metadata);
        }

        [TestMethod]
        public void ValidateMetadata_WithNullName_ThrowsValidationException()
        {
            var metadata = new TemplateMetadata { TemplateName = null! };

            try
            {
                TemplateValidator.ValidateMetadata(metadata);
                Assert.Fail("Expected ValidationException was not thrown.");
            }
            catch (ValidationException)
            {
                // Expected
            }
        }

        [TestMethod]
        public void ValidateMetadata_WithEmptyName_ThrowsValidationException()
        {
            var metadata = new TemplateMetadata { TemplateName = "" };

            try
            {
                TemplateValidator.ValidateMetadata(metadata);
                Assert.Fail("Expected ValidationException was not thrown.");
            }
            catch (ValidationException)
            {
                // Expected
            }
        }

        [TestMethod]
        public void ValidatePath_WithValidPath_DoesNotThrow()
        {
            var path = Path.Combine(TestAssetsDir, "dummy_template.txt");
            TemplateValidator.ValidatePath(path);
        }

        [TestMethod]
        public void ValidatePath_WithNullPath_ThrowsArgumentException()
        {
            string path = null!;

            try
            {
                TemplateValidator.ValidatePath(path);
                Assert.Fail("Expected ArgumentException was not thrown.");
            }
            catch (System.ArgumentException)
            {
                // Expected
            }
        }

        [TestMethod]
        public void ValidatePath_WithEmptyPath_ThrowsArgumentException()
        {
            var path = "";

            try
            {
                TemplateValidator.ValidatePath(path);
                Assert.Fail("Expected ArgumentException was not thrown.");
            }
            catch (System.ArgumentException)
            {
                // Expected
            }
        }

        [TestMethod]
        public void ValidatePath_WithNonExistentPath_ThrowsFileNotFoundException()
        {
            var path = "non_existent_file.txt";

            try
            {
                TemplateValidator.ValidatePath(path);
                Assert.Fail("Expected FileNotFoundException was not thrown.");
            }
            catch (FileNotFoundException)
            {
                // Expected
            }
        }
    }
}
