# Ancestry Trigger Rules

*   **Status**: 📝 Draft
*   **Timestamp**: 2025-11-12
*   **AuditTag**: `PostPhase.Lineage.Rules`

This document proposes a set of lineage-triggered validator rules.

## 1. Proposed Rules

1.  **Stricter Validation for Morphed Capsules**: When a capsule is created by morphing two or more parent capsules, it should be subjected to a stricter set of validation rules. For example, the `DensityValidator` could have a narrower range of acceptable values (e.g., 7% to 10% instead of 5% to 12%).
2.  **Inherited Validation History**: A new capsule should inherit the validation history of its parents. If a parent capsule has a history of validation failures, the child capsule should be automatically flagged for manual review, even if it passes all automated validations.
3.  **Lineage-Specific Validators**: Certain validators should only be applied to capsules of a specific lineage. For example, a hypothetical "HandwritingClarityValidator" could be applied to all capsules in the "Handwritten" lineage to ensure that they are legible.

## 2. Implementation Notes

*   These rules will require modifications to the validation engine to allow for dynamic adjustment of validator parameters based on a capsule's lineage.
*   The `SymbolCapsule` contract will need to be extended to include a reference to the capsule's parent(s).
