//===============================================================
// File: SymbolGenerationService.cs
// Author: Claude (Phase 10.1 - Blazor Server Scaffolding)
// Date: 2025-11-15
// Purpose: Service layer for canonical symbol generation and caching.
//
// PHASE 10.1: SYMBOL GENERATION SERVICE
//   - Manages canonical symbol generation via SharpGenerator, FlatGenerator, etc.
//   - Implements memory caching for performance
//   - Provides validation and error handling for generation requests
//   - Supports both comparison workflow and synthetic generator UI
//
// WHY THIS MATTERS:
//   - Pre-generates canonical symbols for fast comparison
//   - Demonstrates caching strategies and performance optimization
//   - Students learn DI patterns with multiple generator dependencies
//   - Enables synthetic symbol generation from Phase 10.6
//
// TEACHING VALUE:
//   - Undergraduate: Caching patterns, parameterized generation
//   - Graduate: DI lifetime management, cache invalidation strategies
//   - PhD: Adaptive parameter recommendation, ML-based optimization
//
// AUDIENCE: Undergraduate / Graduate (caching, service architecture)
//===============================================================
#nullable enable

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SymbolLabsForge.Generation;  // Correct namespace for most generators
using SymbolLabsForge.Generators;   // FlatGenerator outlier (TODO: fix in core library)
using SymbolLabsForge.Contracts;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace SymbolLabsForge.UI.Web.Services
{
    /// <summary>
    /// Service for generating and caching canonical symbols.
    /// </summary>
    /// <remarks>
    /// <para><b>Phase 10.1: Canonical Symbol Management</b></para>
    /// <para>This service provides two workflows:</para>
    /// <list type="number">
    /// <item><b>Comparison Workflow:</b> Pre-generates canonical symbols, caches in memory</item>
    /// <item><b>Synthetic Generation Workflow:</b> Generates custom symbols with user-specified parameters</item>
    /// </list>
    ///
    /// <para><b>Caching Strategy:</b></para>
    /// <para>Canonical symbols are cached indefinitely (no expiration) since they don't change.
    /// Synthetic symbols are NOT cached (user-specific, parameter-dependent).</para>
    /// </remarks>
    public class SymbolGenerationService
    {
        private readonly SharpGenerator _sharpGenerator;
        private readonly FlatGenerator _flatGenerator;
        private readonly NaturalGenerator _naturalGenerator;
        private readonly DoubleSharpGenerator _doubleSharpGenerator;
        private readonly TrebleGenerator _trebleGenerator;
        private readonly IMemoryCache _cache;
        private readonly ILogger<SymbolGenerationService> _logger;

        // Default canonical sizes (from research plan Phase 10.6)
        private readonly Size _sharpSize = new Size(20, 50);
        private readonly Size _flatSize = new Size(12, 30);
        private readonly Size _naturalSize = new Size(20, 50);
        private readonly Size _doubleSharpSize = new Size(25, 55);
        private readonly Size _trebleSize = new Size(180, 450);

        public SymbolGenerationService(
            SharpGenerator sharpGenerator,
            FlatGenerator flatGenerator,
            NaturalGenerator naturalGenerator,
            DoubleSharpGenerator doubleSharpGenerator,
            TrebleGenerator trebleGenerator,
            IMemoryCache cache,
            ILogger<SymbolGenerationService> logger)
        {
            _sharpGenerator = sharpGenerator;
            _flatGenerator = flatGenerator;
            _naturalGenerator = naturalGenerator;
            _doubleSharpGenerator = doubleSharpGenerator;
            _trebleGenerator = trebleGenerator;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Gets canonical symbol for comparison workflow (cached).
        /// </summary>
        /// <param name="symbolType">Type of symbol to generate.</param>
        /// <returns>Canonical symbol image.</returns>
        public Task<Image<L8>> GetCanonicalSymbolAsync(SymbolType symbolType)
        {
            string cacheKey = $"canonical_{symbolType}";

            // Try to get from cache
            if (_cache.TryGetValue(cacheKey, out Image<L8>? cachedImage) && cachedImage != null)
            {
                _logger.LogDebug("Canonical symbol cache hit: {SymbolType}", symbolType);
                return Task.FromResult(cachedImage.Clone()); // Clone to prevent mutation
            }

            _logger.LogInformation("Generating canonical symbol: {SymbolType}", symbolType);

            // Generate canonical symbol using ISymbolGenerator.GenerateRawImage(Size, int? seed)
            Size size = GetDefaultSize(symbolType);
            int? seed = null; // Deterministic generation (no seed variation)

            Image<L8> canonicalImage = symbolType switch
            {
                SymbolType.Sharp => _sharpGenerator.GenerateRawImage(size, seed),
                SymbolType.Flat => _flatGenerator.GenerateRawImage(size, seed),
                SymbolType.Natural => _naturalGenerator.GenerateRawImage(size, seed),
                SymbolType.DoubleSharp => _doubleSharpGenerator.GenerateRawImage(size, seed),
                SymbolType.Treble => _trebleGenerator.GenerateRawImage(size, seed),
                _ => throw new ArgumentException($"Unknown symbol type: {symbolType}")
            };

            // Cache indefinitely (canonical symbols don't change)
            _cache.Set(cacheKey, canonicalImage, new MemoryCacheEntryOptions
            {
                Priority = CacheItemPriority.NeverRemove
            });

            _logger.LogInformation("Canonical symbol cached: {SymbolType}, size: {Size}",
                symbolType, $"{canonicalImage.Width}×{canonicalImage.Height}");

            return Task.FromResult(canonicalImage.Clone()); // Clone to prevent mutation
        }

        /// <summary>
        /// Generates synthetic symbol with custom parameters (NOT cached).
        /// </summary>
        /// <param name="request">Symbol generation request with custom parameters.</param>
        /// <returns>Validation result with generated image or error message.</returns>
        public ValidationResult<Image<L8>> GenerateSymbol(SymbolGenerationRequest request)
        {
            // Validate request
            var validation = ValidateRequest(request);
            if (!validation.IsValid)
            {
                return ValidationResult<Image<L8>>.Failure(validation.ErrorMessage!);
            }

            try
            {
                _logger.LogInformation("Generating synthetic symbol: {SymbolType}, size: {Size}",
                    request.SymbolType, $"{request.Width}×{request.Height}");

                // Create size and seed for generation
                Size size = new Size(request.Width, request.Height);
                int? seed = null; // Deterministic generation (future: support seed variation)

                // Generate symbol using ISymbolGenerator.GenerateRawImage(Size, int? seed)
                Image<L8> generatedImage = request.SymbolType switch
                {
                    SymbolType.Sharp => _sharpGenerator.GenerateRawImage(size, seed),
                    SymbolType.Flat => _flatGenerator.GenerateRawImage(size, seed),
                    SymbolType.Natural => _naturalGenerator.GenerateRawImage(size, seed),
                    SymbolType.DoubleSharp => _doubleSharpGenerator.GenerateRawImage(size, seed),
                    SymbolType.Treble => _trebleGenerator.GenerateRawImage(size, seed),
                    _ => throw new ArgumentException($"Unknown symbol type: {request.SymbolType}")
                };

                _logger.LogInformation("Synthetic symbol generated successfully: {SymbolType}, {Size}",
                    request.SymbolType, $"{generatedImage.Width}×{generatedImage.Height}");

                return ValidationResult<Image<L8>>.Success(generatedImage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate symbol: {SymbolType}", request.SymbolType);
                return ValidationResult<Image<L8>>.Failure($"Generation failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates symbol generation request.
        /// </summary>
        /// <remarks>
        /// <para><b>Teaching Value (Phase 10.6):</b></para>
        /// <para>Students learn comprehensive input validation to prevent:</para>
        /// <list type="bullet">
        /// <item>DoS attacks (extreme dimensions)</item>
        /// <item>Invalid parameters (negative dimensions, extreme aspect ratios)</item>
        /// <item>Memory exhaustion (max pixel limit)</item>
        /// </list>
        /// </remarks>
        private ValidationResult ValidateRequest(SymbolGenerationRequest request)
        {
            // Dimension validation
            if (request.Width <= 0 || request.Height <= 0)
            {
                return ValidationResult.Failure(
                    $"Width and height must be greater than 0. Got: {request.Width}×{request.Height}");
            }

            // Max dimensions (DoS prevention)
            const int MaxWidth = 500;
            const int MaxHeight = 1000;

            if (request.Width > MaxWidth || request.Height > MaxHeight)
            {
                return ValidationResult.Failure(
                    $"Size exceeds maximum allowed. Max: {MaxWidth}×{MaxHeight}, Got: {request.Width}×{request.Height}. " +
                    $"This prevents memory exhaustion (~{request.Width * request.Height / 1000} KB).");
            }

            // Aspect ratio sanity check
            double ratio = (double)request.Width / request.Height;
            if (ratio < 0.1 || ratio > 10.0)
            {
                return ValidationResult.Failure(
                    $"Aspect ratio too extreme (must be between 0.1 and 10.0). Got: {ratio:F2}");
            }

            // Max total pixels (memory safety)
            const int MaxPixels = 500 * 1000; // 500,000 pixels (~500 KB)

            if (request.Width * request.Height > MaxPixels)
            {
                return ValidationResult.Failure(
                    $"Image too large ({request.Width * request.Height} pixels). Max: {MaxPixels} pixels.");
            }

            return ValidationResult.Success();
        }

        /// <summary>
        /// Gets default size for canonical symbols.
        /// </summary>
        private Size GetDefaultSize(SymbolType symbolType)
        {
            return symbolType switch
            {
                SymbolType.Sharp => _sharpSize,
                SymbolType.Flat => _flatSize,
                SymbolType.Natural => _naturalSize,
                SymbolType.DoubleSharp => _doubleSharpSize,
                SymbolType.Treble => _trebleSize,
                _ => throw new ArgumentException($"Unknown symbol type: {symbolType}")
            };
        }
    }

    /// <summary>
    /// Request for synthetic symbol generation (Phase 10.6).
    /// </summary>
    public class SymbolGenerationRequest
    {
        public SymbolType SymbolType { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        // Future: Add GenerationOptions (anti-aliasing, stroke thickness, background)
    }

    /// <summary>
    /// Validation result with generic data payload.
    /// </summary>
    public class ValidationResult<T>
    {
        public bool IsValid { get; init; }
        public string? ErrorMessage { get; init; }
        public T? Data { get; init; }

        public static ValidationResult<T> Success(T data) => new() { IsValid = true, Data = data };
        public static ValidationResult<T> Failure(string error) => new() { IsValid = false, ErrorMessage = error };
    }
}
