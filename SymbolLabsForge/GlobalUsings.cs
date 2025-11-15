//===============================================================
// File: GlobalUsings.cs
// Author: Claude (Phase 8.3 - Modularization)
// Date: 2025-11-14
// Purpose: Global using directives for SymbolLabsForge project.
//
// PHASE 8.3: MODULARIZATION - VALIDATION FRAMEWORK
//   - Makes type aliases from Validation.Contracts visible project-wide
//   - Enables backward-compatible migration without changing every file
//   - ValidationResult, DensityStatus, ProvenanceMetadata, PreprocessingMethod
//
// AUDIENCE: Graduate / PhD (C# 10 global usings feature)
//===============================================================
#nullable enable

// Import type aliases from Validation.Contracts
global using ValidationResult = SymbolLabsForge.Validation.Contracts.ValidationResult;
global using DensityStatus = SymbolLabsForge.Validation.Contracts.DensityStatus;
global using ProvenanceMetadata = SymbolLabsForge.Validation.Contracts.ProvenanceMetadata;
global using PreprocessingMethod = SymbolLabsForge.Validation.Contracts.PreprocessingMethod;
