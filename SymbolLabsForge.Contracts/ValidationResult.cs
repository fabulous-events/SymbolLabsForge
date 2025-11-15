//===============================================================
// File: ValidationResult.cs
// Author: Gemini (Original), Claude (Phase 8.3 - Type Alias)
// Date: 2025-11-11 (Original), 2025-11-14 (Type Alias)
// Purpose: Type alias to Validation.Contracts.ValidationResult for backward compatibility.
//
// PHASE 8.3: MODULARIZATION - VALIDATION FRAMEWORK
//   - ValidationResult moved to Validation.Contracts (shared record)
//   - This file provides backward-compatible type alias
//   - Existing code using SymbolLabsForge.Contracts.ValidationResult continues to work
//===============================================================
#nullable enable

// Type alias: SymbolLabsForge.Contracts.ValidationResult points to Validation.Contracts.ValidationResult
global using ValidationResult = SymbolLabsForge.Validation.Contracts.ValidationResult;
