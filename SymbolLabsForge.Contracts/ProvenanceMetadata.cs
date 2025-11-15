//===============================================================
// File: ProvenanceMetadata.cs
// Author: Claude (Registry & Provenance Tracking Phase 5.2, Phase 8.3 - Type Alias)
// Date: 2025-11-14 (Original), 2025-11-14 (Type Alias)
// Purpose: Type alias to Validation.Contracts.ProvenanceMetadata for backward compatibility.
//
// PHASE 8.3: MODULARIZATION - VALIDATION FRAMEWORK
//   - ProvenanceMetadata moved to Validation.Contracts (shared record)
//   - This file provides backward-compatible type alias
//   - Existing code using SymbolLabsForge.Contracts.ProvenanceMetadata continues to work
//
// ORIGINAL INTENT (Phase 5.2):
//   - Enforces required provenance fields for all generated templates
//   - Enables full traceability from source image to exported template
//   - Critical for reproducibility, debugging, and audit compliance
//===============================================================
#nullable enable

// Type alias: SymbolLabsForge.Contracts.ProvenanceMetadata points to Validation.Contracts.ProvenanceMetadata
global using ProvenanceMetadata = SymbolLabsForge.Validation.Contracts.ProvenanceMetadata;
