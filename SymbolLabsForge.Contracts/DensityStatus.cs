//===============================================================
// File: DensityStatus.cs
// Author: Gemini (Original), Claude (Phase 8.3 - Type Alias)
// Date: 2025-11-11 (Original), 2025-11-14 (Type Alias)
// Purpose: Type alias to Validation.Contracts.DensityStatus for backward compatibility.
//
// PHASE 8.3: MODULARIZATION - VALIDATION FRAMEWORK
//   - DensityStatus moved to Validation.Contracts (shared enum)
//   - This file provides backward-compatible type alias
//   - Existing code using SymbolLabsForge.Contracts.DensityStatus continues to work
//
// MIGRATION NOTE:
//   - Old: DensityStatus.Valid → New: DensityStatus.Acceptable
//   - All other values remain the same (Unknown, TooHigh, TooLow)
//===============================================================
#nullable enable

// Type alias: SymbolLabsForge.Contracts.DensityStatus points to Validation.Contracts.DensityStatus
global using DensityStatus = SymbolLabsForge.Validation.Contracts.DensityStatus;
