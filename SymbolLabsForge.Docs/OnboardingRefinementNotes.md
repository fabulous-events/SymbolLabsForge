# Onboarding Refinement Notes

*   **Status**: 📝 Draft
*   **Timestamp**: 2025-11-12
*   **AuditTag**: `PostPhase.Simulation.Review`

This document outlines suggested refinements to the SymbolLabsForge onboarding process based on a review of the `OnboardingSimulationLog.md` and `LaunchChecklist.md`.

## 1. Gaps and Ambiguities

*   **Lack of Contextual Understanding**: The current simulation log only tracks command execution, not user comprehension. It's unclear if the user understands the purpose of the validators or the meaning of their output.
*   **"Happy Path" Bias**: The simulation does not include any scenarios where a validation fails. This is a critical gap, as contributors need to know how to diagnose and address validation failures.
*   **Disconnected Governance**: The link between the validation process and the governance process is not explicitly demonstrated. The simulation shows a user approving a proposal, but not how a validation issue might lead to the creation of such a proposal.

## 2. Proposed Refinements

1.  **Introduce a Failure Scenario**: The onboarding simulation should be updated to include a step where the user is guided to create a capsule that intentionally fails a validator (e.g., by creating a symbol that is too dense or not dense enough).
2.  **Integrate Documentation**: After the validation failure, the simulation should instruct the user to open the `ValidatorRationale.md` file to understand the reason for the failure. This will test the user's ability to use the documentation to solve problems.
3.  **Create a Governance Pathway**: The simulation should include a step where the user creates a governance proposal to adjust the threshold of the validator that failed in the previous step. This will create a clear and practical link between validation and governance.

## 3. Governance Hooks

*   **Validator Failure -> Governance Proposal**: The onboarding process should explicitly mention that repeated or unexpected validator failures are a valid reason to initiate a governance proposal.
*   **Validator Rationale -> Onboarding**: The `ValidatorRationale.md` file should be presented to the user as a primary resource during the onboarding process.
