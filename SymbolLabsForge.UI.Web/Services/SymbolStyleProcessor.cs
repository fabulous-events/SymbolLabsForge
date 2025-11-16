//===============================================================
// File: SymbolStyleProcessor.cs
// Author: Claude (Phase 10.4 - Priority 2: Advanced Style Options)
// Date: 2025-11-15
// Purpose: Post-processing service for applying style customizations to generated symbols.
//
// PHASE 10.4 PRIORITY 2: SYMBOL STYLE PROCESSOR
//   - Applies stroke thickness adjustments (morphological dilation/erosion)
//   - Applies background color customization (transparent, white, black)
//   - Provides before/after preview support
//   - Validates style parameters to prevent invalid operations
//
// WHY THIS MATTERS:
//   - Demonstrates post-processing pipeline design
//   - Students learn morphological image operations (computer vision)
//   - Shows separation of concerns (generation vs. styling)
//   - Enables parameter space exploration (how styles affect comparison similarity)
//
// TEACHING VALUE:
//   - Undergraduate: Image processing operations, parameter effects
//   - Graduate: Post-processing pipeline architecture, morphological operations
//   - PhD: Parameter space exploration, style transfer effects on similarity metrics
//
// AUDIENCE: Graduate / PhD (Image Processing, Pipeline Architecture)
//===============================================================
#nullable enable

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Microsoft.Extensions.Logging;
using System;

namespace SymbolLabsForge.UI.Web.Services
{
    /// <summary>
    /// Post-processing service for applying style customizations to generated symbols.
    /// </summary>
    /// <remarks>
    /// <para><b>Phase 10.4: Post-Processing Pipeline</b></para>
    /// <para>This service operates AFTER symbol generation, modifying the rendered image
    /// without changing the generator code. This separation of concerns allows:</para>
    /// <list type="bullet">
    /// <item>Generators remain simple (focused on geometry)</item>
    /// <item>Styles can be added/removed without touching generators</item>
    /// <item>Multiple style variations from single generation</item>
    /// </list>
    ///
    /// <para><b>Teaching Value (Graduate):</b></para>
    /// <para>Students learn pipeline architecture patterns:</para>
    /// <list type="number">
    /// <item>Generator produces base image (canonical form)</item>
    /// <item>StyleProcessor applies transformations (customization)</item>
    /// <item>Result combines both (separation of concerns)</item>
    /// </list>
    ///
    /// <para><b>Why Post-Processing?</b></para>
    /// <para>Some style effects (anti-aliasing, gradient fills) require render-time control.
    /// But many effects (stroke thickness, background color) can be applied post-render.
    /// This keeps generators simple while enabling experimentation.</para>
    /// </remarks>
    public class SymbolStyleProcessor
    {
        private readonly ILogger<SymbolStyleProcessor> _logger;

        public SymbolStyleProcessor(ILogger<SymbolStyleProcessor> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Applies style customizations to a symbol image.
        /// </summary>
        /// <param name="source">Source image (will not be modified).</param>
        /// <param name="options">Style options to apply.</param>
        /// <returns>New image with styles applied.</returns>
        /// <exception cref="ArgumentNullException">If source or options is null.</exception>
        /// <remarks>
        /// <para><b>Teaching Value (Graduate):</b></para>
        /// <para>Students learn immutable transformation pattern:</para>
        /// <list type="bullet">
        /// <item>Source image is cloned (not mutated)</item>
        /// <item>Transformations applied to clone</item>
        /// <item>Original preserved for before/after comparison</item>
        /// </list>
        ///
        /// <para><b>Pipeline Order:</b></para>
        /// <list type="number">
        /// <item>Clone source (immutability)</item>
        /// <item>Apply stroke thickness (morphological operations)</item>
        /// <item>Apply background color (if not transparent)</item>
        /// <item>Return styled image</item>
        /// </list>
        /// </remarks>
        public Image<L8> ApplyStyles(Image<L8> source, StyleOptions options)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), "Source image cannot be null.");

            if (options == null)
                throw new ArgumentNullException(nameof(options), "Style options cannot be null.");

            // Validate options
            var validation = options.Validate();
            if (!validation.IsValid)
            {
                throw new ArgumentException($"Invalid style options: {validation.ErrorMessage}", nameof(options));
            }

            _logger.LogInformation("Applying styles: StrokeThickness={StrokeThickness}, Background={Background}",
                options.StrokeThickness, options.BackgroundColor);

            // Clone source (immutable transformation)
            var result = source.Clone();

            try
            {
                // Apply stroke thickness adjustment (morphological operations)
                if (options.StrokeThickness != StyleOptions.DefaultStrokeThickness)
                {
                    ApplyStrokeThickness(result, options.StrokeThickness);
                }

                // Apply background color (if not transparent)
                if (options.BackgroundColor != BackgroundColorOption.Transparent)
                {
                    ApplyBackgroundColor(result, options.BackgroundColor);
                }

                _logger.LogInformation("Styles applied successfully: {Width}×{Height}",
                    result.Width, result.Height);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to apply styles");
                result.Dispose(); // Clean up on error
                throw; // Re-throw for caller to handle
            }
        }

        /// <summary>
        /// Applies stroke thickness adjustment using morphological operations.
        /// </summary>
        /// <param name="image">Image to modify (mutated in-place).</param>
        /// <param name="thickness">Target stroke thickness (1-10).</param>
        /// <remarks>
        /// <para><b>Teaching Value (Graduate/PhD):</b></para>
        /// <para>Students learn morphological image processing:</para>
        ///
        /// <para><b>Dilation (Thickening):</b></para>
        /// <para>Expands foreground pixels by adding pixels to boundaries.
        /// Used when thickness > default (2).</para>
        /// <code>
        /// Structuring element: 3×3 square
        /// Iterations: (thickness - default)
        /// Effect: Strokes become thicker, gaps fill in
        /// </code>
        ///
        /// <para><b>Erosion (Thinning):</b></para>
        /// <para>Shrinks foreground pixels by removing pixels from boundaries.
        /// Used when thickness &lt; default (2).</para>
        /// <code>
        /// Structuring element: 3×3 square
        /// Iterations: (default - thickness)
        /// Effect: Strokes become thinner, may disconnect
        /// </code>
        ///
        /// <para><b>Implementation Note:</b></para>
        /// <para>Since ImageSharp doesn't provide built-in morphological operations,
        /// we implement them manually using pixel-level operations. This demonstrates
        /// the underlying algorithm and has high teaching value.</para>
        ///
        /// <para><b>Research Opportunity (PhD):</b></para>
        /// <para>How does stroke thickness affect comparison similarity?</para>
        /// <list type="bullet">
        /// <item>Hypothesis: Thicker strokes → higher similarity (more overlap)</item>
        /// <item>Experiment: Generate symbols with thickness 1-10, compare each</item>
        /// <item>Metric: Similarity % vs. thickness (expected: positive correlation)</item>
        /// </list>
        /// </remarks>
        private void ApplyStrokeThickness(Image<L8> image, int thickness)
        {
            int delta = thickness - StyleOptions.DefaultStrokeThickness;

            if (delta > 0)
            {
                // Thicken: Apply morphological dilation
                _logger.LogDebug("Applying dilation: {Iterations} iterations", delta);

                for (int i = 0; i < delta; i++)
                {
                    Dilate(image);
                }
            }
            else if (delta < 0)
            {
                // Thin: Apply morphological erosion
                int erosionAmount = -delta;
                _logger.LogDebug("Applying erosion: {Iterations} iterations", erosionAmount);

                for (int i = 0; i < erosionAmount; i++)
                {
                    Erode(image);
                }
            }
            // If delta == 0, no change needed (already at default thickness)
        }

        /// <summary>
        /// Dilates the image by expanding foreground pixels (manual implementation).
        /// </summary>
        /// <param name="image">Image to dilate.</param>
        /// <remarks>
        /// <para><b>Teaching Value (Graduate):</b></para>
        /// <para>Manual dilation algorithm:</para>
        /// <list type="number">
        /// <item>Copy original image (to avoid feedback)</item>
        /// <item>For each pixel, check if it's foreground (value > threshold)</item>
        /// <item>If foreground, set all 8 neighbors to foreground</item>
        /// <item>Result: Strokes expand by 1 pixel in all directions</item>
        /// </list>
        /// </remarks>
        private void Dilate(Image<L8> image)
        {
            const byte ForegroundThreshold = 128; // Pixels > 128 are foreground
            const byte ForegroundValue = 255;     // White foreground

            // Create a copy to avoid feedback during iteration
            var original = image.Clone();

            try
            {
                image.ProcessPixelRows(original, (targetRows, sourceRows) =>
                {
                    for (int y = 0; y < image.Height; y++)
                    {
                        var sourceRow = sourceRows.GetRowSpan(y);
                        var targetRow = targetRows.GetRowSpan(y);

                        for (int x = 0; x < image.Width; x++)
                        {
                            // If current pixel is foreground, dilate to neighbors
                            if (sourceRow[x].PackedValue > ForegroundThreshold)
                            {
                                // Set all 8 neighbors to foreground (3×3 structuring element)
                                for (int dy = -1; dy <= 1; dy++)
                                {
                                    for (int dx = -1; dx <= 1; dx++)
                                    {
                                        int nx = x + dx;
                                        int ny = y + dy;

                                        // Check bounds
                                        if (nx >= 0 && nx < image.Width && ny >= 0 && ny < image.Height)
                                        {
                                            var neighborRow = targetRows.GetRowSpan(ny);
                                            neighborRow[nx] = new L8(ForegroundValue);
                                        }
                                    }
                                }
                            }
                        }
                    }
                });
            }
            finally
            {
                original.Dispose();
            }
        }

        /// <summary>
        /// Erodes the image by shrinking foreground pixels (manual implementation).
        /// </summary>
        /// <param name="image">Image to erode.</param>
        /// <remarks>
        /// <para><b>Teaching Value (Graduate):</b></para>
        /// <para>Manual erosion algorithm:</para>
        /// <list type="number">
        /// <item>Copy original image (to avoid feedback)</item>
        /// <item>For each pixel, check if it's foreground (value > threshold)</item>
        /// <item>If foreground, check all 8 neighbors</item>
        /// <item>If ANY neighbor is background, set current pixel to background</item>
        /// <item>Result: Strokes shrink by 1 pixel in all directions</item>
        /// </list>
        /// </remarks>
        private void Erode(Image<L8> image)
        {
            const byte ForegroundThreshold = 128; // Pixels > 128 are foreground
            const byte BackgroundValue = 0;       // Black background

            // Create a copy to avoid feedback during iteration
            var original = image.Clone();

            try
            {
                image.ProcessPixelRows(original, (targetRows, sourceRows) =>
                {
                    for (int y = 0; y < image.Height; y++)
                    {
                        var sourceRow = sourceRows.GetRowSpan(y);
                        var targetRow = targetRows.GetRowSpan(y);

                        for (int x = 0; x < image.Width; x++)
                        {
                            // If current pixel is foreground, check if any neighbor is background
                            if (sourceRow[x].PackedValue > ForegroundThreshold)
                            {
                                bool hasBackgroundNeighbor = false;

                                // Check all 8 neighbors (3×3 structuring element)
                                for (int dy = -1; dy <= 1 && !hasBackgroundNeighbor; dy++)
                                {
                                    for (int dx = -1; dx <= 1 && !hasBackgroundNeighbor; dx++)
                                    {
                                        if (dx == 0 && dy == 0) continue; // Skip center pixel

                                        int nx = x + dx;
                                        int ny = y + dy;

                                        // Check bounds
                                        if (nx >= 0 && nx < image.Width && ny >= 0 && ny < image.Height)
                                        {
                                            var neighborRow = sourceRows.GetRowSpan(ny);
                                            if (neighborRow[nx].PackedValue <= ForegroundThreshold)
                                            {
                                                hasBackgroundNeighbor = true;
                                            }
                                        }
                                        else
                                        {
                                            // Out of bounds = background
                                            hasBackgroundNeighbor = true;
                                        }
                                    }
                                }

                                // If any neighbor is background, erode this pixel
                                if (hasBackgroundNeighbor)
                                {
                                    targetRow[x] = new L8(BackgroundValue);
                                }
                            }
                        }
                    }
                });
            }
            finally
            {
                original.Dispose();
            }
        }

        /// <summary>
        /// Applies background color to image.
        /// </summary>
        /// <param name="image">Image to modify (mutated in-place).</param>
        /// <param name="background">Background color option.</param>
        /// <remarks>
        /// <para><b>Teaching Value (Undergraduate):</b></para>
        /// <para>Students learn background color effects on visual perception:</para>
        /// <list type="bullet">
        /// <item><b>Transparent:</b> Symbol only (default for generators)</item>
        /// <item><b>White:</b> Light background (good for dark mode previews)</item>
        /// <item><b>Black:</b> Dark background (inverts contrast, highlights gaps)</item>
        /// </list>
        ///
        /// <para><b>Implementation:</b></para>
        /// <para>For grayscale (L8) images, background color is applied by:</para>
        /// <list type="number">
        /// <item>Iterate all pixels</item>
        /// <item>If pixel is transparent (0) → Set to background value</item>
        /// <item>If pixel has content (>0) → Keep original value</item>
        /// </list>
        ///
        /// <para><b>Why This Matters:</b></para>
        /// <para>Background affects visual diff perception. Black background makes
        /// white pixels (differences) more visible in comparison results.</para>
        /// </remarks>
        private void ApplyBackgroundColor(Image<L8> image, BackgroundColorOption background)
        {
            byte bgValue = background switch
            {
                BackgroundColorOption.White => 255, // White
                BackgroundColorOption.Black => 0,   // Black (but need to preserve foreground)
                _ => 0 // Transparent (no change)
            };

            _logger.LogDebug("Applying background color: {Background} (value={Value})",
                background, bgValue);

            if (background == BackgroundColorOption.Black)
            {
                // For black background, need to invert: foreground becomes white on black
                image.Mutate(ctx =>
                {
                    ctx.ProcessPixelRowsAsVector4((span) =>
                    {
                        for (int i = 0; i < span.Length; i++)
                        {
                            // Invert grayscale value
                            span[i] = new System.Numerics.Vector4(
                                1.0f - span[i].X, // Invert
                                1.0f - span[i].X, // Invert
                                1.0f - span[i].X, // Invert
                                1.0f              // Keep alpha
                            );
                        }
                    });
                });
            }
            else if (background == BackgroundColorOption.White)
            {
                // For white background, fill transparent pixels with white
                image.Mutate(ctx =>
                {
                    ctx.BackgroundColor(Color.White);
                });
            }
            // Transparent: no change needed
        }
    }

    /// <summary>
    /// Style options for symbol post-processing.
    /// </summary>
    public class StyleOptions
    {
        /// <summary>
        /// Default stroke thickness (baseline for generators).
        /// </summary>
        public const int DefaultStrokeThickness = 2;

        /// <summary>
        /// Stroke thickness (1-10 pixels).
        /// </summary>
        /// <remarks>
        /// <para>1 = Very thin (erosion)</para>
        /// <para>2 = Default (no change)</para>
        /// <para>10 = Very thick (dilation)</para>
        /// </remarks>
        public int StrokeThickness { get; set; } = DefaultStrokeThickness;

        /// <summary>
        /// Background color option.
        /// </summary>
        public BackgroundColorOption BackgroundColor { get; set; } = BackgroundColorOption.Transparent;

        /// <summary>
        /// Validates style options.
        /// </summary>
        /// <returns>Validation result.</returns>
        public StyleValidationResult Validate()
        {
            // Stroke thickness range check
            if (StrokeThickness < 1 || StrokeThickness > 10)
            {
                return StyleValidationResult.Failure(
                    $"Stroke thickness must be between 1 and 10. Got: {StrokeThickness}");
            }

            // Background color enum check
            if (!Enum.IsDefined(typeof(BackgroundColorOption), BackgroundColor))
            {
                return StyleValidationResult.Failure(
                    $"Invalid background color: {BackgroundColor}");
            }

            return StyleValidationResult.Success();
        }

        /// <summary>
        /// Gets diagnostic string for logging.
        /// </summary>
        public string GetDiagnostics()
        {
            return $"StrokeThickness={StrokeThickness}, BackgroundColor={BackgroundColor}";
        }
    }

    /// <summary>
    /// Background color options for symbol rendering.
    /// </summary>
    public enum BackgroundColorOption
    {
        /// <summary>
        /// Transparent background (default for generators).
        /// </summary>
        Transparent = 0,

        /// <summary>
        /// White background (light mode, good for previews).
        /// </summary>
        White = 1,

        /// <summary>
        /// Black background (dark mode, inverts contrast).
        /// </summary>
        Black = 2
    }

    /// <summary>
    /// Validation result for style options.
    /// </summary>
    public class StyleValidationResult
    {
        public bool IsValid { get; init; }
        public string? ErrorMessage { get; init; }

        public static StyleValidationResult Success() => new() { IsValid = true };
        public static StyleValidationResult Failure(string error) => new() { IsValid = false, ErrorMessage = error };
    }
}
