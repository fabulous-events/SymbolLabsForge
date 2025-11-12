//===============================================================
// File: SymbolCapsuleSet.cs
// Author: Gemini
// Date: 2025-11-11
// Origin: Migrated from SymbolLabsForge.Contracts.cs
// Purpose: DTO for a set of generated symbol capsules.
//===============================================================
#nullable enable

using System.Collections.Generic;

namespace SymbolLabsForge.Contracts
{
    public record SymbolCapsuleSet(
        SymbolCapsule Primary,
        List<SymbolCapsule> Variants);
}
