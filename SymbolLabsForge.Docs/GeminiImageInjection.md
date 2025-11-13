# Gemini /image Command Injection Behavior

*   **Status**: ✅ Active
*   **Timestamp**: 2025-11-12
*   **AuditTag**: `Command.ImageInjection.Behavior`

This document defines the behavior, fallback logic, and governance hooks for the `/image` command.

## 1. Command Behavior

*   **Purpose**: To inject a base64-encoded image from the canonical asset path into a specified markdown file or the console.
*   **Path Resolution**: The command **must** use the `IAssetPathProvider` service to resolve the path to the assets directory. Hardcoded paths are strictly prohibited.
*   **Injection Format**: The output is a markdown-formatted image tag with an inline base64-encoded data URI (e.g., `![file.png](data:image/png;base64,...)`).

## 2. Fallback Logic

*   **Asset Not Found**: If the specified image file does not exist at the canonical asset path, the command will log a "Fallback" message to the console and record the failure in `SessionComplianceLog.md`. The rationale will be "Asset not found".
*   **Malformed Asset**: If the image file exists but cannot be read or encoded, the command will log a "Fallback" message to the console and record the failure in `SessionComplianceLog.md`. The rationale will be "Malformed asset".

## 3. Governance Hooks

*   **Traceability**: Every execution of the `/image` command is logged in `SessionComplianceLog.md`, including the filename, validator context, status (success or fallback), and rationale.
*   **Path Governance**: The canonical asset path is managed by the `AssetPathProvider` and is considered a governance-grade component. Any changes to this path must be reviewed and approved.
