# Asset Path Governance

*   **Status**: ✅ Active
*   **Timestamp**: 2025-11-12
*   **AuditTag**: `Governance.AssetPath`

This document codifies the logic for resolving asset paths for future contributors and AI agents.

## 1. Canonical Pathing Logic

*   **Single Source of Truth**: The `IAssetPathProvider` service is the single source of truth for all asset paths within the SymbolLabs ecosystem.
*   **Dependency Injection**: All components that need to access assets **must** do so by injecting the `IAssetPathProvider` interface. Direct file system access with hardcoded or manually constructed paths is prohibited.
*   **Configuration**: The root asset path is configured at the application's entry point when the `AssetPathProvider` is registered with the dependency injection container.

## 2. Contributor Safety

*   By centralizing path management, we reduce the risk of errors from typos or incorrect path construction.
*   This approach makes the codebase more resilient to changes in the directory structure.

## 3. AI Agent Compliance

*   AI agents **must** use the `IAssetPathProvider` when generating code that accesses assets.
*   Any deviation from this standard will be flagged as a compliance failure in `SessionComplianceLog.md`.
