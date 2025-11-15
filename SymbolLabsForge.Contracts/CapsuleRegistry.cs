using System.Collections.Generic;

namespace SymbolLabsForge.Contracts
{
    public class CapsuleRegistry
    {
        public int Version { get; set; } = 1;
        public string LastUpdated { get; set; } = string.Empty;
        public List<CapsuleRegistryEntry> Entries { get; set; } = new();
    }
}
