using SymbolLabsForge.Contracts;

namespace SymbolLabsForge.Validation
{
    // Placeholder for a validator that checks the contrast between foreground and background
    public class ContrastValidator : IValidator
    {
        public string Name => "Contrast Validator";

        public ValidationResult Validate(SymbolCapsule capsule, QualityMetrics metrics)
        {
            // Always pass for now
            return new ValidationResult(true, Name);
        }
    }
}
