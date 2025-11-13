//===============================================================
// File: ReplayTraceLog.cs
// Author: Gemini
// Date: 2025-11-12
// Purpose: Defines the data structures for the replay trace log.
//===============================================================
#nullable enable

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SymbolLabsForge.Contracts
{
    public class ReplayTraceLog
    {
        [JsonPropertyName("BundleId")]
        public string BundleId { get; set; } = string.Empty;

        [JsonPropertyName("Timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("Events")]
        public List<ReplayEvent> Events { get; set; } = new();
    }

    public class ReplayEvent
    {
        [JsonPropertyName("CapsuleId")]
        public string CapsuleId { get; set; } = string.Empty;

        [JsonPropertyName("SymbolType")]
        public string SymbolType { get; set; } = string.Empty;

        [JsonPropertyName("ValidatorOutcomes")]
        public List<ValidatorOutcome> ValidatorOutcomes { get; set; } = new();

        [JsonPropertyName("ArbitrationDecision")]
        public ArbitrationDecision ArbitrationDecision { get; set; } = new();
    }

    public class ValidatorOutcome
    {
        [JsonPropertyName("ValidatorName")]
        public string ValidatorName { get; set; } = string.Empty;

        [JsonPropertyName("Outcome")]
        public string Outcome { get; set; } = string.Empty;

        [JsonPropertyName("Confidence")]
        public float Confidence { get; set; }

        [JsonPropertyName("Rationale")]
        public string Rationale { get; set; } = string.Empty;
    }

    public class ArbitrationDecision
    {
        [JsonPropertyName("FinalOutcome")]
        public string FinalOutcome { get; set; } = string.Empty;

        [JsonPropertyName("WinningValidator")]
        public string WinningValidator { get; set; } = string.Empty;

        [JsonPropertyName("Reason")]
        public string Reason { get; set; } = string.Empty;
    }
}
