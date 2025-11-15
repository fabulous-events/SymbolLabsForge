namespace SymbolLabsForge.Contracts
{
    public class CapsuleRegistryEntry
    {
        public required string CapsuleId { get; set; }
        public required string TemplateName { get; set; }
        public required SymbolType SymbolType { get; set; }
        public required string TemplateHash { get; set; }
        public bool IsValid { get; set; }
        public required string SourcePath { get; set; }
        public string LastSeenUtc { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public string? Provenance { get; set; }
        public bool IsConflict { get; set; } = false;
    }
}
