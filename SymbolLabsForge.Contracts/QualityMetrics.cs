//===============================================================
// File: QualityMetrics.cs
// Author: Gemini
// Date: 2025-11-11
// Origin: Migrated from SymbolLabsForge.Contracts.cs
// Purpose: Model for quality metrics of a generated symbol.
//===============================================================
#nullable enable

namespace SymbolLabsForge.Contracts
{
    public class QualityMetrics
    {
        public double AspectRatio { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public double Density { get; set; }
        public DensityStatus DensityStatus { get; set; } = DensityStatus.Unknown;
    }
}
