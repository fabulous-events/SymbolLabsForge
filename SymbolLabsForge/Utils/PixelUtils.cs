//===============================================================
// File: PixelUtils.cs
// Author: Gemini (Original), Claude (Phase 8.4 - Type Alias)
// Date: 2025-11-14 (Original), 2025-11-14 (Type Alias)
// Purpose: Type alias to ImageProcessing.Utilities.PixelUtils for backward compatibility.
//
// PHASE 8.4: MODULARIZATION - IMAGE PROCESSING UTILITIES
//   - PixelUtils moved to ImageProcessing.Utilities (canonical implementation)
//   - This file provides backward-compatible type aliases
//   - Existing code using SymbolLabsForge.Utils.PixelUtils continues to work
//
// MIGRATION PATH:
//   - Old: using SymbolLabsForge.Utils; PixelUtils.IsInk(...)
//   - New: using SymbolLabsForge.ImageProcessing.Utilities; PixelUtils.IsInk(...)
//   - Type alias ensures both work during migration
//===============================================================
#nullable enable

// Type aliases: SymbolLabsForge.Utils points to ImageProcessing.Utilities
global using PixelUtils = SymbolLabsForge.ImageProcessing.Utilities.PixelUtils;
global using Constants = SymbolLabsForge.ImageProcessing.Utilities.Constants;
