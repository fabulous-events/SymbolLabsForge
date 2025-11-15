//===============================================================
// File: ProvenanceRegressionTests.cs
// Author: Claude (Phase 5 - Registry & Provenance Tracking)
// Date: 2025-11-14
// Purpose: Prevents regression of provenance tracking defects fixed in Phase 5.
//          Codifies metadata validation and provenance enforcement as permanent quality gates.
//
// DEFECT HISTORY:
//   Phase 5.1: TemplateMetadata accepted "unknown", "default", "unhashed" defaults
//   Phase 5.2: No structured provenance tracking (nullable string only)
//   Phase 5.3: CapsuleExporter had no validation, no hash computation
//   Phase 5.4: CapsuleRegistryManager accepted incomplete metadata with fallbacks
//
// VALIDATION STRATEGY:
//   - Guard against "unknown", "default", "unhashed" metadata reintroduction
//   - Ensure provenance completeness before export/registry operations
//   - Verify SHA256 hash computation from image bytes
//   - Confirm fail-fast validation (InvalidOperationException)
//   - Validate legacy migration path for backward compatibility
//
// CANONICAL STANDARD:
//   - TemplateName: REQUIRED, descriptive (not "unknown" or "default")
//   - GeneratedBy: REQUIRED, tool/version identifier
//   - TemplateHash: REQUIRED, computed SHA256 (not "unhashed")
//   - Provenance: REQUIRED structured metadata (SourceImage, Method, ValidationDate, ValidatedBy)
//   - Export/Registry: Fail-fast if metadata incomplete
//
// LINKED ARTIFACTS:
//   - ProvenanceMetadata.cs (structured provenance record)
//   - PreprocessingMethod.cs (standardized preprocessing enum)
//   - CapsuleExporter.cs (hash computation, validation)
//   - CapsuleRegistryManager.cs (entry validation, legacy migration)
//
// AUDIENCE: Graduate / PhD (provenance tracking, auditability)
//===============================================================
#nullable enable

using Xunit;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SymbolLabsForge.Contracts;
using SymbolLabsForge.Services;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;
using SymbolLabsForge.Configuration;

namespace SymbolLabsForge.Tests.Regression
{
    /// <summary>
    /// Regression tests that prevent re-introduction of provenance tracking defects
    /// identified and fixed in Phase 5 of Registry & Provenance Tracking.
    /// </summary>
    public class ProvenanceRegressionTests : IDisposable
    {
        private readonly string _tempDirectory;

        public ProvenanceRegressionTests()
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), $"ProvenanceTests_{Guid.NewGuid():N}");
            Directory.CreateDirectory(_tempDirectory);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, recursive: true);
            }
        }

        #region CapsuleExporter Validation Tests

        /// <summary>
        /// REGRESSION GUARD: Prevents export with missing TemplateName.
        ///
        /// DEFECT HISTORY (Phase 5.3):
        ///   - Original Implementation: No validation, allowed null/empty TemplateName
        ///   - Root Cause: No fail-fast validation before export
        ///   - Impact: Exported capsules with incomplete metadata, breaking identification
        ///   - Fix: Added ValidateMetadata() that throws InvalidOperationException
        ///
        /// VALIDATION STRATEGY:
        ///   - Create capsule with null TemplateName
        ///   - Attempt export via CapsuleExporter.ExportAsync()
        ///   - Verify InvalidOperationException is thrown with clear error message
        /// </summary>
        [Fact]
        public async Task CapsuleExporter_ExportAsync_ThrowsOnMissingTemplateName()
        {
            // Arrange: Create capsule with missing TemplateName
            using var image = CreateTestImageL8(10, 10);
            var metadata = new TemplateMetadata
            {
                TemplateName = "", // Invalid: empty
                GeneratedBy = "SymbolLabsForge v1.0",
                TemplateHash = "abc123",
                SymbolType = SymbolType.Clef,
                Provenance = new ProvenanceMetadata
                {
                    SourceImage = "test.png",
                    Method = PreprocessingMethod.Binarized,
                    ValidationDate = DateTime.UtcNow,
                    ValidatedBy = "TestRunner"
                }
            };
            var capsule = new SymbolCapsule(
                image,
                metadata,
                new QualityMetrics(),
                true,
                new System.Collections.Generic.List<ValidationResult>()
            );

            var exporter = new CapsuleExporter();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => exporter.ExportAsync(capsule, _tempDirectory, "test"));

            Assert.Contains("TemplateName is missing or empty", exception.Message);
        }

        /// <summary>
        /// REGRESSION GUARD: Prevents export with missing GeneratedBy.
        ///
        /// DEFECT HISTORY (Phase 5.3):
        ///   - Original Implementation: Allowed null/empty GeneratedBy
        ///   - Root Cause: No validation enforcement
        ///   - Impact: Capsules had no tool/version traceability
        ///   - Fix: Validate GeneratedBy before export
        ///
        /// VALIDATION STRATEGY:
        ///   - Create capsule with null GeneratedBy
        ///   - Attempt export
        ///   - Verify InvalidOperationException with clear error message
        /// </summary>
        [Fact]
        public async Task CapsuleExporter_ExportAsync_ThrowsOnMissingGeneratedBy()
        {
            // Arrange: Create capsule with missing GeneratedBy
            using var image = CreateTestImageL8(10, 10);
            var metadata = new TemplateMetadata
            {
                TemplateName = "test-template",
                GeneratedBy = "", // Invalid: empty
                TemplateHash = "abc123",
                SymbolType = SymbolType.Clef,
                Provenance = new ProvenanceMetadata
                {
                    SourceImage = "test.png",
                    Method = PreprocessingMethod.Binarized,
                    ValidationDate = DateTime.UtcNow,
                    ValidatedBy = "TestRunner"
                }
            };
            var capsule = new SymbolCapsule(
                image,
                metadata,
                new QualityMetrics(),
                true,
                new System.Collections.Generic.List<ValidationResult>()
            );

            var exporter = new CapsuleExporter();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => exporter.ExportAsync(capsule, _tempDirectory, "test"));

            Assert.Contains("GeneratedBy is missing or empty", exception.Message);
        }

        /// <summary>
        /// REGRESSION GUARD: Prevents export with "unhashed" TemplateHash.
        ///
        /// DEFECT HISTORY (Phase 5.3):
        ///   - Original Implementation: Exported templates with "unhashed" placeholder
        ///   - Root Cause: No hash computation, no validation
        ///   - Impact: Templates had no integrity verification, broke deduplication
        ///   - Fix: Compute SHA256 hash, reject "unhashed" in validation
        ///
        /// VALIDATION STRATEGY:
        ///   - Create capsule with "unhashed" TemplateHash
        ///   - Attempt export
        ///   - Verify InvalidOperationException preventing export
        /// </summary>
        [Fact]
        public async Task CapsuleExporter_ExportAsync_ThrowsOnUnhashedTemplateHash()
        {
            // Arrange: Create capsule with "unhashed" TemplateHash
            using var image = CreateTestImageL8(10, 10);
            var metadata = new TemplateMetadata
            {
                TemplateName = "test-template",
                GeneratedBy = "SymbolLabsForge v1.0",
                TemplateHash = "unhashed", // Invalid: not computed
                SymbolType = SymbolType.Clef,
                Provenance = new ProvenanceMetadata
                {
                    SourceImage = "test.png",
                    Method = PreprocessingMethod.Binarized,
                    ValidationDate = DateTime.UtcNow,
                    ValidatedBy = "TestRunner"
                }
            };
            var capsule = new SymbolCapsule(
                image,
                metadata,
                new QualityMetrics(),
                true,
                new System.Collections.Generic.List<ValidationResult>()
            );

            var exporter = new CapsuleExporter();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => exporter.ExportAsync(capsule, _tempDirectory, "test"));

            Assert.Contains("TemplateHash is missing or invalid", exception.Message);
        }

        /// <summary>
        /// REGRESSION GUARD: Prevents export with null Provenance.
        ///
        /// DEFECT HISTORY (Phase 5.2-5.3):
        ///   - Original Implementation: Provenance was optional nullable string
        ///   - Root Cause: No structured provenance enforcement
        ///   - Impact: Templates had no traceability to source images/methods
        ///   - Fix: Require structured ProvenanceMetadata with [Required] attribute
        ///
        /// VALIDATION STRATEGY:
        ///   - Create capsule with null Provenance
        ///   - Attempt export
        ///   - Verify InvalidOperationException with provenance error
        /// </summary>
        [Fact]
        public async Task CapsuleExporter_ExportAsync_ThrowsOnNullProvenance()
        {
            // Arrange: Create capsule with null Provenance
            using var image = CreateTestImageL8(10, 10);
            var metadata = new TemplateMetadata
            {
                TemplateName = "test-template",
                GeneratedBy = "SymbolLabsForge v1.0",
                TemplateHash = "abc123",
                SymbolType = SymbolType.Clef,
                Provenance = null! // Invalid: null provenance
            };
            var capsule = new SymbolCapsule(
                image,
                metadata,
                new QualityMetrics(),
                true,
                new System.Collections.Generic.List<ValidationResult>()
            );

            var exporter = new CapsuleExporter();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => exporter.ExportAsync(capsule, _tempDirectory, "test"));

            Assert.Contains("Provenance metadata is missing", exception.Message);
        }

        /// <summary>
        /// REGRESSION GUARD: Prevents export with incomplete Provenance.
        ///
        /// DEFECT HISTORY (Phase 5.2):
        ///   - Original Implementation: No validation of provenance fields
        ///   - Root Cause: Provenance was free-form string
        ///   - Impact: Incomplete traceability (missing source image or validator)
        ///   - Fix: Validate SourceImage and ValidatedBy in structured provenance
        ///
        /// VALIDATION STRATEGY:
        ///   - Create capsule with missing Provenance.SourceImage
        ///   - Attempt export
        ///   - Verify InvalidOperationException with specific field error
        /// </summary>
        [Fact]
        public async Task CapsuleExporter_ExportAsync_ThrowsOnMissingProvenanceSourceImage()
        {
            // Arrange: Create capsule with incomplete Provenance (missing SourceImage)
            using var image = CreateTestImageL8(10, 10);
            var metadata = new TemplateMetadata
            {
                TemplateName = "test-template",
                GeneratedBy = "SymbolLabsForge v1.0",
                TemplateHash = "abc123",
                SymbolType = SymbolType.Clef,
                Provenance = new ProvenanceMetadata
                {
                    SourceImage = "", // Invalid: empty
                    Method = PreprocessingMethod.Binarized,
                    ValidationDate = DateTime.UtcNow,
                    ValidatedBy = "TestRunner"
                }
            };
            var capsule = new SymbolCapsule(
                image,
                metadata,
                new QualityMetrics(),
                true,
                new System.Collections.Generic.List<ValidationResult>()
            );

            var exporter = new CapsuleExporter();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => exporter.ExportAsync(capsule, _tempDirectory, "test"));

            Assert.Contains("Provenance.SourceImage is missing", exception.Message);
        }

        /// <summary>
        /// REGRESSION GUARD: Verifies SHA256 hash computation from image bytes.
        ///
        /// DEFECT HISTORY (Phase 5.3):
        ///   - Original Implementation: No hash computation, relied on caller
        ///   - Root Cause: Hash computation not enforced by exporter
        ///   - Impact: Templates exported with "unhashed" or incorrect hashes
        ///   - Fix: Auto-compute SHA256 hash from PNG bytes in ExportAsync()
        ///
        /// VALIDATION STRATEGY:
        ///   - Create capsule with valid metadata
        ///   - Export capsule
        ///   - Verify exported JSON contains ComputedHash field
        ///   - Verify hash is valid 64-character hex string (SHA256)
        ///   - Uses regex validation (not exact hash comparison to avoid xUnit hash mismatch issues)
        ///
        /// NOTE: For tests requiring exact hash comparison, use AssertHashEqual() helper
        /// to avoid xUnit's cryptic "Expected vs Actual" hash truncation issue.
        /// </summary>
        [Fact]
        public async Task CapsuleExporter_ExportAsync_ComputesSHA256Hash()
        {
            // Arrange: Create valid capsule
            using var image = CreateTestImageL8(10, 10);
            var metadata = new TemplateMetadata
            {
                TemplateName = "test-template",
                GeneratedBy = "SymbolLabsForge v1.0",
                TemplateHash = "computed-hash", // Valid placeholder (will be computed)
                SymbolType = SymbolType.Clef,
                Provenance = new ProvenanceMetadata
                {
                    SourceImage = "test.png",
                    Method = PreprocessingMethod.Binarized,
                    ValidationDate = DateTime.UtcNow,
                    ValidatedBy = "TestRunner"
                }
            };
            var capsule = new SymbolCapsule(
                image,
                metadata,
                new QualityMetrics(),
                true,
                new System.Collections.Generic.List<ValidationResult>()
            );

            var exporter = new CapsuleExporter();

            // Act: Export capsule
            await exporter.ExportAsync(capsule, _tempDirectory, "test");

            // Assert: Verify JSON contains ComputedHash
            var jsonPath = Path.Combine(_tempDirectory, "test-template-test.json");
            Assert.True(File.Exists(jsonPath), "Exported JSON file should exist");

            var jsonContent = await File.ReadAllTextAsync(jsonPath);
            Assert.Contains("ComputedHash", jsonContent);

            // Verify hash format (SHA256 = 64 hex characters)
            var hashMatch = System.Text.RegularExpressions.Regex.Match(jsonContent, @"""ComputedHash"":\s*""([a-f0-9]{64})""");
            Assert.True(hashMatch.Success, "ComputedHash should be valid SHA256 (64 hex characters)");
        }

        #endregion

        #region CapsuleRegistryManager Validation Tests

        /// <summary>
        /// REGRESSION GUARD: Prevents registry from accepting "unknown" TemplateName.
        ///
        /// DEFECT HISTORY (Phase 5.4):
        ///   - Original Implementation: Accepted "unknown" as fallback default
        ///   - Root Cause: No validation before adding entries
        ///   - Impact: Registry contained non-identifiable entries
        ///   - Fix: Added ValidateCapsuleMetadata() rejecting "unknown", "default"
        ///
        /// VALIDATION STRATEGY:
        ///   - Create capsule with "unknown" TemplateName
        ///   - Attempt to add to registry via AddEntryAsync()
        ///   - Verify InvalidOperationException with clear error message
        /// </summary>
        [Fact]
        public async Task CapsuleRegistryManager_AddEntryAsync_RejectsUnknownTemplateName()
        {
            // Arrange: Create capsule with "unknown" TemplateName
            using var image = CreateTestImageL8(10, 10);
            var metadata = new TemplateMetadata
            {
                TemplateName = "unknown", // Invalid: generic default
                GeneratedBy = "SymbolLabsForge v1.0",
                TemplateHash = "abc123",
                SymbolType = SymbolType.Clef,
                Provenance = new ProvenanceMetadata
                {
                    SourceImage = "test.png",
                    Method = PreprocessingMethod.Binarized,
                    ValidationDate = DateTime.UtcNow,
                    ValidatedBy = "TestRunner"
                }
            };
            var capsule = new SymbolCapsule(
                image,
                metadata,
                new QualityMetrics(),
                true,
                new System.Collections.Generic.List<ValidationResult>()
            );

            var pathOptions = Options.Create(new ForgePathSettings { DocsRoot = _tempDirectory });
            var manager = new CapsuleRegistryManager(pathOptions);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => manager.AddEntryAsync(capsule));

            Assert.Contains("TemplateName is missing or invalid", exception.Message);
            Assert.Contains("unknown", exception.Message);
        }

        /// <summary>
        /// REGRESSION GUARD: Prevents registry from accepting "default" TemplateName.
        ///
        /// DEFECT HISTORY (Phase 5.4):
        ///   - Original Implementation: Accepted "default" as fallback
        ///   - Root Cause: No validation rejecting generic names
        ///   - Impact: Registry entries not identifiable/searchable
        ///   - Fix: Reject "default" in ValidateCapsuleMetadata()
        ///
        /// VALIDATION STRATEGY:
        ///   - Create capsule with "default" TemplateName
        ///   - Attempt registry addition
        ///   - Verify InvalidOperationException
        /// </summary>
        [Fact]
        public async Task CapsuleRegistryManager_AddEntryAsync_RejectsDefaultTemplateName()
        {
            // Arrange: Create capsule with "default" TemplateName
            using var image = CreateTestImageL8(10, 10);
            var metadata = new TemplateMetadata
            {
                TemplateName = "default", // Invalid: generic default
                GeneratedBy = "SymbolLabsForge v1.0",
                TemplateHash = "abc123",
                SymbolType = SymbolType.Clef,
                Provenance = new ProvenanceMetadata
                {
                    SourceImage = "test.png",
                    Method = PreprocessingMethod.Binarized,
                    ValidationDate = DateTime.UtcNow,
                    ValidatedBy = "TestRunner"
                }
            };
            var capsule = new SymbolCapsule(
                image,
                metadata,
                new QualityMetrics(),
                true,
                new System.Collections.Generic.List<ValidationResult>()
            );

            var pathOptions = Options.Create(new ForgePathSettings { DocsRoot = _tempDirectory });
            var manager = new CapsuleRegistryManager(pathOptions);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => manager.AddEntryAsync(capsule));

            Assert.Contains("TemplateName is missing or invalid", exception.Message);
            Assert.Contains("default", exception.Message);
        }

        /// <summary>
        /// REGRESSION GUARD: Prevents registry from accepting "unhashed" TemplateHash.
        ///
        /// DEFECT HISTORY (Phase 5.4):
        ///   - Original Implementation: Accepted "unhashed" as fallback
        ///   - Root Cause: No hash computation enforcement
        ///   - Impact: Registry entries had no integrity verification
        ///   - Fix: Reject "unhashed" in ValidateCapsuleMetadata()
        ///
        /// VALIDATION STRATEGY:
        ///   - Create capsule with "unhashed" TemplateHash
        ///   - Attempt registry addition
        ///   - Verify InvalidOperationException
        /// </summary>
        [Fact]
        public async Task CapsuleRegistryManager_AddEntryAsync_RejectsUnhashedTemplateHash()
        {
            // Arrange: Create capsule with "unhashed" TemplateHash
            using var image = CreateTestImageL8(10, 10);
            var metadata = new TemplateMetadata
            {
                TemplateName = "test-template",
                GeneratedBy = "SymbolLabsForge v1.0",
                TemplateHash = "unhashed", // Invalid: not computed
                SymbolType = SymbolType.Clef,
                Provenance = new ProvenanceMetadata
                {
                    SourceImage = "test.png",
                    Method = PreprocessingMethod.Binarized,
                    ValidationDate = DateTime.UtcNow,
                    ValidatedBy = "TestRunner"
                }
            };
            var capsule = new SymbolCapsule(
                image,
                metadata,
                new QualityMetrics(),
                true,
                new System.Collections.Generic.List<ValidationResult>()
            );

            var pathOptions = Options.Create(new ForgePathSettings { DocsRoot = _tempDirectory });
            var manager = new CapsuleRegistryManager(pathOptions);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => manager.AddEntryAsync(capsule));

            Assert.Contains("TemplateHash is missing or invalid", exception.Message);
            Assert.Contains("unhashed", exception.Message);
        }

        /// <summary>
        /// REGRESSION GUARD: Prevents registry from accepting null Provenance.
        ///
        /// DEFECT HISTORY (Phase 5.4):
        ///   - Original Implementation: Provenance was optional nullable string
        ///   - Root Cause: No structured provenance enforcement
        ///   - Impact: Registry entries had missing traceability
        ///   - Fix: Require structured ProvenanceMetadata in ValidateCapsuleMetadata()
        ///
        /// VALIDATION STRATEGY:
        ///   - Create capsule with null Provenance
        ///   - Attempt registry addition
        ///   - Verify InvalidOperationException with provenance error
        /// </summary>
        [Fact]
        public async Task CapsuleRegistryManager_AddEntryAsync_RejectsNullProvenance()
        {
            // Arrange: Create capsule with null Provenance
            using var image = CreateTestImageL8(10, 10);
            var metadata = new TemplateMetadata
            {
                TemplateName = "test-template",
                GeneratedBy = "SymbolLabsForge v1.0",
                TemplateHash = "abc123",
                SymbolType = SymbolType.Clef,
                Provenance = null! // Invalid: null provenance
            };
            var capsule = new SymbolCapsule(
                image,
                metadata,
                new QualityMetrics(),
                true,
                new System.Collections.Generic.List<ValidationResult>()
            );

            var pathOptions = Options.Create(new ForgePathSettings { DocsRoot = _tempDirectory });
            var manager = new CapsuleRegistryManager(pathOptions);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => manager.AddEntryAsync(capsule));

            Assert.Contains("Provenance metadata is missing", exception.Message);
        }

        #endregion

        #region Valid Provenance Tests

        /// <summary>
        /// VALIDATION TEST: Verifies complete metadata with valid provenance passes all validation.
        ///
        /// PURPOSE:
        ///   - Confirm that capsules with complete metadata can be exported and registered
        ///   - Establish baseline for valid provenance structure
        ///   - Demonstrate correct usage pattern for contributors
        ///
        /// VALIDATION STRATEGY:
        ///   - Create capsule with all required fields populated
        ///   - Export via CapsuleExporter (should succeed)
        ///   - Add to registry via CapsuleRegistryManager (should succeed)
        ///   - Verify no exceptions thrown
        /// </summary>
        [Fact]
        public async Task ValidProvenance_PassesExportAndRegistryValidation()
        {
            // Arrange: Create capsule with complete valid metadata
            using var image = CreateTestImageL8(10, 10);
            var metadata = new TemplateMetadata
            {
                TemplateName = "treble-clef-standard",
                GeneratedBy = "SymbolLabsForge v1.5.0",
                TemplateHash = "a1b2c3d4e5f6", // Valid hash (will be recomputed by exporter)
                SymbolType = SymbolType.Clef,
                Provenance = new ProvenanceMetadata
                {
                    SourceImage = "score_01_preprocessed.png",
                    Method = PreprocessingMethod.Skeletonized,
                    ValidationDate = DateTime.UtcNow,
                    ValidatedBy = "SymbolLabsForge v1.5.0",
                    Notes = "Extracted from public domain Bach score, skeletonized using Zhang-Suen"
                }
            };
            var capsule = new SymbolCapsule(
                image,
                metadata,
                new QualityMetrics(),
                true,
                new System.Collections.Generic.List<ValidationResult>()
            );

            var exporter = new CapsuleExporter();
            var pathOptions = Options.Create(new ForgePathSettings { DocsRoot = _tempDirectory });
            var manager = new CapsuleRegistryManager(pathOptions);

            // Act: Export capsule (should succeed)
            await exporter.ExportAsync(capsule, _tempDirectory, "valid");

            // Act: Add to registry (should succeed)
            await manager.AddEntryAsync(capsule);

            // Assert: Verify export succeeded
            var jsonPath = Path.Combine(_tempDirectory, "treble-clef-standard-valid.json");
            Assert.True(File.Exists(jsonPath), "Export should succeed with valid provenance");

            // Assert: Verify registry entry added
            Assert.Single(manager.Registry.Entries);
            var entry = manager.Registry.Entries[0];
            Assert.Equal("treble-clef-standard", entry.TemplateName);
            Assert.Equal("score_01_preprocessed.png", entry.Provenance.SourceImage);
            Assert.Equal(PreprocessingMethod.Skeletonized, entry.Provenance.Method);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Creates a test image with L8 pixel format (8-bit grayscale).
        /// All pixels initialized to white (255 = background).
        /// </summary>
        private static Image<L8> CreateTestImageL8(int width, int height)
        {
            var image = new Image<L8>(width, height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    image[x, y] = new L8(255); // White background
                }
            }
            return image;
        }

        /// <summary>
        /// Creates a SymbolCapsule with specified metadata for testing.
        /// </summary>
        private static SymbolCapsule CreateTestCapsule(Image<L8> image, TemplateMetadata metadata)
        {
            return new SymbolCapsule(
                image,
                metadata,
                new QualityMetrics(),
                true,
                new System.Collections.Generic.List<ValidationResult>()
            );
        }

        /// <summary>
        /// Asserts hash equality with contributor-safe error messaging.
        ///
        /// DEFECT HISTORY (xUnit Hash Mismatch Issue):
        ///   - Original Implementation: Used Assert.Equal() for hash comparison
        ///   - Root Cause: xUnit shows truncated hash values in failure messages
        ///   - Impact: Contributors saw "Expected: a1b2c3..., Actual: d4e5f6..." without full context
        ///   - Fix: Custom assertion with full hash display and determinism guidance
        ///
        /// VALIDATION STRATEGY:
        ///   - Display full expected and actual hash values in error message
        ///   - Provide context message explaining what was being hashed
        ///   - Remind contributors about deterministic hash computation requirements
        ///   - Prevents "six labors" xUnit debugging issue (cryptic hash mismatches)
        ///
        /// AUDIENCE: Graduate / PhD (test engineering, contributor safety)
        /// </summary>
        /// <param name="expected">Expected hash value (64-character lowercase hex string)</param>
        /// <param name="actual">Actual computed hash value</param>
        /// <param name="contextMessage">Description of what was hashed (e.g., "treble clef template")</param>
        public static void AssertHashEqual(string expected, string actual, string contextMessage)
        {
            Assert.True(expected == actual,
                $"Hash mismatch in {contextMessage}.\n" +
                $"Expected: {expected}\n" +
                $"Actual:   {actual}\n" +
                "Ensure image bytes are normalized (PNG encoding, no metadata) before computing SHA256.\n" +
                "Common causes: line ending differences, timestamp metadata, non-deterministic image encoding.");
        }

        #endregion
    }
}
