using SymbolLabsForge.Contracts;
using SymbolLabsForge.Validation;
using System;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace SymbolLabsForge.Tests.Validation
{
    public class TemplateValidatorTests
    {
        private const string TestAssetsDir = "TestAssets";

        public TemplateValidatorTests()
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

        [Fact]
        public void ValidateMetadata_WithValidMetadata_DoesNotThrow()
        {
            var metadata = new TemplateMetadata
            {
                TemplateName = "valid_name",
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
            TemplateValidator.ValidateMetadata(metadata);
        }

        [Fact]
        public void ValidateMetadata_WithNullName_ThrowsValidationException()
        {
            var metadata = new TemplateMetadata
            {
                TemplateName = null!,
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
            Assert.Throws<ValidationException>(() => TemplateValidator.ValidateMetadata(metadata));
        }

        [Fact]
        public void ValidateMetadata_WithEmptyName_ThrowsValidationException()
        {
            var metadata = new TemplateMetadata
            {
                TemplateName = "",
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
            Assert.Throws<ValidationException>(() => TemplateValidator.ValidateMetadata(metadata));
        }

        [Fact]
        public void ValidatePath_WithValidPath_DoesNotThrow()
        {
            var path = Path.Combine(TestAssetsDir, "dummy_template.txt");
            TemplateValidator.ValidatePath(path);
        }

        [Fact]
        public void ValidatePath_WithNullPath_ThrowsArgumentException()
        {
            string path = null!;
            Assert.Throws<System.ArgumentException>(() => TemplateValidator.ValidatePath(path));
        }

        [Fact]
        public void ValidatePath_WithEmptyPath_ThrowsArgumentException()
        {
            var path = "";
            Assert.Throws<System.ArgumentException>(() => TemplateValidator.ValidatePath(path));
        }

        [Fact]
        public void ValidatePath_WithNonExistentPath_ThrowsFileNotFoundException()
        {
            var path = "non_existent_file.txt";
            Assert.Throws<FileNotFoundException>(() => TemplateValidator.ValidatePath(path));
        }
    }
}
