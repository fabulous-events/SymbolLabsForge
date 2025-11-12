using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SymbolLabsForge.Contracts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SymbolLabsForge.Utils
{
    public static class CapsuleLoader
    {
        public static async Task<(SymbolCapsule Capsule, SymbolRequest Request)> LoadFromFileAsync(string jsonPath)
        {
            var jsonContent = await File.ReadAllTextAsync(jsonPath);
            var jsonObject = JObject.Parse(jsonContent);

            var metadataToken = jsonObject["Metadata"];
            if (metadataToken == null) throw new InvalidDataException("Capsule JSON is missing 'Metadata' block.");
            var metadata = metadataToken.ToObject<TemplateMetadata>() ?? throw new InvalidDataException("Failed to deserialize 'Metadata'.");

            var metricsToken = jsonObject["Metrics"];
            if (metricsToken == null) throw new InvalidDataException("Capsule JSON is missing 'Metrics' block.");
            var metrics = metricsToken.ToObject<QualityMetrics>() ?? throw new InvalidDataException("Failed to deserialize 'Metrics'.");

            var validationResultsToken = jsonObject["ValidationResults"];
            var validationResults = validationResultsToken?.ToObject<List<ValidationResult>>() ?? new List<ValidationResult>();

            var imagePath = Path.ChangeExtension(jsonPath, ".png");
            if (!File.Exists(imagePath))
            {
                throw new FileNotFoundException($"Could not find the corresponding image file for the capsule.", imagePath);
            }
            var image = await Image.LoadAsync<L8>(imagePath);

            var capsule = new SymbolCapsule(image, metadata, metrics, validationResults.All(vr => vr.IsValid), validationResults);

            // Reconstruct the request
            var request = new SymbolRequest(
                Enum.Parse<SymbolType>(metadata.TemplateName.Split('-')[0], true), // Infer from name
                new List<Size> { new Size(metrics.Width, metrics.Height) },
                new List<OutputForm> { OutputForm.Raw }, // Assume Raw as a base
                metadata.GenerationSeed
            );

            return (capsule, request);
        }
    }
}
