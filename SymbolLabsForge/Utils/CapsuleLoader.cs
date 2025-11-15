using SymbolLabsForge.Contracts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SymbolLabsForge.Utils
{
    public static class CapsuleLoader
    {
        public static async Task<(SymbolCapsule Capsule, SymbolRequest Request)> LoadFromFileAsync(string jsonPath)
        {
            var jsonContent = await File.ReadAllTextAsync(jsonPath);
            
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
            var dto = JsonSerializer.Deserialize<CapsuleDto>(jsonContent, options);

            if (dto == null) throw new InvalidDataException("Failed to deserialize capsule JSON.");
            
            var imagePath = Path.ChangeExtension(jsonPath, ".png");
            if (!File.Exists(imagePath))
            {
                throw new FileNotFoundException($"Could not find the corresponding image file for the capsule.", imagePath);
            }
            var image = await Image.LoadAsync<L8>(imagePath);

            var finalSymbolType = dto.Metadata.SymbolType ?? InferSymbolTypeFromFilename(jsonPath);

            // Handle provenance: use DTO provenance if present, otherwise create legacy provenance
            var provenance = dto.Metadata.Provenance ?? new ProvenanceMetadata
            {
                SourceImage = "legacy-capsule",
                Method = PreprocessingMethod.Raw,
                ValidationDate = DateTime.UtcNow,
                ValidatedBy = "CapsuleLoader",
                Notes = $"Legacy capsule loaded from {Path.GetFileName(jsonPath)} - provenance not recorded"
            };

            var finalMetadata = new TemplateMetadata
            {
                TemplateName = dto.Metadata.TemplateName,
                SymbolType = finalSymbolType,
                TemplateHash = dto.Metadata.TemplateHash,
                CapsuleId = dto.Metadata.CapsuleId,
                GeneratedBy = dto.Metadata.GeneratedBy,
                GeneratedOn = dto.Metadata.GeneratedOn,
                GenerationSeed = dto.Metadata.GenerationSeed,
                Provenance = provenance
            };

            var capsule = new SymbolCapsule(image.Clone(), finalMetadata, dto.Metrics, dto.ValidationResults.All(vr => vr.IsValid), dto.ValidationResults);

            // Reconstruct the request using the reliable property
            var request = new SymbolRequest(
                finalSymbolType,
                new List<Size> { new Size(dto.Metrics.Width, dto.Metrics.Height) },
                new List<OutputForm> { OutputForm.Raw }, // Assume Raw as a base
                dto.Metadata.GenerationSeed
            );

            return (capsule, request);
        }

        private static SymbolType InferSymbolTypeFromFilename(string filename)
        {
            var name = Path.GetFileNameWithoutExtension(filename).ToLowerInvariant();
            if (name.Contains("clef")) return SymbolType.Clef;
            if (name.Contains("flat")) return SymbolType.Flat;
            if (name.Contains("sharp")) return SymbolType.Sharp;
            if (name.Contains("natural")) return SymbolType.Natural;
            return SymbolType.Unknown;
        }

        private class CapsuleDto
        {
            public required TemplateMetadataDto Metadata { get; set; }
            public required QualityMetrics Metrics { get; set; }
            public required List<ValidationResult> ValidationResults { get; set; }
        }

        private class TemplateMetadataDto
        {
            public string TemplateName { get; set; } = "";
            public SymbolType? SymbolType { get; set; }
            public string TemplateHash { get; set; } = "";
            public string CapsuleId { get; set; } = "";
            public string GeneratedBy { get; set; } = "unknown";
            public string GeneratedOn { get; set; } = DateTime.UtcNow.ToString("o");
            public int? GenerationSeed { get; set; }
            public ProvenanceMetadata? Provenance { get; set; } // Optional for backward compatibility with legacy capsules
        }
    }
}

