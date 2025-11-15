//===============================================================
// File: GlobalUsings.cs
// Author: Claude (Phase 8.6 - Integration Testing)
// Date: 2025-11-14
// Purpose: Global using directives for test project.
//
// PHASE 8.6: MODULARIZATION - INTEGRATION TESTING
//   - Makes type aliases from Validation.Contracts visible project-wide
//   - Ensures tests compile after validator extraction
//===============================================================
#nullable enable

// Import type aliases from Validation.Contracts
global using ValidationResult = SymbolLabsForge.Validation.Contracts.ValidationResult;
global using DensityStatus = SymbolLabsForge.Validation.Contracts.DensityStatus;
global using ProvenanceMetadata = SymbolLabsForge.Validation.Contracts.ProvenanceMetadata;
global using PreprocessingMethod = SymbolLabsForge.Validation.Contracts.PreprocessingMethod;

// Import validator adapters
global using DensityValidator = SymbolLabsForge.Validation.DensityValidatorAdapter;
global using ContrastValidator = SymbolLabsForge.Validation.ContrastValidatorAdapter;
global using StructureValidator = SymbolLabsForge.Validation.StructureValidatorAdapter;

// Import pixel utilities
global using PixelUtils = SymbolLabsForge.ImageProcessing.Utilities.PixelUtils;
global using Constants = SymbolLabsForge.ImageProcessing.Utilities.Constants;
