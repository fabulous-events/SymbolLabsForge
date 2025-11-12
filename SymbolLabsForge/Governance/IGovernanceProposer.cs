using System.Collections.Generic;
using SymbolLabsForge.Contracts;

namespace SymbolLabsForge.Governance
{
    public interface IGovernanceProposer
    {
        GovernanceProposal Propose(IEnumerable<SymbolCapsule> capsules, IEnumerable<ValidationResult> results);
    }
}
