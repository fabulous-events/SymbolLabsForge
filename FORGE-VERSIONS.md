# SymbolLabsForge - Versioning Protocol

This document outlines the versioning, changelog format, and compatibility matrix for the `SymbolLabsForge` NuGet package.

## 1. Semantic Versioning (SemVer)

The Forge follows [Semantic Versioning 2.0.0](https://semver.org/). Given a version number `MAJOR.MINOR.PATCH`:

*   **MAJOR** version is incremented for incompatible API changes.
    *   *Example*: Renaming a record property in `SymbolLabsForge.Contracts`.
*   **MINOR** version is incremented for adding functionality in a backward-compatible manner.
    *   *Example*: Adding a new `SymbolType` or a new `IValidator`.
*   **PATCH** version is incremented for backward-compatible bug fixes.
    *   *Example*: Fixing a rendering artifact in the `SharpGenerator`.

## 2. Changelog Format

All changes must be documented in `CHANGELOG.md`. Entries should be grouped by `Added`, `Changed`, `Fixed`, `Removed`.

```markdown
## [1.7.0] - 2025-11-10

### Added
- `CapsuleId` and `TemplateHash` to `TemplateMetadata` for replay bundle indexing.
- `GenerationSeed` to `SymbolRequest` for reproducible generation.

### Changed
- `SymbolRequest` now takes `List<Size>` for multi-scale support.
- `DensityStatus` is now a type-safe enum.
```

## 3. Compatibility Matrix

| Forge Version | SymbolLabs Version | .NET Version | Notes |
|---------------|--------------------|--------------|-------|
| `1.7.x`       | `2.0.0`+           | .NET 8.0     | Initial stable release. |
| `1.6.x`       | N/A                | .NET 8.0     | Pre-release, API unstable. |

