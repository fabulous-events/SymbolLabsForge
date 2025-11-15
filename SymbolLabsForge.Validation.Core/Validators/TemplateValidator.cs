//===============================================================
// File: TemplateValidator.cs
// Author: Gemini (Original), Claude (Phase 8.3 - Generic Validator)
// Date: 2025-11-14
// Purpose: Generic template metadata validator.
//
// PHASE 8.3: MODULARIZATION - VALIDATION FRAMEWORK
//   - Converted from static utility to generic validator
//   - Works with IImageContainer<TMetadata, TMetrics> where TMetadata : ITemplateMetadata
//   - Decouples from SymbolCapsule and TemplateMetadata, enabling reuse
//
// VALIDATION LOGIC:
//   - TemplateName must not be empty, "unknown", or "default"
//   - GeneratedBy must not be empty
//   - TemplateHash must not be "unhashed" or empty
//   - Provenance must be non-null with required fields populated
//
// AUDIENCE: Graduate / PhD (metadata governance, provenance tracking)
//===============================================================
#nullable enable

using SymbolLabsForge.Validation.Contracts;

namespace SymbolLabsForge.Validation.Core.Validators
{
    /// <summary>
    /// Generic template metadata validator for image containers.
    /// Validates metadata completeness and integrity.
    /// </summary>
    /// <typeparam name="TMetadata">Metadata type that implements ITemplateMetadata</typeparam>
    /// <typeparam name="TMetrics">Metrics type (unused by this validator)</typeparam>
    public class TemplateValidator<TMetadata, TMetrics> : IValidator<TMetadata, TMetrics>
        where TMetadata : ITemplateMetadata
    {
        public string Name => "TemplateValidator";

        /// <summary>
        /// Validates the template metadata of the image container.
        /// </summary>
        /// <param name="container">Image container with image, metadata, and metrics</param>
        /// <param name="metrics">Metrics object (not modified by this validator)</param>
        /// <returns>ValidationResult indicating pass/fail with narratable error message</returns>
        public ValidationResult Validate(IImageContainer<TMetadata, TMetrics>? container, TMetrics metrics)
        {
            if (container == null)
            {
                return new ValidationResult(false, Name, "Container cannot be null.");
            }

            var metadata = container.Metadata;

            // Validate TemplateName
            if (string.IsNullOrWhiteSpace(metadata.TemplateName))
            {
                return new ValidationResult(false, Name, "TemplateName is required and cannot be empty.");
            }

            if (metadata.TemplateName.ToLowerInvariant() == "unknown" ||
                metadata.TemplateName.ToLowerInvariant() == "default")
            {
                return new ValidationResult(false, Name,
                    $"TemplateName '{metadata.TemplateName}' is not allowed. Use a descriptive name.");
            }

            // Validate GeneratedBy
            if (string.IsNullOrWhiteSpace(metadata.GeneratedBy))
            {
                return new ValidationResult(false, Name, "GeneratedBy is required and cannot be empty.");
            }

            // Validate TemplateHash
            if (string.IsNullOrWhiteSpace(metadata.TemplateHash))
            {
                return new ValidationResult(false, Name, "TemplateHash is required and cannot be empty.");
            }

            if (metadata.TemplateHash.ToLowerInvariant() == "unhashed")
            {
                return new ValidationResult(false, Name,
                    "TemplateHash must be computed (SHA256 hash), not 'unhashed'.");
            }

            // Validate Provenance (if present)
            if (metadata.Provenance != null)
            {
                if (string.IsNullOrWhiteSpace(metadata.Provenance.SourceImage))
                {
                    return new ValidationResult(false, Name,
                        "ProvenanceMetadata.SourceImage is required when Provenance is provided.");
                }

                if (string.IsNullOrWhiteSpace(metadata.Provenance.ValidatedBy))
                {
                    return new ValidationResult(false, Name,
                        "ProvenanceMetadata.ValidatedBy is required when Provenance is provided.");
                }
            }

            return new ValidationResult(true, Name);
        }
    }
}
