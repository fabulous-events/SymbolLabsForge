using Newtonsoft.Json;
using SymbolLabsForge.Contracts;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SymbolLabsForge.Services
{
    public class CapsuleRegistryManager
    {
        private readonly string _registryPath;

        public CapsuleRegistryManager(string registryPath)
        {
            _registryPath = registryPath;
        }

        public async Task AddEntryAsync(SymbolCapsule capsule)
        {
            var registry = await ReadRegistryAsync();

            var newEntry = new CapsuleRegistryEntry
            {
                CapsuleId = capsule.Metadata.CapsuleId,
                TemplateHash = capsule.Metadata.TemplateHash,
                GeneratedOn = capsule.Metadata.GeneratedOn,
                IsValid = capsule.IsValid
            };

            // Avoid duplicate entries
            if (!registry.Capsules.Any(c => c.CapsuleId == newEntry.CapsuleId))
            {
                registry.Capsules.Add(newEntry);
                await WriteRegistryAsync(registry);
            }
        }

        private async Task<CapsuleRegistry> ReadRegistryAsync()
        {
            if (!File.Exists(_registryPath))
            {
                return new CapsuleRegistry { Capsules = new List<CapsuleRegistryEntry>() };
            }

            var jsonContent = await File.ReadAllTextAsync(_registryPath);
            return JsonConvert.DeserializeObject<CapsuleRegistry>(jsonContent) ?? new CapsuleRegistry { Capsules = new List<CapsuleRegistryEntry>() };
        }

        private async Task WriteRegistryAsync(CapsuleRegistry registry)
        {
            var jsonContent = JsonConvert.SerializeObject(registry, Formatting.Indented);
            await File.WriteAllTextAsync(_registryPath, jsonContent);
        }
    }

    // DTOs for registry serialization
    public class CapsuleRegistry
    {
        public List<CapsuleRegistryEntry> Capsules { get; set; } = new List<CapsuleRegistryEntry>();
    }

    public class CapsuleRegistryEntry
    {
        public string CapsuleId { get; set; } = string.Empty;
        public string TemplateHash { get; set; } = string.Empty;
        public string GeneratedOn { get; set; } = string.Empty;
        public bool IsValid { get; set; }
    }
}
