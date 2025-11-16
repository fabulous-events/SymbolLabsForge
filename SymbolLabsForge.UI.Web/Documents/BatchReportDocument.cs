//===============================================================
// File: BatchReportDocument.cs
// Author: Claude (Phase 10.5 - Batch PDF Export)
// Date: 2025-11-15
// Purpose: QuestPDF document for batch comparison report (multi-page).
//
// PHASE 10.5: BATCH REPORT DOCUMENT
//   - Multi-page PDF for batch comparison results
//   - Summary page with aggregate statistics
//   - Individual pages for each symbol comparison
//   - Comparison table for quick overview
//
// WHY THIS MATTERS:
//   - Students can submit batch comparison reports for assignments
//   - Instructors review multiple symbols in one document
//   - Demonstrates multi-page PDF composition
//   - Shows data aggregation and visualization
//
// TEACHING VALUE:
//   - Undergraduate: Multi-page documents, table generation
//   - Graduate: Document composition patterns, data aggregation
//   - PhD: Report design for research submissions
//
// AUDIENCE: Graduate / PhD (document composition, data visualization)
//===============================================================
#nullable enable

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SymbolLabsForge.UI.Web.Services;

namespace SymbolLabsForge.UI.Web.Documents
{
    /// <summary>
    /// QuestPDF document for batch symbol comparison report (multi-page).
    /// </summary>
    /// <remarks>
    /// <para><b>Phase 10.5: Multi-Page Batch Report</b></para>
    /// <para>This document generates a consolidated PDF with:</para>
    /// <list type="number">
    /// <item>Page 1: Summary page (title, batch statistics, comparison table)</item>
    /// <item>Page 2-N: Individual comparison pages (one per symbol)</item>
    /// </list>
    /// <para><b>Teaching Moment:</b> Multi-page document composition with QuestPDF.</para>
    /// </remarks>
    public class BatchReportDocument : IDocument
    {
        private readonly BatchComparisonResult[] _results;
        private readonly BatchSummaryStatistics _summary;
        private readonly double _tolerance;
        private readonly Dictionary<int, byte[]> _diffImages;
        private readonly DateTime _generatedAt;

        public BatchReportDocument(
            BatchComparisonResult[] results,
            BatchSummaryStatistics summary,
            double tolerance,
            Dictionary<int, byte[]> diffImages)
        {
            _results = results;
            _summary = summary;
            _tolerance = tolerance;
            _diffImages = diffImages;
            _generatedAt = DateTime.UtcNow;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                // Page setup: Letter size, portrait orientation
                page.Size(PageSizes.Letter);
                page.Margin(2); // 2 cm margin
                page.DefaultTextStyle(x => x.FontSize(11));

                // Header (appears on all pages)
                page.Header().Element(ComposeHeader);

                // Content: Summary page + individual comparison pages
                page.Content().Column(column =>
                {
                    column.Spacing(10);

                    // Page 1: Summary page
                    column.Item().Element(ComposeSummaryPage);

                    // Page 2-N: Individual comparison pages (page break before each)
                    foreach (var result in _results)
                    {
                        column.Item().PageBreak();
                        column.Item().Element(container => ComposeIndividualComparison(container, result));
                    }
                });

                // Footer (appears on all pages)
                page.Footer().AlignCenter().Text(text =>
                {
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                    text.Span($" — Generated: {_generatedAt:yyyy-MM-dd HH:mm:ss UTC}");
                });
            });
        }

        /// <summary>
        /// Composes the header (appears on all pages).
        /// </summary>
        private void ComposeHeader(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Text("SymbolLabsForge")
                    .FontSize(20)
                    .Bold()
                    .FontColor(Colors.Blue.Darken3);

                column.Item().Text("Batch Comparison Report")
                    .FontSize(16)
                    .SemiBold();

                column.Item().PaddingTop(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten1);
            });
        }

        /// <summary>
        /// Composes the summary page (Page 1).
        /// </summary>
        private void ComposeSummaryPage(IContainer container)
        {
            container.Column(column =>
            {
                column.Spacing(15);

                // Section 1: Batch metadata
                column.Item().Element(ComposeBatchMetadata);

                // Section 2: Summary statistics
                column.Item().Element(ComposeSummaryStatistics);

                // Section 3: Comparison table (quick overview)
                column.Item().Element(ComposeComparisonTable);
            });
        }

        /// <summary>
        /// Composes batch metadata section.
        /// </summary>
        private void ComposeBatchMetadata(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Text("Batch Metadata").FontSize(14).SemiBold();

                column.Item().PaddingLeft(10).Column(innerColumn =>
                {
                    innerColumn.Item().Text(text =>
                    {
                        text.Span("Total Symbols: ").SemiBold();
                        text.Span(_results.Length.ToString());
                    });

                    innerColumn.Item().Text(text =>
                    {
                        text.Span("Tolerance: ").SemiBold();
                        text.Span($"{_tolerance:P2}");
                    });

                    innerColumn.Item().Text(text =>
                    {
                        text.Span("Comparison Date: ").SemiBold();
                        text.Span(_generatedAt.ToString("yyyy-MM-dd HH:mm:ss UTC"));
                    });
                });
            });
        }

        /// <summary>
        /// Composes summary statistics section.
        /// </summary>
        private void ComposeSummaryStatistics(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Text("Summary Statistics").FontSize(14).SemiBold();

                column.Item().PaddingLeft(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3); // Metric name
                        columns.RelativeColumn(2); // Value
                    });

                    // Header row
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5)
                            .Text("Metric").SemiBold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5)
                            .Text("Value").SemiBold();
                    });

                    // Data rows
                    table.Cell().Padding(5).Text("Total Symbols");
                    table.Cell().Padding(5).Text(_summary.TotalCount.ToString());

                    table.Cell().Padding(5).Text("Pass Count");
                    table.Cell().Padding(5).Text($"{_summary.PassCount} ({_summary.PassRate:F1}%)")
                        .FontColor(Colors.Green.Darken2);

                    table.Cell().Padding(5).Text("Fail Count");
                    table.Cell().Padding(5).Text($"{_summary.FailCount} ({100 - _summary.PassRate:F1}%)")
                        .FontColor(Colors.Red.Darken2);

                    table.Cell().Padding(5).Text("Average Similarity");
                    table.Cell().Padding(5).Text($"{_summary.AverageSimilarity:F2}%");
                });
            });
        }

        /// <summary>
        /// Composes comparison table (quick overview of all symbols).
        /// </summary>
        private void ComposeComparisonTable(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Text("Comparison Results").FontSize(14).SemiBold();

                column.Item().PaddingLeft(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1); // Index
                        columns.RelativeColumn(2); // Symbol Type
                        columns.RelativeColumn(2); // Similarity
                        columns.RelativeColumn(2); // Result
                    });

                    // Header row
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5)
                            .Text("#").SemiBold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5)
                            .Text("Symbol Type").SemiBold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5)
                            .Text("Similarity").SemiBold();
                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5)
                            .Text("Result").SemiBold();
                    });

                    // Data rows
                    foreach (var result in _results)
                    {
                        table.Cell().Padding(5).Text((result.Index + 1).ToString());
                        table.Cell().Padding(5).Text(result.SymbolType.ToString());
                        table.Cell().Padding(5).Text($"{result.ComparisonResult.SimilarityPercent:F2}%");
                        table.Cell().Padding(5).Text(text =>
                        {
                            if (result.ComparisonResult.Similar)
                            {
                                text.Span("✅ PASS").FontColor(Colors.Green.Darken2);
                            }
                            else
                            {
                                text.Span("❌ FAIL").FontColor(Colors.Red.Darken2);
                            }
                        });
                    }
                });
            });
        }

        /// <summary>
        /// Composes individual comparison page (one per symbol).
        /// </summary>
        private void ComposeIndividualComparison(IContainer container, BatchComparisonResult result)
        {
            container.Column(column =>
            {
                column.Spacing(10);

                // Page title
                column.Item().Text($"Comparison {result.Index + 1}: {result.SymbolType}")
                    .FontSize(14)
                    .SemiBold();

                // Result status
                column.Item().Text(text =>
                {
                    text.Span("Status: ").SemiBold();
                    if (result.ComparisonResult.Similar)
                    {
                        text.Span("✅ PASS").FontColor(Colors.Green.Darken2).Bold();
                    }
                    else
                    {
                        text.Span("❌ FAIL").FontColor(Colors.Red.Darken2).Bold();
                    }
                });

                // Similarity metrics
                column.Item().Text(text =>
                {
                    text.Span("Similarity: ").SemiBold();
                    text.Span($"{result.ComparisonResult.SimilarityPercent:F2}%");
                });

                // Statistics
                if (result.ComparisonResult.Statistics != null)
                {
                    var stats = result.ComparisonResult.Statistics;

                    column.Item().PaddingTop(5).Text("Statistics").SemiBold();

                    column.Item().PaddingLeft(10).Column(innerColumn =>
                    {
                        innerColumn.Item().Text($"Total Pixels: {stats.TotalPixels:N0}");
                        innerColumn.Item().Text($"Different Pixels: {stats.DifferentPixels:N0} ({stats.DifferencePercent:F2}%)");
                        innerColumn.Item().Text($"Max Error: {stats.MaxError} / 255");
                        innerColumn.Item().Text($"Mean Error: {stats.MeanError:F2} / 255");
                    });
                }

                // Visual diff image (if available)
                if (_diffImages.TryGetValue(result.Index, out byte[]? diffImageBytes) && diffImageBytes.Length > 0)
                {
                    column.Item().PaddingTop(10).Text("Visual Diff").SemiBold();
                    column.Item().Image(diffImageBytes).FitWidth();
                    column.Item().PaddingTop(5).Text(
                        "Red pixels show differences, gray pixels match perfectly.")
                        .FontSize(9)
                        .Italic()
                        .FontColor(Colors.Grey.Darken1);
                }
            });
        }
    }
}
