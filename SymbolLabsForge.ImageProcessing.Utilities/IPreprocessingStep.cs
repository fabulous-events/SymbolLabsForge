//===============================================================
// File: IPreprocessingStep.cs
// Author: Gemini (Original), Claude (Phase 8.8 - Extracted to ImageProcessing.Utilities)
// Date: 2025-11-15
// Purpose: Interface for preprocessing pipeline components.
//
// PHASE 8.8: MODULARIZATION - INTERFACE EXTRACTION
//   - Extracted from SymbolLabsForge.Preprocessing to ImageProcessing.Utilities
//   - Defines contract for all preprocessing steps (binarization, skeletonization, etc.)
//   - Enables composable preprocessing pipelines via Strategy Pattern
//
// WHY THIS MATTERS:
//   - OMR requires multiple preprocessing stages: binarization → skeletonization → morphology
//   - Each stage transforms Image<L8> → Image<L8>
//   - This interface enables composition, testing, and pluggable algorithms
//
// TEACHING VALUE:
//   - Undergraduate: Interface-based design, pipeline pattern
//   - Graduate: Strategy Pattern, Open/Closed Principle (OCP)
//   - PhD: Algorithm comparison frameworks (swap binarization methods, benchmark)
//
// USAGE EXAMPLE:
//   IPreprocessingStep[] pipeline = {
//       new BinarizationProcessor(threshold: 128),
//       new SkeletonizationProcessor(),
//       new MorphologicalCloseProcessor(iterations: 2)
//   };
//
//   Image<L8> processed = rawImage;
//   foreach (var step in pipeline) {
//       processed = step.Process(processed);
//   }
//
// AUDIENCE: Undergraduate / Graduate
//===============================================================
#nullable enable

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SymbolLabsForge.ImageProcessing.Utilities
{
    /// <summary>
    /// Defines a contract for preprocessing pipeline components.
    /// Each step transforms an L8 grayscale image to another L8 grayscale image.
    /// </summary>
    /// <remarks>
    /// <para><b>Design Rationale:</b></para>
    /// <para>OMR preprocessing is inherently sequential: raw → binarized → skeletonized → morphologically processed.
    /// This interface enables composition of these stages into testable, swappable, and narratable pipelines.</para>
    ///
    /// <para><b>Implementation Guidelines:</b></para>
    /// <list type="bullet">
    /// <item>Process() should NOT mutate the input image (create a clone if needed)</item>
    /// <item>Process() should be idempotent (calling twice produces same result)</item>
    /// <item>Process() should validate input dimensions and throw ArgumentException if invalid</item>
    /// </list>
    ///
    /// <para><b>Teaching Note:</b></para>
    /// <para>This is an example of the Strategy Pattern (GoF). Each preprocessing algorithm
    /// (binarization, skeletonization, etc.) is a concrete strategy implementing this interface.
    /// The OMR pipeline (context) composes these strategies without knowing their implementation details.</para>
    /// </remarks>
    public interface IPreprocessingStep
    {
        /// <summary>
        /// Applies this preprocessing step to the input image.
        /// </summary>
        /// <param name="image">The input L8 grayscale image.</param>
        /// <returns>A new L8 grayscale image with the preprocessing applied. Original image is NOT modified.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if image is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if image dimensions are invalid for this step.</exception>
        Image<L8> Process(Image<L8> image);
    }
}
