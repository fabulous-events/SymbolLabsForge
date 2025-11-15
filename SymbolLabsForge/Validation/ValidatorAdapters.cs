//===============================================================
// File: ValidatorAdapters.cs
// Author: Claude (Phase 8.3 - Modularization)
// Date: 2025-11-14
// Purpose: Backward-compatible adapters for generic validators.
//
// PHASE 8.3: MODULARIZATION - VALIDATION FRAMEWORK
//   - Adapters wrap generic validators from Validation.Core
//   - Implement non-generic IValidator interface for backward compatibility
//   - Enable Scrutor auto-discovery without code changes
//   - All 150 tests continue to work without modification
//
// ADAPTER PATTERN:
//   - Each adapter wraps a generic validator instance
//   - Delegates Validate() calls to wrapped instance
//   - Type-specific: DensityValidatorAdapter wraps DensityValidator<TemplateMetadata, QualityMetrics>
//
// DEPRECATION STRATEGY:
//   - Adapters will be marked [Obsolete] in future version
//   - Consumers should migrate to generic validators directly
//   - Old non-generic validators remain in Validation/ for reference (will be deleted in Phase 9)
//
// AUDIENCE: Graduate / PhD (adapter pattern, backward compatibility, design patterns)
//===============================================================
#nullable enable

using Microsoft.Extensions.Options;
using SymbolLabsForge.Contracts;
using SymbolLabsForge.Validation.Core.Validators;

namespace SymbolLabsForge.Validation
{
    /// <summary>
    /// Backward-compatible adapter for DensityValidator.
    /// Wraps DensityValidator&lt;TemplateMetadata, QualityMetrics&gt; to implement non-generic IValidator.
    /// </summary>
    public class DensityValidatorAdapter : IValidator
    {
        public string Name => _wrappedValidator.Name;

        private readonly DensityValidator<TemplateMetadata, QualityMetrics> _wrappedValidator;

        public DensityValidatorAdapter(IOptions<DensityValidatorSettings> options)
        {
            // Convert DensityValidatorSettings to Validation.Core.DensityValidatorSettings
            var coreOptions = Options.Create(new Core.Validators.DensityValidatorSettings
            {
                MinDensityThreshold = options.Value.MinDensityThreshold,
                MaxDensityThreshold = options.Value.MaxDensityThreshold
            });

            _wrappedValidator = new DensityValidator<TemplateMetadata, QualityMetrics>(coreOptions);
        }

        public ValidationResult Validate(SymbolCapsule? capsule, QualityMetrics metrics)
        {
            // Delegate to wrapped generic validator
            return _wrappedValidator.Validate(capsule, metrics);
        }
    }

    /// <summary>
    /// Backward-compatible adapter for ContrastValidator.
    /// Wraps ContrastValidator&lt;TemplateMetadata, QualityMetrics&gt; to implement non-generic IValidator.
    /// </summary>
    public class ContrastValidatorAdapter : IValidator
    {
        public string Name => _wrappedValidator.Name;

        private readonly ContrastValidator<TemplateMetadata, QualityMetrics> _wrappedValidator;

        public ContrastValidatorAdapter()
        {
            _wrappedValidator = new ContrastValidator<TemplateMetadata, QualityMetrics>();
        }

        public ValidationResult Validate(SymbolCapsule? capsule, QualityMetrics metrics)
        {
            // Delegate to wrapped generic validator
            return _wrappedValidator.Validate(capsule, metrics);
        }
    }

    /// <summary>
    /// Backward-compatible adapter for StructureValidator.
    /// Wraps StructureValidator&lt;TemplateMetadata, QualityMetrics&gt; to implement non-generic IValidator.
    /// </summary>
    public class StructureValidatorAdapter : IValidator
    {
        public string Name => _wrappedValidator.Name;

        private readonly StructureValidator<TemplateMetadata, QualityMetrics> _wrappedValidator;

        public StructureValidatorAdapter()
        {
            _wrappedValidator = new StructureValidator<TemplateMetadata, QualityMetrics>();
        }

        public ValidationResult Validate(SymbolCapsule? capsule, QualityMetrics metrics)
        {
            // Delegate to wrapped generic validator
            return _wrappedValidator.Validate(capsule, metrics);
        }
    }
}
