using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SymbolLabsForge.Contracts;
using SymbolLabsForge.Preprocessing;
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

            var metadata = new TemplateMetadata
            {
                TemplateName = $"{request.Type}_morph_{request.FromStyle}_to_{request.ToStyle}",
                GeneratedBy = $"SymbolLabsForge v{version}", // Should be dynamic
                MorphLineage = $"{request.Type}:{request.FromStyle} -> {request.Type}:{request.ToStyle}",
                InterpolationFactor = request.InterpolationFactor,
                AuditTag = "Phase5"
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

            var primaryCapsule = GenerateSingleCapsule(request, request.Dimensions.First(), _skeletonizationProcessor, _validators, _logger);

            var variants = new List<SymbolCapsule>();
            foreach (var dim in request.Dimensions.Skip(1))
            {
                variants.Add(GenerateSingleCapsule(request, dim, _skeletonizationProcessor, _validators, _logger));
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
                            edgeCaseImage.Mutate(x => x.Rotate(45));
                            break;
                        case EdgeCaseType.Clipped:
                            edgeCaseImage.Mutate(x => x.Crop(new Rectangle(10, 10, edgeCaseImage.Width - 20, edgeCaseImage.Height - 20)));
                            break;
                        case EdgeCaseType.InkBleed:
                            edgeCaseImage.Mutate(x => x.GaussianBlur(1.5f));
                            break;
                    }

                    variants.Add(CreateCapsuleFromImage(edgeCaseImage, primaryCapsule.Metadata, $"edge_{edgeCase}"));
                }
            }

            return new SymbolCapsuleSet(primaryCapsule, variants);
        }

        private SymbolCapsule CreateCapsuleFromImage(Image<L8> image, TemplateMetadata originalMetadata, string nameSuffix)
        {
            var metrics = new QualityMetrics
            {
                Width = image.Width,
                Height = image.Height,
                AspectRatio = (double)image.Height / image.Width
            };

            using var ms = new MemoryStream();
            image.SaveAsBmp(ms);
            var imageBytes = ms.ToArray();
            var sha = HashUtil.ComputeSha256(imageBytes);

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
            var metadata = new TemplateMetadata
            {
                TemplateName = $"{request.Type}_{dimensions.Width}x{dimensions.Height}",
                GeneratedBy = "SymbolLabsForge",
                GenerationSeed = request.GenerationSeed,
                SymbolType = request.Type
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
            using var ms = new MemoryStream();
            finalImage.SaveAsBmp(ms);
            var imageBytes = ms.ToArray();
            var sha = HashUtil.ComputeSha256(imageBytes);

            metadata = metadata with { TemplateHash = sha };
            metadata = metadata with { CapsuleId = $"{metadata.TemplateName}-{metadata.TemplateHash.Substring(0, 8)}" };
            return new SymbolCapsule(finalImage, metadata, metrics, overallIsValid, validationResults);
        }

        private SymbolCapsule CreateFallbackCapsule(SymbolRequest request, Size dimensions, string failureReason)
        {
            var metadata = new TemplateMetadata { TemplateName = $"{request.Type}-fallback" };
            TemplateValidator.ValidateMetadata(metadata);
            var metrics = new QualityMetrics { Width = dimensions.Width, Height = dimensions.Height };
            var validationResults = new List<ValidationResult> { new ValidationResult(false, "FallbackHandler", failureReason) };
            // Return an empty image for the fallback
            return new SymbolCapsule(new Image<L8>(dimensions.Width, dimensions.Height), metadata, metrics, false, validationResults);
        }
    }
}
