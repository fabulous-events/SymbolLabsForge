//===============================================================
// File: BatchComparisonOptions.cs
// Author: Claude (Phase 10.7 - Batch Enhancements)
// Date: 2025-11-15
// Purpose: Configuration options for batch comparison service.
//
// PHASE 10.7: CONFIGURABLE BATCH COMPARISON
//   - Max concurrent comparisons (default: 4)
//   - Per-symbol timeout (default: 30 seconds)
//   - Enable retry logic (default: false)
//   - Max retry attempts (default: 3)
//
// WHY THIS MATTERS:
//   - Students can tune performance vs. memory trade-offs
//   - Demonstrates configuration pattern (IOptions<T>)
//   - Shows timeout strategies for robustness
//   - Teaching moment: performance engineering
//
// TEACHING VALUE:
//   - Graduate: Configuration patterns, timeout strategies
//   - PhD: Performance tuning, resource management
//
// AUDIENCE: Graduate / PhD (performance optimization)
//===============================================================
#nullable enable

namespace SymbolLabsForge.UI.Web.Services
{
    /// <summary>
    /// Configuration options for batch comparison operations.
    /// </summary>
    /// <remarks>
    /// <para><b>Phase 10.7: Configurable Batch Comparison</b></para>
    /// <para>These options control performance vs. resource trade-offs:</para>
    /// <list type="bullet">
    /// <item>MaxConcurrentComparisons: Higher = faster, but more memory</item>
    /// <item>PerSymbolTimeout: Prevents infinite hangs</item>
    /// <item>EnableRetry: Handles transient errors</item>
    /// </list>
    /// </remarks>
    public class BatchComparisonOptions
    {
        /// <summary>
        /// Maximum concurrent symbol comparisons.
        /// </summary>
        /// <remarks>
        /// <para><b>Teaching Moment (Graduate):</b></para>
        /// <para>Default: 4 concurrent comparisons</para>
        /// <para>Range: 1-8</para>
        /// <para>Trade-off: Higher concurrency = faster processing but more memory usage.</para>
        /// <para>Each comparison uses ~50 MB RAM, so 4 concurrent = ~200 MB peak.</para>
        /// </remarks>
        public int MaxConcurrentComparisons { get; set; } = 4;

        /// <summary>
        /// Timeout for each individual symbol comparison.
        /// </summary>
        /// <remarks>
        /// <para><b>Teaching Moment (Graduate):</b></para>
        /// <para>Default: 30 seconds per symbol</para>
        /// <para>Prevents one slow/hanging comparison from blocking entire batch.</para>
        /// <para>Adjust based on symbol complexity (larger images = longer timeout).</para>
        /// </remarks>
        public TimeSpan PerSymbolTimeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Enable retry logic for failed comparisons.
        /// </summary>
        /// <remarks>
        /// <para><b>Teaching Moment (PhD):</b></para>
        /// <para>Retry handles transient errors (network issues, file I/O failures).</para>
        /// <para>Uses exponential backoff: 1s, 2s, 4s delays between attempts.</para>
        /// </remarks>
        public bool EnableRetry { get; set; } = false;

        /// <summary>
        /// Maximum number of retry attempts for failed comparisons.
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 3;
    }
}
