# Validator Fallback Matrix

*   **Status**: ✅ Active
*   **AuditTag**: `Phase9.2`

This document defines the automated fallback actions taken when a validator fails during CI/CD or manual submission.

| Validator           | Failure Condition               | Fallback Action                               | Rationale                                         |
|---------------------|---------------------------------|-----------------------------------------------|---------------------------------------------------|
| `DensityValidator`  | Density < 0.25 (Too Low)        | **Route to Morph**: Use handwritten morph lineage. | Low-density symbols often resemble handwritten ones.      |
| `DensityValidator`  | Density > 0.75 (Too High)       | **Flag for Review**: Tag as `HighDensityReview`.  | High-density failures are often complex visual artifacts. |
| `ClippingValidator` | Edge pixels > 5% threshold      | **Apply Auto-Cropping**: Pad canvas by 10%.     | Minor clipping can be resolved with automated padding.    |
| `StructureValidator`| No connected components         | **Reject**: Tag as `EmptyCanvas`.               | An empty symbol has no value.                     |
