using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SymbolLabsForge.Contracts;
using SymbolLabsForge.ImageProcessing.Utilities;
using SymbolLabsForge.Generation;
using System.Reflection;
using SymbolLabsForge.Utils;
using System.IO;
using SymbolLabsForge.Validation;

namespace SymbolLabsForge
{
    internal class SymbolForge : ISymbolForge
    {
        private readonly IEnumerable<ISymbolGenerator> _generators;
        private readonly IEnumerable<IValidator> _validators;
        private readonly IPreprocessingStep _skeletonizationProcessor;
        private readonly IMorphEngine _morphEngine;
        private readonly ILogger<SymbolForge> _logger;

        public SymbolForge(
            IEnumerable<ISymbolGenerator> generators,
            IEnumerable<IValidator> validators,
            IPreprocessingStep skeletonizationProcessor,
            IMorphEngine morphEngine,
            ILogger<SymbolForge> logger)
        {
            _generators = generators;
            _validators = validators;
            _skeletonizationProcessor = skeletonizationProcessor;
            _morphEngine = morphEngine;
            _logger = logger;
        }

        public async Task<SymbolCapsule> MorphAsync(MorphRequest request)
        {
            _logger.LogInformation("Received request to morph {SymbolType} from {From} to {To}.", request.Type, request.FromStyle, request.ToStyle);

            using var morphedImage = await _morphEngine.MorphAsync(request);

            var version = Assembly.GetEntryAssembly()
              ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
              ?.InformationalVersion ?? "0.0.0-local";

            // Compute hash from morphed image for integrity
            var computedHash = CanonicalHashProvider.ComputeSha256(morphedImage);

            var metadata = new TemplateMetadata
            {
                TemplateName = $"{request.Type}_morph_{request.FromStyle}_to_{request.ToStyle}",
                GeneratedBy = $"SymbolLabsForge v{version}", // Should be dynamic
                TemplateHash = computedHash,
                SymbolType = request.Type,
                MorphLineage = $"{request.Type}:{request.FromStyle} -> {request.Type}:{request.ToStyle}",
                InterpolationFactor = request.InterpolationFactor,
                AuditTag = "Phase5",
                Provenance = new ProvenanceMetadata
                {
                    SourceImage = $"{request.FromStyle} + {request.ToStyle}",
                    Method = PreprocessingMethod.Custom,
                    ValidationDate = DateTime.UtcNow,
                    ValidatedBy = $"SymbolLabsForge v{version}",
                    Notes = $"Morphed interpolation (factor: {request.InterpolationFactor})"
                }
            };

            TemplateValidator.ValidateMetadata(metadata);

            var metrics = new QualityMetrics
            {
                Width = morphedImage.Width,
                Height = morphedImage.Height,
                AspectRatio = (double)morphedImage.Height / morphedImage.Width
            };

            // Run validation on the morphed image
            var validationResults = new List<ValidationResult>();
            bool overallIsValid = true;
            var tempCapsule = new SymbolCapsule(morphedImage, metadata, metrics, true, new List<ValidationResult>());

            foreach (var validator in _validators)
            {
                var result = validator.Validate(tempCapsule, metrics);
                validationResults.Add(result);
                if (!result.IsValid) overallIsValid = false;
            }

            return new SymbolCapsule(morphedImage, metadata, metrics, overallIsValid, validationResults);
        }

        public SymbolCapsuleSet Generate(SymbolRequest request)
        {
            _logger.LogInformation("Received request to generate {SymbolType} at {DimensionsCount} dimensions.", request.Type, request.Dimensions.Count);

            var generatedCapsules = new List<SymbolCapsule>();
            try
            {
                var primaryCapsule = GenerateSingleCapsule(request, request.Dimensions.First(), _skeletonizationProcessor, _validators, _logger);
                generatedCapsules.Add(primaryCapsule);

                var variants = new List<SymbolCapsule>();
                foreach (var dim in request.Dimensions.Skip(1))
                {
                    var variantCapsule = GenerateSingleCapsule(request, dim, _skeletonizationProcessor, _validators, _logger);
                    generatedCapsules.Add(variantCapsule);
                    variants.Add(variantCapsule);
                }

                // Handle edge cases
                if (request.EdgeCasesToGenerate != null)
                {
                    foreach (var edgeCase in request.EdgeCasesToGenerate)
                    {
                        _logger.LogInformation("Generating edge case: {EdgeCaseType}", edgeCase);
                        Image<L8> edgeCaseImage = primaryCapsule.TemplateImage.Clone();

                        switch (edgeCase)
                        {
                            case EdgeCaseType.Rotated:
                                edgeCaseImage.Mutate(x => x.Rotate(Constants.DefaultRotation));
                                break;
                            case EdgeCaseType.Clipped:
                                var cropRect = new Rectangle(Constants.DefaultCropX, Constants.DefaultCropY, edgeCaseImage.Width - (Constants.DefaultCropX * 2), edgeCaseImage.Height - (Constants.DefaultCropY * 2));
                                edgeCaseImage.Mutate(x => x.Crop(cropRect));
                                break;
                            case EdgeCaseType.InkBleed:
                                edgeCaseImage.Mutate(x => x.GaussianBlur(Constants.DefaultGaussianBlur));
                                break;
                        }
                        var edgeCaseCapsule = CreateCapsuleFromImage(edgeCaseImage, primaryCapsule.Metadata, $"edge_{edgeCase}");
                        generatedCapsules.Add(edgeCaseCapsule);
                        variants.Add(edgeCaseCapsule);
                    }
                }

                return new SymbolCapsuleSet(primaryCapsule, variants);
            }
            catch (Exception)
            {
                // If anything goes wrong, dispose of any capsules we've created to prevent leaks.
                foreach (var capsule in generatedCapsules)
                {
                    capsule.Dispose();
                }
                throw;
            }
        }

        private SymbolCapsule CreateCapsuleFromImage(Image<L8> image, TemplateMetadata originalMetadata, string nameSuffix)
        {
            var metrics = new QualityMetrics
            {
                Width = image.Width,
                Height = image.Height,
                AspectRatio = (double)image.Height / image.Width
            };

            var sha = CanonicalHashProvider.ComputeSha256(image);

            var metadata = originalMetadata with
            {
                TemplateName = $"{originalMetadata.TemplateName}_{nameSuffix}",
                TemplateHash = sha,
                CapsuleId = $"{originalMetadata.TemplateName}_{nameSuffix}-{sha.Substring(0, 8)}"
            };

            return new SymbolCapsule(image, metadata, metrics, true, new List<ValidationResult>());
        }


        private SymbolCapsule GenerateSingleCapsule(
            SymbolRequest request,
            Size dimensions,
            IPreprocessingStep skeletonizationProcessor,
            IEnumerable<IValidator> validators,
            ILogger<SymbolForge> logger)
        {
            var generator = _generators.FirstOrDefault(g => g.SupportedType == request.Type);
            if (generator == null)
            {
                // FALLBACK LOGIC: Return an invalid capsule instead of throwing an exception
                logger.LogError("No generator found for symbol type {SymbolType}. Applying fallback.", request.Type);
                return CreateFallbackCapsule(request, dimensions, "Generator not found.");
            }

            // 1. Generate Raw Image
            using var rawImage = generator.GenerateRawImage(dimensions, request.GenerationSeed);
            logger.LogDebug("Generated raw image for {SymbolType} at {Dimensions}.", request.Type, dimensions);

            // ... (processing steps remain the same)
            Image<L8> binarizedImage = rawImage.CloneAs<L8>();
            Image<L8> finalImage = request.OutputForms.Contains(OutputForm.Skeletonized)
                ? skeletonizationProcessor.Process(binarizedImage.Clone())
                : binarizedImage;

            // 4. Create Metadata and Metrics
            var version = Assembly.GetEntryAssembly()
              ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
              ?.InformationalVersion ?? "0.0.0-local";

            var metadata = new TemplateMetadata
            {
                TemplateName = $"{request.Type}_{dimensions.Width}x{dimensions.Height}",
                GeneratedBy = $"SymbolLabsForge v{version}",
                TemplateHash = "pending-computation", // Will be updated after hash computation
                GenerationSeed = request.GenerationSeed,
                SymbolType = request.Type,
                Provenance = new ProvenanceMetadata
                {
                    SourceImage = "synthetic-generation",
                    Method = request.OutputForms.Contains(OutputForm.Skeletonized) ? PreprocessingMethod.Skeletonized : PreprocessingMethod.Binarized,
                    ValidationDate = DateTime.UtcNow,
                    ValidatedBy = $"SymbolLabsForge v{version}",
                    Notes = $"Synthetically generated {request.Type} symbol"
                }
            };
            TemplateValidator.ValidateMetadata(metadata);
            var metrics = new QualityMetrics
            {
                Width = dimensions.Width,
                Height = dimensions.Height,
                AspectRatio = (double)dimensions.Height / dimensions.Width
            };

            // 5. Validate Capsule
            var validationResults = new List<ValidationResult>();
            bool overallIsValid = true;

            var tempCapsule = new SymbolCapsule(finalImage, metadata, metrics, true, new List<ValidationResult>());

            foreach (var validator in validators)
            {
                if (request.ValidatorOverrides?.TryGetValue(validator.Name, out var @override) == true && @override.Overridden)
                {
                    // OVERRIDE LOGIC
                    logger.LogWarning("Validator '{ValidatorName}' was overridden. Reason: {Reason}", validator.Name, @override.Reason);
                    validationResults.Add(new ValidationResult(true, validator.Name, $"Overridden: {@override.Reason}"));
                    // Note: We do not set overallIsValid to false for an override
                }
                else
                {
                    var result = validator.Validate(tempCapsule, metrics);
                    validationResults.Add(result);
                    if (!result.IsValid)
                    {
                        overallIsValid = false;
                    }
                }
            }

            // ... (hash calculation and return)
            var sha = CanonicalHashProvider.ComputeSha256(finalImage);

            metadata = metadata with { TemplateHash = sha };
            metadata = metadata with { CapsuleId = $"{metadata.TemplateName}-{metadata.TemplateHash.Substring(0, 8)}" };
            return new SymbolCapsule(finalImage, metadata, metrics, overallIsValid, validationResults);
        }

        private SymbolCapsule CreateFallbackCapsule(SymbolRequest request, Size dimensions, string failureReason)
        {
            var version = Assembly.GetEntryAssembly()
              ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
              ?.InformationalVersion ?? "0.0.0-local";

            var metadata = new TemplateMetadata
            {
                TemplateName = $"{request.Type}-fallback",
                GeneratedBy = $"SymbolLabsForge v{version}",
                TemplateHash = "fallback-no-hash",
                SymbolType = request.Type,
                Provenance = new ProvenanceMetadata
                {
                    SourceImage = "fallback-generation",
                    Method = PreprocessingMethod.Raw,
                    ValidationDate = DateTime.UtcNow,
                    ValidatedBy = $"SymbolLabsForge v{version}",
                    Notes = $"Fallback capsule due to generation failure: {failureReason}"
                }
            };
            TemplateValidator.ValidateMetadata(metadata);
            var metrics = new QualityMetrics { Width = dimensions.Width, Height = dimensions.Height };
            var validationResults = new List<ValidationResult> { new ValidationResult(false, "FallbackHandler", failureReason) };
            // Return an empty image for the fallback
            return new SymbolCapsule(new Image<L8>(dimensions.Width, dimensions.Height), metadata, metrics, false, validationResults);
        }
    }
}
