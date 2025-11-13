using SymbolLabsForge.Contracts;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using System.Threading.Tasks;

namespace SymbolLabsForge.Validator
{
    public static class CapsuleLoader
    {
        public static async Task<(SymbolCapsule, SymbolRequest)> LoadFromFileAsync(string filePath)
        {
            var json = await File.ReadAllTextAsync(filePath);
            var capsule = JsonConvert.DeserializeObject<SymbolCapsule>(json);
            var request = new SymbolRequest(capsule.Metadata.SymbolType, new List<Size> { new Size(capsule.TemplateImage.Width, capsule.TemplateImage.Height) }, new List<OutputForm> { OutputForm.Raw });
            return (capsule, request);
        }
    }
}