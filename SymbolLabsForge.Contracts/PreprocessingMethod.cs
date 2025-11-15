//===============================================================
// File: PreprocessingMethod.cs
// Author: Claude (Registry & Provenance Tracking Phase 5.2, Phase 8.3 - Type Alias)
// Date: 2025-11-14 (Original), 2025-11-14 (Type Alias)
// Purpose: Type alias to Validation.Contracts.PreprocessingMethod for backward compatibility.
//
// PHASE 8.3: MODULARIZATION - VALIDATION FRAMEWORK
//   - PreprocessingMethod moved to Validation.Contracts (shared enum)
//   - This file provides backward-compatible type alias
//   - Existing code using SymbolLabsForge.Contracts.PreprocessingMethod continues to work
//
// ORIGINAL INTENT (Phase 5.2):
//   - Defines standard preprocessing methods for traceability
//   - Ensures templates document their preprocessing pipeline
//   - Critical for reproducibility and debugging
//===============================================================
#nullable enable

// Type alias: SymbolLabsForge.Contracts.PreprocessingMethod points to Validation.Contracts.PreprocessingMethod
global using PreprocessingMethod = SymbolLabsForge.Validation.Contracts.PreprocessingMethod;
