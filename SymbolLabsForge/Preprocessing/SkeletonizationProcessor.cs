//===============================================================
// File: SkeletonizationProcessor.cs (TYPE ALIAS - DO NOT MODIFY)
// Author: Claude (Phase 8.8 - Backward Compatibility Alias)
// Date: 2025-11-15
// Purpose: Type aliases for backward compatibility after Phase 8.8 extraction.
//
// PHASE 8.8 MODULARIZATION:
//   - SkeletonizationProcessor and IPreprocessingStep extracted to SymbolLabsForge.ImageProcessing.Utilities
//   - This file provides backward compatibility via global using directives
//   - Canonical implementation is now in ImageProcessing.Utilities namespace
//
// MIGRATION PATH:
//   - Existing code continues to work: `using SymbolLabsForge.Preprocessing;`
//   - New code should use: `using SymbolLabsForge.ImageProcessing.Utilities;`
//   - These aliases will be removed in a future major version (SemVer 2.0)
//
// PHASE 2A BUG FIX REFERENCE:
//   - See ImageProcessing.Utilities/SkeletonizationProcessor.cs for full defect history
//   - Critical bug: Inverted pixel logic (0 treated as both ink AND background)
//   - Fix: Canonical PixelUtils.IsInk() standard enforcement
//
// TEACHING VALUE:
//   - Demonstrates backward-compatible refactoring strategy
//   - Shows how to migrate complex algorithms without breaking existing code
//   - Explains type alias pattern for gradual migration
//
// AUDIENCE: Undergraduate (software maintenance, API evolution)
//===============================================================
global using IPreprocessingStep = SymbolLabsForge.ImageProcessing.Utilities.IPreprocessingStep;
global using SkeletonizationProcessor = SymbolLabsForge.ImageProcessing.Utilities.SkeletonizationProcessor;
