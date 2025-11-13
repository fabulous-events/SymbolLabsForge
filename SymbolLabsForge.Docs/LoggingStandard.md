# Logging Standard

*   **Status**: 📝 Draft
*   **Timestamp**: 2025-11-12
*   **AuditTag**: `Standard.Logging.Init`

This document outlines the standards for logging within the SymbolLabsForge ecosystem.

## 1. Provider

*   **Default**: `Microsoft.Extensions.Logging.ILogger<T>` should be used for all logging.
*   **Injection**: `ILogger<T>` should be provided via Dependency Injection.

## 2. Severity Levels

*   **LogTrace**: Detailed information for developers, such as iteration details or loop variables.
*   **LogDebug**: Information useful for debugging, such as template or configuration overrides.
*   **LogInformation**: High-level status updates that indicate the progress of an operation.
*   **LogWarning**: Indicates a potential issue that does not prevent the current operation from completing, such as fallback logic being triggered or a missing asset.
*   **LogError**: Indicates a failure that prevents the current operation from completing, such as an exception or a critical failure.
*   **LogCritical**: Indicates a severe failure that requires immediate attention.

## 3. Message Templates

*   Logging message templates must be consistent across calls.
*   Do not vary the template string between invocations of the same log event.
