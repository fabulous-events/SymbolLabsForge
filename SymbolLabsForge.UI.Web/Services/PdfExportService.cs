//===============================================================
// File: PdfExportService.cs
// Author: Claude (Phase 10.5 - PDF Export)
// Date: 2025-11-15
// Purpose: Service for exporting comparison results to PDF.
//
// PHASE 10.5: PDF EXPORT SERVICE
//   - Generates PDF reports for single comparisons
//   - Generates consolidated batch reports
//   - Uses QuestPDF for modern PDF generation
//   - Loads diff images from file system
//
// WHY THIS MATTERS:
//   - Students can save comparison results for assignments
//   - Instructors receive formatted reports for grading
//   - Demonstrates service layer pattern for document generation
//   - Shows how to integrate third-party libraries (QuestPDF)
//
// TEACHING VALUE:
//   - Undergraduate: Service pattern, file I/O, PDF generation basics
//   - Graduate: Document composition, error handling, resource management
//   - PhD: Report design for academic submissions, batch processing
//
// AUDIENCE: Undergraduate / Graduate (service architecture)
//===============================================================
#nullable enable

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using SymbolLabsForge.UI.Web.Documents;
using Microsoft.Extensions.Logging;

namespace SymbolLabsForge.UI.Web.Services
{
    /// <summary>
    /// Service for exporting comparison results to PDF format.
    /// </summary>
    /// <remarks>
    /// <para><b>Phase 10.5: PDF Export Service</b></para>
    /// <para>This service generates professional PDF reports using QuestPDF:</para>
    /// <list type="number">
    /// <item>Load diff image from file system</item>
    /// <item>Create QuestPDF document (ComparisonReportDocument)</item>
    /// <item>Generate PDF byte array</item>
    /// <item>Return for download or storage</item>
    /// </list>
    /// <para><b>Design Pattern:</b> Singleton service (stateless, no per-request state)</para>
    /// </remarks>
    public class PdfExportService
    {
        private readonly ILogger<PdfExportService> _logger;
        private readonly IWebHostEnvironment _environment;

        public PdfExportService(
            ILogger<PdfExportService> logger,
            IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;

            // Configure QuestPDF license (Community license for non-commercial use)
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        }

        /// <summary>
        /// Exports a single comparison result to PDF.
        /// </summary>
        /// <param name="result">Comparison result to export.</param>
        /// <param name="symbolType">Symbol type that was compared.</param>
        /// <param name="tolerance">Tolerance used in comparison.</param>
        /// <returns>PDF file as byte array, ready for download.</returns>
        /// <exception cref="InvalidOperationException">If diff image file not found.</exception>
        public async Task<byte[]> ExportSingleComparisonAsync(
            ComparisonResult result,
            SymbolType symbolType,
            double tolerance)
        {
            _logger.LogInformation("Exporting single comparison to PDF: {SymbolType}", symbolType);

            try
            {
                // Step 1: Load diff image from file system
                byte[] diffImageBytes = await LoadDiffImageAsync(result.DiffImagePath);

                // Step 2: Create QuestPDF document
                var document = new ComparisonReportDocument(
                    result,
                    symbolType,
                    tolerance,
                    diffImageBytes);

                // Step 3: Generate PDF byte array
                byte[] pdfBytes = document.GeneratePdf();

                _logger.LogInformation("PDF generated successfully: {Size} bytes", pdfBytes.Length);

                return pdfBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export comparison to PDF: {SymbolType}", symbolType);
                throw;
            }
        }

        /// <summary>
        /// Exports multiple comparison results to a consolidated batch report.
        /// </summary>
        /// <param name="batchResults">Array of batch comparison results.</param>
        /// <param name="summary">Summary statistics for batch.</param>
        /// <param name="tolerance">Tolerance used in comparisons.</param>
        /// <returns>PDF file as byte array, ready for download.</returns>
        /// <remarks>
        /// <para><b>Teaching Moment:</b> Batch reports aggregate multiple results into single document.</para>
        /// <para>This demonstrates document composition and multi-page PDF generation.</para>
        /// </remarks>
        public async Task<byte[]> ExportBatchReportAsync(
            BatchComparisonResult[] batchResults,
            BatchSummaryStatistics summary,
            double tolerance)
        {
            _logger.LogInformation("Exporting batch report to PDF: {Count} comparisons", batchResults.Length);

            try
            {
                // Step 1: Load all diff images into dictionary (index → image bytes)
                var diffImages = new Dictionary<int, byte[]>();

                foreach (var result in batchResults)
                {
                    if (!string.IsNullOrEmpty(result.ComparisonResult.DiffImagePath))
                    {
                        try
                        {
                            byte[] imageBytes = await LoadDiffImageAsync(result.ComparisonResult.DiffImagePath);
                            diffImages[result.Index] = imageBytes;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to load diff image for symbol {Index}, skipping", result.Index);
                            diffImages[result.Index] = Array.Empty<byte>();
                        }
                    }
                    else
                    {
                        // For failed comparisons with no diff image, use empty array
                        diffImages[result.Index] = Array.Empty<byte>();
                    }
                }

                // Step 2: Create QuestPDF batch document
                var document = new BatchReportDocument(
                    batchResults,
                    summary,
                    tolerance,
                    diffImages);

                // Step 3: Generate PDF byte array
                byte[] pdfBytes = document.GeneratePdf();

                _logger.LogInformation("Batch PDF generated successfully: {Size} bytes", pdfBytes.Length);

                return pdfBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export batch report to PDF");
                throw;
            }
        }

        /// <summary>
        /// Loads diff image from file system.
        /// </summary>
        /// <param name="diffImagePath">Relative path from wwwroot (e.g., "/diffs/diff_Sharp_20251115120000.png").</param>
        /// <returns>Image bytes (PNG format).</returns>
        /// <exception cref="FileNotFoundException">If diff image file not found.</exception>
        private async Task<byte[]> LoadDiffImageAsync(string? diffImagePath)
        {
            if (string.IsNullOrEmpty(diffImagePath))
            {
                throw new ArgumentException("Diff image path is null or empty", nameof(diffImagePath));
            }

            // Convert relative path to absolute file system path
            // Example: "/diffs/diff_Sharp_20251115120000.png" → "/mnt/e/.../wwwroot/diffs/diff_Sharp_20251115120000.png"
            string relativePath = diffImagePath.TrimStart('/');
            string absolutePath = Path.Combine(_environment.WebRootPath, relativePath);

            if (!File.Exists(absolutePath))
            {
                throw new FileNotFoundException($"Diff image not found: {absolutePath}");
            }

            _logger.LogDebug("Loading diff image from: {Path}", absolutePath);

            return await File.ReadAllBytesAsync(absolutePath);
        }
    }
}
