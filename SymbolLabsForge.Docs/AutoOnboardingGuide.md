# Auto-Generated Onboarding Guide (v2)

*   **Status**: ✅ Active & Auto-Generated
*   **Based on Contributor Behavior up to**: 2025-11-11
*   **AuditTag**: `Phase10.3`

Welcome to SymbolLabsForge! This guide has been automatically generated based on the most common and successful workflows of our contributors.

## 🔥 Hot Path: Creating a Valid `Flat` Symbol
Analysis of recent submissions shows that the most common task is creating a standard `Flat` symbol. Here is the most efficient workflow:

**1. Generate a Standard Asset:**
Most contributors start by generating a baseline asset. This is the recommended first step.
```bash
forge generate-test-assets --symbol Flat
```

**2. (Optional) Apply a Morph:**
If you are creating a stylistic variant, the `Handwritten` to `Typeset` morph is the most common and successful path.
```bash
forge morph --from Flat_Handwritten --to Flat_Typeset --factor 0.5
```
**Heads up!** Morphing from `Handwritten` may trigger the `DensityValidator`. Our data shows that contributors often **override** this with the rationale "Known sparse handwritten style."

**3. Validate Your Capsule:**
Before submitting, always run the validator.
```bash
forge validate-capsule ./path/to/your/capsule.json
```
This workflow has a **98%** success rate for generating valid capsules on the first try.
