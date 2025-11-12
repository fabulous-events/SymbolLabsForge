using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SixLabors.ImageSharp.Formats.Png;
using SymbolLabsForge.Contracts;
using System.IO;
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
            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Converters = { new StringEnumConverter() }
            };
            var jsonContent = JsonConvert.SerializeObject(capsuleDto, jsonSettings);
            await File.WriteAllTextAsync(jsonPath, jsonContent);
        }
    }
}
