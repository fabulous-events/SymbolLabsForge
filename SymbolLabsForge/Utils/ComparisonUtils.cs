//===============================================================
// File: ComparisonUtils.cs (TYPE ALIAS - DO NOT MODIFY)
// Author: Claude (Phase 8.8 - Backward Compatibility Alias)
// Date: 2025-11-15
// Purpose: Type aliases for backward compatibility after Phase 8.8 extraction.
//
// PHASE 8.8 MODULARIZATION:
//   - SnapshotComparer and ImageDiffGenerator extracted to SymbolLabsForge.Testing.Utilities
//   - This file provides backward compatibility via global using directives
//   - Canonical implementation is now in Testing.Utilities namespace
//
// MIGRATION PATH:
//   - Existing code continues to work: `using SymbolLabsForge.Utils;`
//   - New code should use: `using SymbolLabsForge.Testing.Utilities;`
//   - These aliases will be removed in a future major version (SemVer 2.0)
//
// VISUAL REGRESSION TESTING REFERENCE:
//   - See Testing.Utilities/ComparisonUtils.cs for implementation
//   - Snapshot comparison for test automation
//   - Diff image generation for failed comparisons
//
// NOTE: Current implementation is minimal (stubs). Future enhancements planned:
//   - Pixel-by-pixel comparison with tolerance
//   - Visual diff image generation (highlight changed pixels)
//   - Perceptual hashing (pHash) for similarity detection
//
// TEACHING VALUE:
//   - Demonstrates backward-compatible refactoring strategy
//   - Shows how to migrate test utilities without breaking existing tests
//   - Explains type alias pattern for gradual migration
//
// AUDIENCE: Undergraduate (software maintenance, API evolution)
//===============================================================
global using SnapshotComparer = SymbolLabsForge.Testing.Utilities.SnapshotComparer;
global using ImageDiffGenerator = SymbolLabsForge.Testing.Utilities.ImageDiffGenerator;
