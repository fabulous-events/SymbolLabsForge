namespace SymbolLabsForge.Analysis
{
    public record LineageEdge(
        string FromCapsuleId,
        string ToCapsuleId,
        string TransitionType, // e.g., "Morph", "Interpolation"
        string AuditTag
    );
}
