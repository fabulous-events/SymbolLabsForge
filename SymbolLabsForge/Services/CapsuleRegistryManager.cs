//===============================================================
// File: CapsuleRegistryManager.cs
// Author: Gemini (Original), Claude (Registry & Provenance Tracking Phase 5.5)
// Date: 2025-11-14
// Purpose: Manages capsule registry with provenance validation and enforcement.
//
// REGISTRY & PROVENANCE TRACKING (Phase 5.5):
//   - Added metadata validation before adding entries (fail-fast)
//   - Enforces provenance completeness for all new capsules
//   - Handles legacy capsules without provenance (migration path)
//   - Rejects "unknown", "default", "unhashed" metadata
//
// DEFECT HISTORY:
//   - Original Implementation: Accepted incomplete metadata with "unknown" defaults
//   - Root Cause: No validation, no provenance enforcement
//   - Impact: Registry contained incomplete/untraceable entries
//   - Fix: Added ValidateCapsuleMetadata(), structured provenance extraction
//
// VALIDATION STRATEGY:
//   - AddEntryAsync: Validates all required fields before adding
//   - ScanDirectory: Extracts provenance metadata from JSON (handles legacy)
//   - Legacy capsules: Marked with "legacy-unknown" provenance for migration
//   - New capsules: Must have complete provenance or throw
//
// AUDIENCE: Graduate / PhD (registry management, provenance tracking)
//===============================================================
using System;
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

                    // Extract provenance metadata (if present)
                    ProvenanceMetadata? provenance = null;
                    if (metadata.TryGetProperty("Provenance", out var provenanceElement))
                    {
                        provenance = new ProvenanceMetadata
                        {
                            SourceImage = provenanceElement.GetProperty("SourceImage").GetString() ?? string.Empty,
                            Method = Enum.Parse<PreprocessingMethod>(provenanceElement.GetProperty("Method").GetString() ?? "Raw"),
                            ValidationDate = DateTime.Parse(provenanceElement.GetProperty("ValidationDate").GetString() ?? DateTime.UtcNow.ToString("o")),
                            ValidatedBy = provenanceElement.GetProperty("ValidatedBy").GetString() ?? string.Empty,
                            Notes = provenanceElement.TryGetProperty("Notes", out var notes) ? notes.GetString() : null
                        };
                    }
                    else
                    {
                        // Legacy capsules without provenance - create placeholder
                        // This allows scanning old capsules but marks them as incomplete
                        provenance = new ProvenanceMetadata
                        {
                            SourceImage = "legacy-unknown",
                            Method = PreprocessingMethod.Raw,
                            ValidationDate = DateTime.UtcNow,
                            ValidatedBy = "legacy-migration",
                            Notes = "Legacy capsule without provenance metadata - requires re-validation"
                        };
                    }

                    var entry = new CapsuleRegistryEntry
                    {
                        CapsuleId = metadata.GetProperty("CapsuleId").GetString() ?? "unknown",
                        TemplateName = metadata.GetProperty("TemplateName").GetString() ?? "unknown",
                        SymbolType = metadata.TryGetProperty("SymbolType", out var symbolType) ? Enum.Parse<SymbolType>(symbolType.GetString() ?? "Unknown") : SymbolType.Unknown,
                        TemplateHash = metadata.GetProperty("TemplateHash").GetString() ?? "unknown",
                        IsValid = doc.RootElement.GetProperty("ValidationResults").EnumerateArray().All(v => v.GetProperty("IsValid").GetBoolean()),
                        SourcePath = file,
                        LastSeenUtc = DateTime.UtcNow.ToString("o"),
                        SizeBytes = fileInfo.Length,
                        Provenance = provenance  // Add provenance metadata
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
        /// Validates metadata completeness before adding.
        /// Throws InvalidOperationException if metadata is incomplete.
        /// </summary>
        public async Task AddEntryAsync(SymbolCapsule capsule)
        {
            if (capsule == null) throw new ArgumentNullException(nameof(capsule));

            // REGISTRY & PROVENANCE TRACKING (Phase 5.5):
            // Validate metadata completeness BEFORE adding to registry
            ValidateCapsuleMetadata(capsule);

            var entry = new CapsuleRegistryEntry
            {
                CapsuleId = capsule.Metadata.CapsuleId,
                TemplateName = capsule.Metadata.TemplateName,
                SymbolType = capsule.Metadata.SymbolType,
                TemplateHash = capsule.Metadata.TemplateHash,
                IsValid = capsule.IsValid,
                SourcePath = _registryPath,
                LastSeenUtc = DateTime.UtcNow.ToString("o"),
                SizeBytes = capsule.TemplateImage?.Width * capsule.TemplateImage?.Height ?? 0,
                Provenance = capsule.Metadata.Provenance  // Copy structured provenance
            };

            Registry.Entries.Add(entry);
            await SaveRegistryAsync();
        }

        /// <summary>
        /// Validates capsule metadata completeness.
        /// Throws InvalidOperationException if required fields are missing or contain invalid defaults.
        /// </summary>
        private void ValidateCapsuleMetadata(SymbolCapsule capsule)
        {
            if (capsule.Metadata == null)
                throw new InvalidOperationException("Cannot add capsule to registry: Metadata is null.");

            // Validate TemplateName (no "unknown" or "default" allowed)
            if (string.IsNullOrWhiteSpace(capsule.Metadata.TemplateName) ||
                capsule.Metadata.TemplateName == "unknown" ||
                capsule.Metadata.TemplateName == "default")
                throw new InvalidOperationException($"Cannot add capsule to registry: TemplateName is missing or invalid ('{capsule.Metadata.TemplateName}'). Provide a descriptive name.");

            // Validate GeneratedBy (no "unknown" allowed)
            if (string.IsNullOrWhiteSpace(capsule.Metadata.GeneratedBy) ||
                capsule.Metadata.GeneratedBy == "unknown")
                throw new InvalidOperationException($"Cannot add capsule to registry: GeneratedBy is missing or invalid ('{capsule.Metadata.GeneratedBy}'). Specify the tool and version.");

            // Validate TemplateHash (no "unhashed" allowed)
            if (string.IsNullOrWhiteSpace(capsule.Metadata.TemplateHash) ||
                capsule.Metadata.TemplateHash == "unhashed")
                throw new InvalidOperationException($"Cannot add capsule to registry: TemplateHash is missing or invalid ('{capsule.Metadata.TemplateHash}'). Compute SHA256 hash before adding.");

            // Validate Provenance completeness
            if (capsule.Metadata.Provenance == null)
                throw new InvalidOperationException("Cannot add capsule to registry: Provenance metadata is missing. Provide source image, preprocessing method, and validation info.");

            if (string.IsNullOrWhiteSpace(capsule.Metadata.Provenance.SourceImage))
                throw new InvalidOperationException("Cannot add capsule to registry: Provenance.SourceImage is missing.");

            if (string.IsNullOrWhiteSpace(capsule.Metadata.Provenance.ValidatedBy))
                throw new InvalidOperationException("Cannot add capsule to registry: Provenance.ValidatedBy is missing.");

            if (capsule.Metadata.Provenance.ValidationDate == default(DateTime))
                throw new InvalidOperationException("Cannot add capsule to registry: Provenance.ValidationDate is missing or invalid.");
        }
    }
}
