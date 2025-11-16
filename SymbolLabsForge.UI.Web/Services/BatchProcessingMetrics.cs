//===============================================================
// File: BatchProcessingMetrics.cs
// Author: Claude (Phase 10.7 Workstream 2 - Timeout & Metrics)
// Date: 2025-11-15
// Purpose: Performance metrics tracking for batch comparison operations.
//
// PHASE 10.7 WORKSTREAM 2: TIMEOUT & METRICS
//   - Track processing time (start → finish)
//   - Calculate throughput (symbols/second)
//   - Average time per symbol
//   - Correlation ID for structured logging
//
// WHY THIS MATTERS:
//   - Students can analyze batch performance
//   - Demonstrates observability patterns (metrics, logging)
//   - Shows how to identify bottlenecks
//   - Real-world pattern for production monitoring
//
// TEACHING VALUE:
//   - Graduate: Performance metrics, observability
//   - PhD: Performance analysis, bottleneck identification
//
// AUDIENCE: Graduate / PhD (performance analysis, observability)
//===============================================================
#nullable enable

using System.Diagnostics;

namespace SymbolLabsForge.UI.Web.Services
{
    /// <summary>
    /// Performance metrics for batch comparison operations.
    /// </summary>
    /// <remarks>
    /// <para><b>Phase 10.7 Workstream 2: Performance Metrics</b></para>
    /// <para>Tracks timing and throughput metrics for batch symbol comparisons:</para>
    /// <list type="bullet">
    /// <item>Processing time (start → finish)</item>
    /// <item>Throughput (symbols/second)</item>
    /// <item>Average time per symbol</item>
    /// <item>Correlation ID for distributed tracing</item>
    /// </list>
    /// <para><b>Teaching Value:</b> Shows students how to instrument code for observability.</para>
    /// </remarks>
    public class BatchProcessingMetrics
    {
        /// <summary>
        /// Unique correlation ID for this batch operation.
        /// Used for structured logging and distributed tracing.
        /// </summary>
        /// <remarks>
        /// <para><b>Teaching Moment (Graduate):</b></para>
        /// <para>Correlation IDs allow tracing a single operation across multiple log entries.</para>
        /// <para>Format: 32-character hex string (GUID without hyphens).</para>
        /// </remarks>
        public string CorrelationId { get; init; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// Timestamp when batch processing started (UTC).
        /// </summary>
        public DateTime StartTime { get; init; } = DateTime.UtcNow;

        /// <summary>
        /// Timestamp when batch processing finished (UTC).
        /// </summary>
        /// <remarks>
        /// <para><b>Note:</b> Set this property when batch completes (success or failure).</para>
        /// </remarks>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Total processing duration (EndTime - StartTime).
        /// </summary>
        /// <remarks>
        /// <para><b>Teaching Moment (Graduate):</b></para>
        /// <para>Calculated property avoids storing redundant data.</para>
        /// <para>Returns TimeSpan.Zero if batch not yet complete (EndTime is null).</para>
        /// </remarks>
        public TimeSpan Duration => EndTime.HasValue
            ? EndTime.Value - StartTime
            : TimeSpan.Zero;

        /// <summary>
        /// Total number of symbols in batch.
        /// </summary>
        public int TotalSymbols { get; init; }

        /// <summary>
        /// Number of symbols successfully compared.
        /// </summary>
        public int CompletedSymbols { get; set; }

        /// <summary>
        /// Number of symbols that failed comparison.
        /// </summary>
        public int FailedSymbols { get; set; }

        /// <summary>
        /// Number of symbols that timed out.
        /// </summary>
        /// <remarks>
        /// <para><b>Phase 10.7 Workstream 2:</b> Per-symbol timeout tracking.</para>
        /// </remarks>
        public int TimedOutSymbols { get; set; }

        /// <summary>
        /// Number of symbols cancelled by user.
        /// </summary>
        public int CancelledSymbols { get; set; }

        /// <summary>
        /// Throughput in symbols per second (TotalSymbols / Duration.TotalSeconds).
        /// </summary>
        /// <remarks>
        /// <para><b>Teaching Moment (Graduate):</b></para>
        /// <para>Throughput measures processing rate.</para>
        /// <para>Returns 0.0 if Duration is zero (prevents division by zero).</para>
        /// </remarks>
        public double ThroughputSymbolsPerSecond
        {
            get
            {
                if (Duration.TotalSeconds <= 0)
                    return 0.0;

                return TotalSymbols / Duration.TotalSeconds;
            }
        }

        /// <summary>
        /// Average time per symbol (Duration / TotalSymbols).
        /// </summary>
        /// <remarks>
        /// <para><b>Teaching Moment (Graduate):</b></para>
        /// <para>Average latency helps identify slow symbols (outliers).</para>
        /// <para>Returns TimeSpan.Zero if TotalSymbols is zero.</para>
        /// </remarks>
        public TimeSpan AverageTimePerSymbol
        {
            get
            {
                if (TotalSymbols <= 0)
                    return TimeSpan.Zero;

                return TimeSpan.FromSeconds(Duration.TotalSeconds / TotalSymbols);
            }
        }

        /// <summary>
        /// Peak memory usage during batch processing (bytes).
        /// </summary>
        /// <remarks>
        /// <para><b>Future Enhancement:</b> Use GC.GetTotalMemory for memory tracking.</para>
        /// <para>Currently optional (null if not tracked).</para>
        /// </remarks>
        public long? PeakMemoryBytes { get; set; }

        /// <summary>
        /// Concurrency level used for this batch.
        /// </summary>
        public int ConcurrencyLevel { get; init; }

        /// <summary>
        /// Creates a formatted summary string for logging.
        /// </summary>
        /// <returns>Human-readable summary of batch metrics.</returns>
        /// <remarks>
        /// <para><b>Teaching Moment (Graduate):</b></para>
        /// <para>Structured string formatting for log messages.</para>
        /// </remarks>
        public string ToSummaryString()
        {
            return $"Batch {CorrelationId.Substring(0, 8)}: " +
                   $"{TotalSymbols} symbols, " +
                   $"{Duration.TotalSeconds:F2}s, " +
                   $"{ThroughputSymbolsPerSecond:F2} symbols/s, " +
                   $"concurrency: {ConcurrencyLevel}, " +
                   $"completed: {CompletedSymbols}, " +
                   $"failed: {FailedSymbols}, " +
                   $"timed out: {TimedOutSymbols}, " +
                   $"cancelled: {CancelledSymbols}";
        }

        /// <summary>
        /// Creates a new Stopwatch for timing individual operations.
        /// </summary>
        /// <returns>Started Stopwatch instance.</returns>
        /// <remarks>
        /// <para><b>Teaching Moment (Graduate):</b></para>
        /// <para>Stopwatch provides high-resolution timing (better than DateTime.Now).</para>
        /// </remarks>
        public static Stopwatch CreateStopwatch()
        {
            var sw = new Stopwatch();
            sw.Start();
            return sw;
        }
    }
}
