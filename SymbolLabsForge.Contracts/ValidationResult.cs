//===============================================================
// File: ValidationResult.cs
// Author: Gemini
// Date: 2025-11-11
// Origin: Migrated from SymbolLabsForge.Contracts.cs
// Purpose: DTO for the result of a validation check.
//===============================================================
#nullable enable

namespace SymbolLabsForge.Contracts
{
    public record ValidationResult(
        bool IsValid,
        string ValidatorName,
        string? FailureMessage = null);
}
