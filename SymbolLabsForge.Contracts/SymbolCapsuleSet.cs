//===============================================================
// File: SymbolCapsuleSet.cs
// Author: Gemini
// Date: 2025-11-11
// Origin: Migrated from SymbolLabsForge.Contracts.cs
// Purpose: DTO for a set of generated symbol capsules.
//===============================================================
#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace SymbolLabsForge.Contracts
{
    public record SymbolCapsuleSet(
        SymbolCapsule Primary,
        List<SymbolCapsule> Variants) : IDisposable
    {
        public void Dispose()
        {
            Primary?.Dispose();
            if (Variants != null)
            {
                foreach (var variant in Variants)
                {
                    variant?.Dispose();
                }
            }
        }
    }
}
