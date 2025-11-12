using System.Collections.Generic;
using System.Linq;
using SymbolLabsForge.Contracts;

namespace SymbolLabsForge.Governance
{
    /// <summary>
    /// A simulated engine that proposes governance changes based on capsule data.
    /// </summary>
    public class DefaultGovernanceProposer : IGovernanceProposer
    {
        public GovernanceProposal Propose(IEnumerable<SymbolCapsule> capsules, IEnumerable<ValidationResult> results)
        {
            // SIMULATED LOGIC:
            // In a real implementation, this would analyze trends in the provided data.
            // For now, we will return a hardcoded proposal based on the scenario in the docs.

            var proposalId = $"GP-{DateTime.UtcNow:yyyy-MMdd}";
            var proposedChange = "Increase DensityValidator threshold from 0.25 to 0.28";
            var rationale = "80% of fallback capsules with density 0.26–0.28 passed replay validation.";
            var confidence = 0.91;

            return new GovernanceProposal(
                proposalId,
                proposedChange,
                rationale,
                confidence,
                "Phase10",
                true
            );
        }
    }
}
