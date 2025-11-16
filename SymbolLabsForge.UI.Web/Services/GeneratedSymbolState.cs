//===============================================================
// File: GeneratedSymbolState.cs
// Author: Claude (Phase 10.4 - State Transfer Implementation)
// Date: 2025-11-15
// Purpose: Scoped service for transferring generated symbols between components.
//
// PHASE 10.4: STATE TRANSFER SERVICE
//   - Manages state transfer from Generator to Comparison
//   - Scoped lifetime ensures state persists during request
//   - One-time consumption pattern (state cleared after use)
//   - Validation and error handling for robust state management
//
// WHY THIS MATTERS:
//   - Eliminates download/re-upload friction (generate → download → re-upload)
//   - Demonstrates scoped service state management patterns
//   - Students learn cross-component communication in Blazor
//   - Shows one-time use state machine design
//
// TEACHING VALUE:
//   - Undergraduate: Cross-component state transfer, service lifetime
//   - Graduate: Scoped service memory management, state consumption patterns
//   - PhD: State machine design, automatic cleanup, memory safety
//
// AUDIENCE: Graduate / PhD (State Management, Service Architecture)
//===============================================================
#nullable enable

using System;

namespace SymbolLabsForge.UI.Web.Services
{
    /// <summary>
    /// Scoped service for transferring generated symbol state from Generator to Comparison.
    /// </summary>
    /// <remarks>
    /// <para><b>Phase 10.4: State Transfer Pattern</b></para>
    /// <para>This service implements a one-time consumption pattern:</para>
    /// <list type="number">
    /// <item>Generator creates symbol and stores in this service</item>
    /// <item>Generator navigates to Comparison page</item>
    /// <item>Comparison reads state from service (HasData = true)</item>
    /// <item>Comparison consumes state (Clear() called automatically)</item>
    /// <item>State is now empty (HasData = false)</item>
    /// </list>
    ///
    /// <para><b>Why Scoped Lifetime?</b></para>
    /// <para>Scoped services are created once per client connection in Blazor Server.
    /// This ensures state persists across navigation within the same browser session,
    /// but is isolated between different users.</para>
    ///
    /// <para><b>Teaching Value (Graduate):</b></para>
    /// <para>Students learn DI lifetime management:</para>
    /// <list type="bullet">
    /// <item><b>Transient:</b> New instance per injection (no state sharing)</item>
    /// <item><b>Scoped:</b> One instance per request (state persists during navigation) ✅ Used here</item>
    /// <item><b>Singleton:</b> One instance for all users (dangerous for user-specific state)</item>
    /// </list>
    /// </remarks>
    public class GeneratedSymbolState
    {
        /// <summary>
        /// Raw image data (PNG bytes).
        /// </summary>
        public byte[]? ImageData { get; private set; }

        /// <summary>
        /// Symbol type (Sharp, Flat, Natural, DoubleSharp, Treble).
        /// </summary>
        public string? SymbolType { get; private set; }

        /// <summary>
        /// Image width in pixels.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Image height in pixels.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Timestamp when symbol was generated.
        /// </summary>
        public DateTime? GeneratedAt { get; private set; }

        /// <summary>
        /// Indicates whether the state contains valid data.
        /// </summary>
        /// <remarks>
        /// <para><b>Teaching Value (Undergraduate):</b></para>
        /// <para>Shows defensive programming pattern - check HasData before consuming state.</para>
        /// <para>Prevents null reference exceptions by explicitly validating all required fields.</para>
        /// </remarks>
        public bool HasData =>
            ImageData != null &&
            !string.IsNullOrEmpty(SymbolType) &&
            Width > 0 &&
            Height > 0;

        /// <summary>
        /// Stores generated symbol data for transfer to Comparison page.
        /// </summary>
        /// <param name="imageData">Raw PNG bytes.</param>
        /// <param name="symbolType">Symbol type (Sharp, Flat, Natural, DoubleSharp, Treble).</param>
        /// <param name="width">Image width in pixels.</param>
        /// <param name="height">Image height in pixels.</param>
        /// <exception cref="ArgumentNullException">If imageData or symbolType is null/empty.</exception>
        /// <exception cref="ArgumentException">If width or height is <= 0.</exception>
        /// <remarks>
        /// <para><b>Teaching Value (Graduate):</b></para>
        /// <para>Demonstrates comprehensive input validation:</para>
        /// <list type="bullet">
        /// <item>Null checks with descriptive error messages</item>
        /// <item>Range validation (width, height must be positive)</item>
        /// <item>Early failure (fail-fast principle)</item>
        /// </list>
        /// </remarks>
        public void SetData(byte[] imageData, string symbolType, int width, int height)
        {
            // Validation: Null checks
            if (imageData == null || imageData.Length == 0)
            {
                throw new ArgumentNullException(nameof(imageData), "Image data cannot be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(symbolType))
            {
                throw new ArgumentNullException(nameof(symbolType), "Symbol type cannot be null or empty.");
            }

            // Validation: Range checks
            if (width <= 0)
            {
                throw new ArgumentException($"Width must be greater than 0. Got: {width}", nameof(width));
            }

            if (height <= 0)
            {
                throw new ArgumentException($"Height must be greater than 0. Got: {height}", nameof(height));
            }

            // Store state
            ImageData = imageData;
            SymbolType = symbolType;
            Width = width;
            Height = height;
            GeneratedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Clears all state (one-time consumption pattern).
        /// </summary>
        /// <remarks>
        /// <para><b>Teaching Value (PhD):</b></para>
        /// <para>Demonstrates state machine design:</para>
        /// <list type="bullet">
        /// <item><b>Initial State:</b> HasData = false (no data)</item>
        /// <item><b>Populated State:</b> HasData = true (after SetData)</item>
        /// <item><b>Consumed State:</b> HasData = false (after Clear)</item>
        /// </list>
        ///
        /// <para>This prevents accidental state reuse:</para>
        /// <para>If user navigates Comparison → Generator → Comparison again,
        /// the second Comparison visit won't see stale data from first visit.</para>
        ///
        /// <para><b>Memory Safety:</b></para>
        /// <para>Setting ImageData = null allows GC to reclaim byte array memory.
        /// Important for large images (Treble Clef 180×450 = ~81 KB).</para>
        /// </remarks>
        public void Clear()
        {
            ImageData = null;
            SymbolType = null;
            Width = 0;
            Height = 0;
            GeneratedAt = null;
        }

        /// <summary>
        /// Gets diagnostic information about current state (for debugging/logging).
        /// </summary>
        /// <returns>Human-readable state summary.</returns>
        /// <remarks>
        /// <para><b>Teaching Value (Graduate):</b></para>
        /// <para>Shows debugging-friendly design - state can be inspected without side effects.</para>
        /// </remarks>
        public string GetDiagnostics()
        {
            if (!HasData)
            {
                return "GeneratedSymbolState: Empty (no data)";
            }

            return $"GeneratedSymbolState: {SymbolType} ({Width}×{Height}), {ImageData!.Length} bytes, generated at {GeneratedAt:yyyy-MM-dd HH:mm:ss} UTC";
        }
    }
}
