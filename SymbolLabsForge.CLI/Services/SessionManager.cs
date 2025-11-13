using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SymbolLabsForge.CLI.Services
{
    /// <summary>
    /// Manages the loading and saving of the CLI session state.
    /// </summary>
    public class SessionManager
    {
        private readonly ILogger<SessionManager> _logger;
        private readonly string _sessionFilePath;
        private static readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

        public SessionState CurrentState { get; private set; }

        public SessionManager(ILogger<SessionManager> logger)
        {
            _logger = logger;
            string geminiDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".gemini");
            _sessionFilePath = Path.Combine(geminiDir, "session.json");
            CurrentState = new SessionState();
        }

        /// <summary>
        /// Loads the session state from the file system.
        /// </summary>
        public async Task LoadSessionAsync()
        {
            _logger.LogTrace("Attempting to load session from {Path}", _sessionFilePath);
            if (!File.Exists(_sessionFilePath))
            {
                _logger.LogDebug("Session file not found. Using default state.");
                return;
            }

            try
            {
                string json = await File.ReadAllTextAsync(_sessionFilePath);
                var state = JsonSerializer.Deserialize<SessionState>(json);
                if (state != null)
                {
                    CurrentState = state;
                    _logger.LogInformation("Session loaded successfully.");
                }
                else
                {
                    _logger.LogWarning("Failed to deserialize session file. Using default state.");
                    CurrentState = new SessionState();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while loading the session file. Using default state.");
                CurrentState = new SessionState();
            }
        }

        /// <summary>
        /// Saves the current session state to the file system using an atomic write.
        /// </summary>
        public async Task SaveSessionAsync()
        {
            _logger.LogTrace("Attempting to save session to {Path}", _sessionFilePath);
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_sessionFilePath)!);

                string json = JsonSerializer.Serialize(CurrentState, _serializerOptions);
                
                // Atomic write with file lock to prevent race conditions
                string tempFile = _sessionFilePath + ".tmp";

                using (var stream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var writer = new StreamWriter(stream))
                {
                    await writer.WriteAsync(json);
                }

                File.Move(tempFile, _sessionFilePath, overwrite: true);

                _logger.LogInformation("Session saved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while saving the session file.");
            }
        }
    }
}
