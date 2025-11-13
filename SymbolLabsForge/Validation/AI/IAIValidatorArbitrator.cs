using SymbolLabsForge.Contracts;

namespace SymbolLabsForge.Validation.AI
{
    public interface IAIValidatorArbitrator
    {
        ValidationResult Arbitrate(SymbolCapsule capsule, ValidationResult claudeResult, ValidationResult vortexResult);
    }
}