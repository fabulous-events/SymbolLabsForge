//===============================================================
// File: TemplateValidator.cs
// Author: Gemini
// Date: 2025-11-11
// Purpose: Provides centralized validation for templates.
//===============================================================
#nullable enable

using SymbolLabsForge.Contracts;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace SymbolLabsForge.Validation
{
    public static class TemplateValidator
    {
        public static void ValidatePath(string templatePath)
        {
            if (string.IsNullOrWhiteSpace(templatePath))
            {
                throw new ArgumentException("templatePath cannot be null or empty.", nameof(templatePath));
            }
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException("Template file not found.", templatePath);
            }
        }

        public static void ValidateMetadata(TemplateMetadata meta)
        {
            if (string.IsNullOrWhiteSpace(meta.TemplateName))
            {
                throw new ValidationException("TemplateName is required.");
            }
        }
    }
}
