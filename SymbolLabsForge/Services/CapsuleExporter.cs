using SixLabors.ImageSharp.Formats.Png;
using SymbolLabsForge.Contracts;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SymbolLabsForge.Services
{
    public class CapsuleExporter
    {
        public async Task ExportAsync(SymbolCapsule capsule, string basePath, string form)
        {
            // 1. Define filenames
            var baseFileName = $"{capsule.Metadata.TemplateName}-{form}".ToLower();
            var imagePath = Path.Combine(basePath, $"{baseFileName}.png");
            var jsonPath = Path.Combine(basePath, $"{baseFileName}.json");

            // 2. Save the image
            using (var fileStream = new FileStream(imagePath, FileMode.Create))
            {
                await capsule.TemplateImage.SaveAsync(fileStream, new PngEncoder());
            }

            // 3. Create a DTO for serialization
            var capsuleDto = new
            {
                capsule.Metadata,
                capsule.Metrics,
                capsule.ValidationResults
            };

            // 4. Serialize and save the JSON
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            };
            var jsonContent = JsonSerializer.Serialize(capsuleDto, jsonOptions);
            await File.WriteAllTextAsync(jsonPath, jsonContent);
        }
    }
}
