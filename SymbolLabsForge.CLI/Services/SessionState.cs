using System;
using System.Text.Json.Serialization;

namespace SymbolLabsForge.CLI.Services
{
    /// <summary>
    /// Represents the persistent state of the Gemini CLI session.
    /// </summary>
    public class SessionState
    {
        /// <summary>
        /// The name of the active AI model.
        /// </summary>
        public string? ActiveModel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether sandbox mode is enabled.
        /// Defaults to false (off).
        /// </summary>
        [JsonPropertyName("sandbox")]
        public bool SandboxMode { get; set; } = false;
    }
}
