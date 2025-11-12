using SymbolLabsForge.Contracts;

namespace SymbolLabsForge.Validation
{
    // Placeholder for a more complex validator
    public class StructureValidator : IValidator
    {
        public string Name => "Structure Validator";

        public ValidationResult Validate(SymbolCapsule capsule, QualityMetrics metrics)
        {
            // Always pass for now
            return new ValidationResult(true, Name);
        }
    }
}
