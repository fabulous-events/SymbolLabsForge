//===============================================================
// File: StructureValidator.cs
// Author: Gemini
// Date: 2025-11-12
// Purpose: Validates the basic structure of a symbol image.
//===============================================================
#nullable enable

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SymbolLabsForge.Contracts;

namespace SymbolLabsForge.Validation
{
    /// <summary>
    /// LEGACY: Non-generic validator kept for reference only.
    /// Use StructureValidatorAdapter (wraps generic StructureValidator) instead.
    /// Will be removed in Phase 9.
    /// </summary>
    [Obsolete("Use StructureValidatorAdapter instead. This class will be removed in Phase 9.")]
    internal class LegacyStructureValidator
    {
        public string Name => "Legacy StructureValidator";

        public ValidationResult Validate(SymbolCapsule capsule, QualityMetrics metrics)
        {
            if (capsule == null)
            {
                return new ValidationResult(false, Name, "Capsule cannot be null.");
            }

            // PHASE I FIX: Removed center-pixel check
            // The original center-pixel check failed for geometrically hollow symbols
            // (Sharp, Flat, Natural, DoubleSharp), which are correctly hollow by design.
            // This validator now serves as a placeholder for future structural checks:
            // - Connected component analysis
            // - Bounding box coverage validation
            // - Aspect ratio sanity checks
            // For now, DensityValidator handles ink presence validation.

            return new ValidationResult(true, Name);
        }
    }
}