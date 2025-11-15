namespace SymbolLabsForge.Validation
{
    public class DensityValidatorSettings
    {
        public const string SectionName = "Validation:Density";

        public float MinDensityThreshold { get; set; } = 0.05f; // Default to original value

        public float MaxDensityThreshold { get; set; } = 0.12f; // Default to original value
    }
}
