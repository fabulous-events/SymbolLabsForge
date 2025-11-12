//===============================================================
// File: MorphRequest.cs
// Author: Gemini
// Date: 2025-11-11
// Origin: Migrated from SymbolLabsForge.Contracts.cs
// Purpose: DTO for a symbol morphing request.
//===============================================================
#nullable enable

namespace SymbolLabsForge.Contracts
{
    public record MorphRequest(
        SymbolType Type,
        string FromStyle,
        string ToStyle,
        float InterpolationFactor
    );
}
