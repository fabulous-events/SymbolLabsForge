namespace SymbolLabsForge.Validation.AI
{
    public record ArbitrationResult(
        bool RequiresContributorReview,
        bool OverrideSuggested,
        string SuggestedAction,
        string Reason,
        double ClaudeConfidence,
        double VortexConfidence,
        string AuditTag,
        string ContributorDecision = "Pending"
    );
}
