using SymbolLabsForge.Contracts;

namespace SymbolLabsForge.Validation.AI
{
    public class DefaultArbitrator : IAIValidatorArbitrator
    {
        public ArbitrationResult Arbitrate(SymbolCapsule capsule, ValidationResult claude, ValidationResult vortex)
        {
            bool agreement = claude.IsValid == vortex.IsValid;
            bool overrideSuggested = !agreement && claude.IsValid; // Suggest override if Claude passes and Vortex fails
            string reason = agreement
                ? "Claude and Vortex agree on capsule validity."
                : "Claude suggests override; Vortex rejects due to strict threshold.";

            return new ArbitrationResult(
                RequiresContributorReview: !agreement,
                OverrideSuggested: overrideSuggested,
                SuggestedAction: overrideSuggested ? "Override" : "Reject",
                Reason: reason,
                ClaudeConfidence: 0.85, // Placeholder confidence
                VortexConfidence: 0.92, // Placeholder confidence
                AuditTag: "Phase7"
            );
        }
    }
}
