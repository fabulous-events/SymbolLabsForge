//===============================================================
// File: ISymbolGenerator.cs
// Author: Gemini
// Date: 2025-11-11
// Origin: Migrated from SymbolLabsForge.Contracts.cs
// Purpose: Interface for a symbol generation service.
//===============================================================
#nullable enable

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SymbolLabsForge.Contracts
{
    public interface ISymbolGenerator
    {
        SymbolType SupportedType { get; }
        Image<L8> GenerateRawImage(Size dimensions, int? seed);
    }
}
