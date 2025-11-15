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
    public class StructureValidator : IValidator
    {
        public string Name => "StructureValidator";

        public ValidationResult Validate(SymbolCapsule capsule, QualityMetrics metrics)
        {
            if (capsule == null)
            {
                return new ValidationResult(false, Name, "Capsule cannot be null.");
            }

            var result = new ValidationResult(true, Name);
            var image = capsule.TemplateImage;
            var centerX = image.Width / 2;
            var centerY = image.Height / 2;
            var centerPixel = image[centerX, centerY];

            if (centerPixel.PackedValue > 128) // Assuming L8 format, 0 is black, 255 is white
            {
                return new ValidationResult(false, Name, "Symbol is hollow; center pixel is not ink.");
            }

            return result;
        }
    }
}