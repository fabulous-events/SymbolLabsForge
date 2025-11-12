using System;
using SymbolLabsForge.Contracts;

namespace SymbolLabsForge.Analysis
{
    public record LineageNode(
        string CapsuleId,
        SymbolType Type,
        string Style,
        float? InterpolationFactor,
        string Contributor,
        DateTime GeneratedOn
    );
}
