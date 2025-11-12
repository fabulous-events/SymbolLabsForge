using SymbolLabsForge.Contracts;

namespace SymbolLabsForge.Validation.AI
{
    public interface IAIValidatorArbitrator
    {
        ArbitrationResult Arbitrate(SymbolCapsule capsule, ValidationResult claudeResult, ValidationResult vortexResult);
    }
}
