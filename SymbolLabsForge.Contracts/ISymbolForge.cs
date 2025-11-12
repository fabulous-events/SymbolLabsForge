//===============================================================
// File: ISymbolForge.cs
// Author: Gemini
// Date: 2025-11-11
// Origin: Migrated from SymbolLabsForge.Contracts.cs
// Purpose: Defines the public API for the Synthetic Forge.
//===============================================================
#nullable enable

using System.Threading.Tasks;

namespace SymbolLabsForge.Contracts
{
    public interface ISymbolForge
    {
        SymbolCapsuleSet Generate(SymbolRequest request);
        Task<SymbolCapsule> MorphAsync(MorphRequest request);
    }
}
