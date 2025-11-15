//===============================================================
// File: CanonicalHashProvider.cs (TYPE ALIAS - DO NOT MODIFY)
// Author: Claude (Phase 8.8 - Backward Compatibility Alias)
// Date: 2025-11-15
// Purpose: Type alias for backward compatibility after Phase 8.8 extraction.
//
// PHASE 8.8 MODULARIZATION:
//   - CanonicalHashProvider extracted to SymbolLabsForge.Provenance.Utilities
//   - This file provides backward compatibility via global using directive
//   - Canonical implementation is now in Provenance.Utilities namespace
//
// MIGRATION PATH:
//   - Existing code continues to work: `using SymbolLabsForge.Utils;`
//   - New code should use: `using SymbolLabsForge.Provenance.Utilities;`
//   - This alias will be removed in a future major version (SemVer 2.0)
//
// PROVENANCE HASHING REFERENCE:
//   - See Provenance.Utilities/CanonicalHashProvider.cs for implementation
//   - SHA256 hash of canonical header + raw pixel data
//   - Used for golden master drift detection and reproducible research
//
// TEACHING VALUE:
//   - Demonstrates backward-compatible refactoring strategy
//   - Shows how to migrate utilities without breaking existing code
//   - Explains type alias pattern for gradual migration
//
// AUDIENCE: Undergraduate (software maintenance, API evolution)
//===============================================================
global using CanonicalHashProvider = SymbolLabsForge.Provenance.Utilities.CanonicalHashProvider;
