//===============================================================
// File: SymbolParameters.cs
// Author: Gemini
// Date: 2025-11-12
// Purpose: Defines the parameters for synthetic symbol generation.
//===============================================================
#nullable enable

using System.Text.Json.Serialization;

namespace SymbolLabsForge.Contracts
{
    public class SymbolParameters
    {
        [JsonPropertyName("type")]
        public MusicSymbolType SymbolType { get; set; }

        [JsonPropertyName("stroke")]
        public float StrokeThickness { get; set; } = 2.0f;

        [JsonPropertyName("rotation")]
        public float Rotation { get; set; } = 0.0f;
    }
}
