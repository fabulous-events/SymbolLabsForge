//===============================================================
// File: ValidationResult.cs
// Author: Gemini (Original), Claude (Phase 8.3 - Modularization)
// Date: 2025-11-11 (Original), 2025-11-14 (Moved to Validation.Contracts)
// Purpose: DTO for the result of a validation check with narratable error messages.
//
// PHASE 8.3: MODULARIZATION - VALIDATION FRAMEWORK
//   - Moved from SymbolLabsForge.Contracts to Validation.Contracts
//   - Shared by both generic and legacy validators
//   - FailureMessage provides student-friendly explanations
//
// DESIGN RATIONALE:
//   - Record type ensures immutability (validation results shouldn't change)
//   - FailureMessage is optional (null for PASS, populated for FAIL)
//   - ValidatorName enables tracing which validator failed
//
// AUDIENCE: Undergraduate / Graduate (validation patterns)
//===============================================================
#nullable enable

namespace SymbolLabsForge.Validation.Contracts
{
    /// <summary>
    /// Result of a validation operation with narratable error messages.
    /// </summary>
    /// <param name="IsValid">True if validation passed, false if failed</param>
    /// <param name="ValidatorName">Name of the validator that produced this result</param>
    /// <param name="FailureMessage">
    /// Narratable explanation of why validation failed (null if passed).
    /// Example: "Density of 12.5% is above the 8% threshold."
    /// </param>
    public record ValidationResult(
        bool IsValid,
        string ValidatorName,
        string? FailureMessage = null);
}
