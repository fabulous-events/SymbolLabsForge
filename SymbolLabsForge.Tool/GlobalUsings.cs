//===============================================================
// File: GlobalUsings.cs
// Author: Claude (Phase 8.6 - Integration Testing)
// Date: 2025-11-14
// Purpose: Global using directives for Tool project.
//
// PHASE 8.6: MODULARIZATION - INTEGRATION TESTING
//   - Makes type aliases from Validation.Contracts visible project-wide
//   - Ensures Tool compiles after validator extraction
//===============================================================
#nullable enable

// Import type aliases from Validation.Contracts
global using ValidationResult = SymbolLabsForge.Validation.Contracts.ValidationResult;
global using DensityStatus = SymbolLabsForge.Validation.Contracts.DensityStatus;
global using ProvenanceMetadata = SymbolLabsForge.Validation.Contracts.ProvenanceMetadata;
global using PreprocessingMethod = SymbolLabsForge.Validation.Contracts.PreprocessingMethod;
