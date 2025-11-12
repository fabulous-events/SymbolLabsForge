# SymbolLabsForge Audit Matrix

This document maps tests to specific governance requirements, providing a traceable audit trail for quality assurance and compliance.

| Test Name                               | Component   | AuditTag  | SymbolType | Coverage Area         | FallbackApplied | OverrideUsed | Replayable | RegressionLinked | SeedReused | Governance Requirement                               |
|-----------------------------------------|-------------|-----------|------------|-----------------------|-----------------|--------------|------------|------------------|------------|------------------------------------------------------|
| `FlatGenerator_ValidDimensions`         | Generator   | Phase2.6  | Flat       | Image Size            | No              | No           | Yes        | No               | No         | Ensure generators respect requested dimensions.      |
| `FlatGenerator_MatchesVerifiedSnapshot` | Generator   | Phase2.9  | Flat       | Visual Regression     | No              | No           | Yes        | Yes              | Yes        | Ensure generator output is visually deterministic.   |
| `DensityValidator_ThresholdBoundary`    | Validator   | Phase2.7  | Any        | Density Threshold     | No              | No           | Yes        | No               | No         | Validate correctness of pixel density calculations.  |
| `DensityValidator_EdgeCaseImages`       | Validator   | Phase2.8  | Any        | Edge Case Resilience  | No              | No           | Yes        | No               | No         | Ensure validators handle empty/full images.        |
| `StructureValidator_EmptyImage`         | Validator   | Phase2.12 | Any        | Structural Integrity  | No              | No           | Yes        | No               | No         | Ensure symbols have meaningful content.              |
| `CapsuleExporter_MetadataIntegrity`     | Export      | Phase2.10 | Any        | Metadata Schema       | No              | No           | No         | No               | No         | Verify exported metadata is complete and correct.    |
| `CapsuleRegistry_NoDuplicates`          | Registry    | Phase2.7  | Any        | Data Integrity        | No              | No           | No         | No               | No         | Prevent duplicate entries in the capsule registry.   |
| `SymbolForge_EndToEnd_Succeeds`         | Integration | Phase2.7  | All        | Pipeline Integrity    | No              | No           | Yes        | No               | Yes        | Verify the end-to-end generation process succeeds.   |
| `StressTest_100SymbolsInParallel`       | Stress Test | Phase2.11 | Flat       | Stability/Concurrency | No              | No           | No         | No               | No         | Ensure the Forge is stable under high load.          |
| `ErrorSimulation_InvalidDimensions`     | Error Sim   | Phase2.12 | Flat       | Resilience            | Yes             | No           | Yes        | No               | No         | Ensure the Forge handles invalid inputs gracefully.  |
| `Override_WithValidatorOverride`        | Override    | Phase2.17 | Flat       | Governance            | No              | Yes          | Yes        | No               | No         | Ensure validator overrides are logged and respected. |
