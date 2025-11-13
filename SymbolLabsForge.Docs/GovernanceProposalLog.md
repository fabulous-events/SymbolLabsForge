# Governance Proposal Log

*   **Status**: 📝 Draft
*   **Timestamp**: 2025-11-12
*   **AuditTag**: `PostPhase.Governance.Draft`

This document outlines a formal governance proposal based on the analysis of `OverridePatterns.md` and `ValidationTension.md`.

## 1. Proposed Governance Rules

### 1.1. Override Allowances

*   **DensityValidator**: Overrides are permitted for capsules in the "Handwritten" lineage, provided that the contributor includes the rationale "Known sparse handwritten style" in the override request.
*   **ContrastValidator**: Overrides for "Intentional low-contrast variant" are permitted, but will be monitored. If the frequency of these overrides exceeds 50% in a given month, this rule will be revisited.
*   **StructureValidator**: Overrides for "Minimalist symbol design" are not permitted. The validator's threshold will be maintained to ensure clarity and readability.

### 1.2. Fallback Validator Selection

*   When a `DensityValidator` override is triggered, the capsule will be re-validated with a `SparseHandwritingDensityValidator` (to be created) that has a lower density threshold.
*   For all other permitted overrides, no fallback validator will be used. The contributor's decision will be final.

### 1.3. Arbitration Thresholds for Consensus

*   If the confidence scores of the Claude and Vortex AI validators differ by more than `0.2`, the capsule will be automatically flagged for contributor arbitration.
*   If both validators agree that a capsule is invalid, but disagree on the reason, the capsule will be flagged for contributor arbitration.
*   If a contributor overrides an AI validator's decision more than 3 times in a 24-hour period, their override privileges will be temporarily suspended pending a review.

## 2. Next Steps

*   This proposal will be submitted to the governance committee for review and approval.
*   The `SparseHandwritingDensityValidator` needs to be implemented.
*   The validation engine needs to be updated to support the proposed arbitration thresholds and override rules.