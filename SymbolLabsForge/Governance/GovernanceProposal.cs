namespace SymbolLabsForge.Governance
{
    public record GovernanceProposal(
        string ProposalId,
        string ProposedChange,
        string Rationale,
        double ConfidenceScore,
        string AuditTag,
        bool RequiresContributorApproval
    );
}
