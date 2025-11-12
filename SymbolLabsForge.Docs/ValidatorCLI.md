# Capsule Validator CLI - Documentation

*   **Status**: In Development (Blocked by Build Failure)
*   **AuditTag**: `Phase3.3`

## Overview
The Capsule Validator CLI is a command-line tool for auditing and re-validating symbol capsules from their `.json` metadata files. It is a critical component for CI pipelines and regression testing.

## Usage

### Validate a Single Capsule
```bash
dotnet run --project SymbolLabsForgeValidator -- validate-capsule <path-to-capsule.json>
```

### Command: `validate-capsule`
Validates the integrity and content of a single capsule file.

#### Arguments
*   `<path-to-capsule.json>` (Required): The file path to the capsule's metadata file.

#### Options
*   `--fix` (Planned): If validation fails, this flag will attempt to regenerate the capsule with the same parameters to create a new, valid version.

## Exit Codes
*   `0`: Validation passed.
*   `1`: Validation failed.
*   `-1`: An unexpected error occurred.

## CI Integration
The validator can be used in a CI pipeline to automatically check the validity of all generated test assets.

```yaml
- name: Validate Test Capsules
  run: |
    find ./TestAssets -name "*.json" -exec dotnet run --project SymbolLabsForgeValidator -- validate-capsule {} \;
```
