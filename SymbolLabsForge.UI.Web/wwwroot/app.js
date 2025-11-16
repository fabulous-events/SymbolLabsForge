//===============================================================
// File: app.js
// Author: Claude (Phase 10.5 - PDF Export)
// Date: 2025-11-15
// Purpose: JavaScript interop helpers for SymbolLabsForge Blazor UI.
//
// PHASE 10.5: JAVASCRIPT INTEROP FOR FILE DOWNLOADS
//   - downloadFile() - Triggers browser download for byte arrays
//   - Used by PDF export feature to save reports locally
//
// WHY THIS MATTERS:
//   - Blazor cannot trigger file downloads natively (security)
//   - JavaScript interop bridges this gap
//   - Students learn cross-platform communication patterns
//
// TEACHING VALUE:
//   - Undergraduate: JavaScript interop basics
//   - Graduate: Browser APIs (createObjectURL, download attribute)
//   - PhD: Security considerations in client-side downloads
//
// AUDIENCE: Undergraduate / Graduate (JavaScript interop)
//===============================================================

/**
 * Triggers a browser download for a file from a data URL.
 *
 * @param {string} fileName - Name of file to download (e.g., "report.pdf")
 * @param {string} dataUrl - Base64-encoded data URL (e.g., "data:application/pdf;base64,...")
 *
 * @remarks
 * TEACHING MOMENT (Undergraduate):
 * This function creates a temporary <a> element, sets its href to the data URL,
 * sets the download attribute to the filename, clicks it programmatically,
 * then removes it. This is the standard pattern for client-side file downloads.
 *
 * SECURITY NOTE (Graduate):
 * Data URLs can be large (5-10 MB PDFs). For production, consider:
 * - File size limits (browser may block large data URLs)
 * - Memory usage (base64 encoding increases size by ~33%)
 * - Alternative: server-side file generation with download link
 *
 * WHY NOT Blazor FileDownload?
 * - Blazor Server has no built-in file download API
 * - JavaScript interop is the recommended approach
 * - Future: Blazor may add native file download support
 */
window.downloadFile = function (fileName, dataUrl) {
    // Create temporary anchor element
    const link = document.createElement('a');
    link.href = dataUrl;
    link.download = fileName;

    // Append to body (required for Firefox)
    document.body.appendChild(link);

    // Trigger download
    link.click();

    // Cleanup: remove temporary element
    document.body.removeChild(link);

    console.log(`Downloaded file: ${fileName}`);
};
