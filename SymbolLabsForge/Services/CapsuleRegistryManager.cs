using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SymbolLabsForge.Contracts;
using Microsoft.Extensions.Options;
using SymbolLabsForge.Configuration;

namespace SymbolLabsForge.Services
{
    public class CapsuleRegistryManager
    {
        private readonly string _registryPath;
        public CapsuleRegistry Registry { get; private set; }

        public CapsuleRegistryManager(IOptions<ForgePathSettings> pathOptions)
        {
            _registryPath = Path.Combine(pathOptions.Value.DocsRoot, "CapsuleRegistry.json");
            Registry = new CapsuleRegistry();
        }

        public async Task LoadRegistryAsync()
        {
            if (!File.Exists(_registryPath))
            {
                Registry = new CapsuleRegistry();
                return;
            }

            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() }
            };
            var json = await File.ReadAllTextAsync(_registryPath);
            Registry = JsonSerializer.Deserialize<CapsuleRegistry>(json, options) ?? new CapsuleRegistry();
        }

        public async Task SaveRegistryAsync()
        {
            Registry.LastUpdated = System.DateTime.UtcNow.ToString("o");
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            };
            var json = JsonSerializer.Serialize(Registry, options);

            // Atomic write pattern
            var tempPath = Path.GetTempFileName();
            await File.WriteAllTextAsync(tempPath, json);

            if (File.Exists(_registryPath))
            {
                File.Replace(tempPath, _registryPath, null);
            }
            else
            {
                File.Move(tempPath, _registryPath);
            }
        }

        public async Task ScanDirectory(string directoryPath, bool incremental = false)
        {
            if (incremental)
            {
                await LoadRegistryAsync();
            }
            else
            {
                Registry.Entries.Clear();
            }

            var jsonFiles = Directory.EnumerateFiles(directoryPath, "*.json", SearchOption.AllDirectories);

            foreach (var file in jsonFiles)
            {
                try
                {
                    var fileInfo = new FileInfo(file);
                    var jsonContent = await File.ReadAllTextAsync(file);

                    // Partial deserialization to get only what we need
                    using var doc = JsonDocument.Parse(jsonContent);
                    var metadata = doc.RootElement.GetProperty("Metadata");

                    var entry = new CapsuleRegistryEntry
                    {
                        CapsuleId = metadata.GetProperty("CapsuleId").GetString() ?? "unknown",
                        TemplateName = metadata.GetProperty("TemplateName").GetString() ?? "unknown",
                        SymbolType = metadata.TryGetProperty("SymbolType", out var symbolType) ? Enum.Parse<SymbolType>(symbolType.GetString() ?? "Unknown") : SymbolType.Unknown,
                        TemplateHash = metadata.GetProperty("TemplateHash").GetString() ?? "unknown",
                        IsValid = doc.RootElement.GetProperty("ValidationResults").EnumerateArray().All(v => v.GetProperty("IsValid").GetBoolean()),
                        SourcePath = file,
                        LastSeenUtc = DateTime.UtcNow.ToString("o"),
                        SizeBytes = fileInfo.Length
                    };

                    // Simple conflict check (can be improved)
                    var existingEntry = Registry.Entries.FirstOrDefault(e => e.CapsuleId == entry.CapsuleId);
                    if (existingEntry != null)
                    {
                        if (existingEntry.TemplateHash != entry.TemplateHash)
                        {
                            existingEntry.IsConflict = true;
                            entry.IsConflict = true;
                            Registry.Entries.Add(entry); // Add the conflicting entry
                        }
                        else
                        {
                            // Update existing entry
                            existingEntry.SourcePath = entry.SourcePath;
                            existingEntry.LastSeenUtc = entry.LastSeenUtc;
                            existingEntry.SizeBytes = entry.SizeBytes;
                            existingEntry.IsValid = entry.IsValid;
                            existingEntry.IsConflict = false; // Ensure it's not marked as conflict if hashes match
                        }
                    }
                    else
                    {
                        Registry.Entries.Add(entry);
                    }
                }
                catch (JsonException)
                {
                    // Log and continue as per instructions
                    Console.Error.WriteLine($"Warning: Skipping invalid JSON file: {file}");
                }
                catch (KeyNotFoundException)
                {
                    Console.Error.WriteLine($"Warning: Skipping file with missing metadata: {file}");
                }
            }
        }

        /// <summary>
        /// Adds a capsule entry to the registry and persists it.
        /// </summary>
        public async Task AddEntryAsync(SymbolCapsule capsule)
        {
            if (capsule == null) throw new ArgumentNullException(nameof(capsule));

            var entry = new CapsuleRegistryEntry
            {
                CapsuleId = capsule.Metadata.CapsuleId ?? "unknown",
                TemplateName = capsule.Metadata.TemplateName ?? "unknown",
                SymbolType = capsule.Metadata.SymbolType,
                TemplateHash = capsule.Metadata.TemplateHash ?? "unknown",
                IsValid = capsule.IsValid,
                SourcePath = _registryPath,
                LastSeenUtc = DateTime.UtcNow.ToString("o"),
                SizeBytes = capsule.TemplateImage?.Width * capsule.TemplateImage?.Height ?? 0
            };

            Registry.Entries.Add(entry);
            await SaveRegistryAsync();
        }
    }
}
