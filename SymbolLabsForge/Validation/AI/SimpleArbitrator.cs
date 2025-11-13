using SymbolLabsForge.Contracts;

namespace SymbolLabsForge.Validation.AI
{
    public class SimpleArbitrator : IAIValidatorArbitrator
    {
        public ValidationResult Arbitrate(SymbolCapsule capsule, ValidationResult claudeResult, ValidationResult vortexResult)
        {
            if (claudeResult.IsValid && vortexResult.IsValid)
            {
                return new ValidationResult(true, "AI Arbitration", "Both AI validators agree on validity.");
            }

            if (claudeResult.IsValid)
            {
                return claudeResult;
            }

            return vortexResult;
        }
    }
}
