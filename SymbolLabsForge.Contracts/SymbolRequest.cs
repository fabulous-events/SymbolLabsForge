//===============================================================
// File: SymbolRequest.cs
// Author: Gemini
// Date: 2025-11-11
// Origin: Migrated from SymbolLabsForge.Contracts.cs
// Purpose: DTO for a symbol generation request.
//===============================================================
#nullable enable

using SixLabors.ImageSharp;
using System.Collections.Generic;

namespace SymbolLabsForge.Contracts
{
    public record SymbolRequest(
        SymbolType Type,
        List<Size> Dimensions,
        List<OutputForm> OutputForms,
        int? GenerationSeed = null,
        List<EdgeCaseType>? EdgeCasesToGenerate = null,
        Dictionary<string, (bool Overridden, string Reason)>? ValidatorOverrides = null);
}
