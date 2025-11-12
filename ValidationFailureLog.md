# SymbolLabsForge - Validation Failure Log

This log tracks synthetic templates that were generated but failed the internal validation pipeline. It serves as an audit trail for quality control and a backlog for fixing generation issues.

*   **Format**: Each entry should include the timestamp, the requested symbol, the validator that failed, and the failure reason.
*   **Consumer**: This log is intended to be written by the consuming application (e.g., the SymbolLabs runner) after receiving a `SymbolCapsuleSet` containing capsules where `IsValid` is `false`.

---

### 2025-11-10 14:30:15 UTC
*   **Capsule ID**: `flat-12x30-skeleton-seed-12345`
*   **Validator**: `DensityValidator`
*   **Failure**: Density was 19.8%, which is above the 12% threshold.
*   **Action**: Awaiting implementation of `SkeletonizationProcessor`.

---
