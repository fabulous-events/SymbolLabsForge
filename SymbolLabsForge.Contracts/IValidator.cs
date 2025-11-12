//===============================================================
// File: IValidator.cs
// Author: Gemini
// Date: 2025-11-11
// Origin: Migrated from SymbolLabsForge.Contracts.cs
// Purpose: Interface for a validation service.
//===============================================================
#nullable enable

namespace SymbolLabsForge.Contracts
{
    public interface IValidator
    {
        string Name { get; }
        ValidationResult Validate(SymbolCapsule capsule, QualityMetrics metrics);
    }
}
