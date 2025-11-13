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
            if (capsule?.TemplateImage == null)
            {
                return new ValidationResult(false, Name, "Input capsule or image is null.");
            }

            var image = capsule.TemplateImage;
            bool hasContent = false;

            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    foreach (var pixel in accessor.GetRowSpan(y))
                    {
                        if (pixel.PackedValue < 255) // Found a non-white pixel
                        {
                            hasContent = true;
                            return;
                        }
                    }
                }
            });

            if (!hasContent)
            {
                return new ValidationResult(false, Name, "Image canvas is empty.");
            }

            return new ValidationResult(true, Name);
        }
    }
}