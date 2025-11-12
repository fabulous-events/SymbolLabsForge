# Capsule Provenance Ledger

*   **Status**: ✅ Active & Immutable
*   **AuditTag**: `Phase14.1`

This document serves as the immutable, human-readable counterpart to the `CapsuleLedger.db`. It provides a high-level audit trail for the lifecycle of every significant capsule.

| Capsule ID                 | Event             | Contributor              | Signature                                | Timestamp           |
|----------------------------|-------------------|--------------------------|------------------------------------------|---------------------|
| `flat-asset-20251111120000`  | `CREATE`          | `michael@symbollabs.org` | `sig-abc...`                             | 2025-11-11 12:05:00 |
| `flat-asset-20251111120000`  | `VALIDATE`        | `CI@symbollabs.org`      | `sig-def...`                             | 2025-11-11 12:05:05 |
| `clef_14x28_morph_0.6`     | `MORPH`           | `dev@forge.ai`           | `sig-ghi...`                             | 2025-11-11 12:15:23 |
| `clef_14x28_morph_0.6`     | `VALIDATE_OVERRIDE` | `michael@symbollabs.org` | `sig-jkl...`                             | 2025-11-11 12:16:00 |
