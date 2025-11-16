//===============================================================
// File: SessionStore.cs
// Author: Claude (Phase 10.6 - Real-Time Collaboration)
// Date: 2025-11-15
// Purpose: In-memory storage for collaborative comparison sessions.
//
// PHASE 10.6: SESSION MANAGEMENT
//   - Thread-safe session storage (ConcurrentDictionary)
//   - Session lifecycle (create, join, leave, cleanup)
//   - Automatic cleanup of expired sessions (24-hour timeout)
//
// WHY THIS MATTERS:
//   - Students learn in-memory distributed state management
//   - Demonstrates thread-safe collections for concurrent access
//   - Shows session lifecycle patterns
//
// TEACHING VALUE:
//   - Graduate: ConcurrentDictionary, thread safety
//   - PhD: Memory management, distributed state patterns
//
// AUDIENCE: Graduate / PhD (concurrent programming, distributed systems)
//===============================================================
#nullable enable

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace SymbolLabsForge.UI.Web.Hubs
{
    /// <summary>
    /// Thread-safe in-memory storage for collaborative comparison sessions.
    /// </summary>
    /// <remarks>
    /// <para><b>Phase 10.6: Session Management</b></para>
    /// <para>Manages session lifecycle:</para>
    /// <list type="number">
    /// <item>CreateSession: Generate unique session ID, add to store</item>
    /// <item>GetSession: Retrieve session by ID</item>
    /// <item>AddParticipant: Add user to session participants list</item>
    /// <item>RemoveParticipant: Remove user from session when disconnected</item>
    /// <item>CleanupExpiredSessions: Remove sessions older than 24 hours</item>
    /// </list>
    /// <para><b>Design Pattern:</b> Singleton service (shared across all SignalR connections).</para>
    /// </remarks>
    public class SessionStore
    {
        private readonly ConcurrentDictionary<string, ComparisonSession> _sessions = new();
        private readonly ILogger<SessionStore> _logger;

        public SessionStore(ILogger<SessionStore> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Creates a new collaborative session.
        /// </summary>
        /// <param name="createdBy">Display name of user creating session.</param>
        /// <returns>Newly created session with unique session ID.</returns>
        /// <remarks>
        /// <para><b>Teaching Moment (Graduate):</b></para>
        /// <para>ConcurrentDictionary.TryAdd is thread-safe (no race conditions).</para>
        /// <para>GUID.ToString("N") generates 32-character hex string (no hyphens) for clean URLs.</para>
        /// </remarks>
        public ComparisonSession CreateSession(string createdBy)
        {
            var session = new ComparisonSession
            {
                CreatedBy = createdBy
            };

            if (_sessions.TryAdd(session.SessionId, session))
            {
                _logger.LogInformation(
                    "Session created: SessionId={SessionId}, CreatedBy={CreatedBy}",
                    session.SessionId, createdBy);

                return session;
            }

            // Extremely unlikely: GUID collision (1 in 2^128)
            throw new InvalidOperationException("Failed to create session (GUID collision)");
        }

        /// <summary>
        /// Retrieves session by session ID.
        /// </summary>
        /// <param name="sessionId">Session ID (GUID string).</param>
        /// <returns>Session if found, null otherwise.</returns>
        public ComparisonSession? GetSession(string sessionId)
        {
            _sessions.TryGetValue(sessionId, out var session);
            return session;
        }

        /// <summary>
        /// Adds participant to session.
        /// </summary>
        /// <param name="sessionId">Session ID.</param>
        /// <param name="participant">Participant to add.</param>
        /// <returns>True if participant added, false if session not found.</returns>
        /// <remarks>
        /// <para><b>Teaching Moment (Graduate):</b></para>
        /// <para>Manual locking (lock) required when modifying List inside ConcurrentDictionary.</para>
        /// <para>ConcurrentDictionary only protects dictionary operations, not nested collections.</para>
        /// </remarks>
        public bool AddParticipant(string sessionId, SessionParticipant participant)
        {
            if (_sessions.TryGetValue(sessionId, out var session))
            {
                lock (session.Participants)
                {
                    // Check if participant already exists (by connection ID)
                    if (!session.Participants.Any(p => p.ConnectionId == participant.ConnectionId))
                    {
                        session.Participants.Add(participant);
                        session.LastActivityAt = DateTime.UtcNow;

                        _logger.LogInformation(
                            "Participant joined: SessionId={SessionId}, ConnectionId={ConnectionId}, DisplayName={DisplayName}",
                            sessionId, participant.ConnectionId, participant.DisplayName);

                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Removes participant from session by connection ID.
        /// </summary>
        /// <param name="sessionId">Session ID.</param>
        /// <param name="connectionId">SignalR connection ID.</param>
        /// <returns>True if participant removed, false if not found.</returns>
        public bool RemoveParticipant(string sessionId, string connectionId)
        {
            if (_sessions.TryGetValue(sessionId, out var session))
            {
                lock (session.Participants)
                {
                    var participant = session.Participants.FirstOrDefault(p => p.ConnectionId == connectionId);
                    if (participant != null)
                    {
                        session.Participants.Remove(participant);
                        session.LastActivityAt = DateTime.UtcNow;

                        _logger.LogInformation(
                            "Participant left: SessionId={SessionId}, ConnectionId={ConnectionId}, DisplayName={DisplayName}",
                            sessionId, connectionId, participant.DisplayName);

                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Updates session state (uploaded image, tolerance, result).
        /// </summary>
        /// <param name="sessionId">Session ID.</param>
        /// <param name="updateAction">Action to update session state.</param>
        /// <returns>True if session updated, false if not found.</returns>
        /// <remarks>
        /// <para><b>Teaching Moment (Graduate):</b></para>
        /// <para>Action delegate allows caller to specify state changes without exposing session internals.</para>
        /// </remarks>
        public bool UpdateSessionState(string sessionId, Action<ComparisonSession> updateAction)
        {
            if (_sessions.TryGetValue(sessionId, out var session))
            {
                lock (session)
                {
                    updateAction(session);
                    session.LastActivityAt = DateTime.UtcNow;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Adds annotation to session.
        /// </summary>
        /// <param name="sessionId">Session ID.</param>
        /// <param name="annotation">Annotation to add.</param>
        /// <returns>True if annotation added, false if session not found.</returns>
        public bool AddAnnotation(string sessionId, Annotation annotation)
        {
            if (_sessions.TryGetValue(sessionId, out var session))
            {
                lock (session.Annotations)
                {
                    session.Annotations.Add(annotation);
                    session.LastActivityAt = DateTime.UtcNow;

                    _logger.LogInformation(
                        "Annotation added: SessionId={SessionId}, AnnotationId={AnnotationId}, Type={Type}",
                        sessionId, annotation.AnnotationId, annotation.Type);

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Removes expired sessions (older than specified timeout).
        /// </summary>
        /// <param name="timeout">Session expiration timeout (default: 24 hours).</param>
        /// <returns>Number of sessions removed.</returns>
        /// <remarks>
        /// <para><b>Teaching Moment (Graduate):</b></para>
        /// <para>Background cleanup prevents memory leaks from abandoned sessions.</para>
        /// <para>Run periodically via BackgroundService (e.g., every hour).</para>
        /// </remarks>
        public int CleanupExpiredSessions(TimeSpan? timeout = null)
        {
            var expirationThreshold = timeout ?? TimeSpan.FromHours(24);
            var cutoffTime = DateTime.UtcNow - expirationThreshold;

            var expiredSessionIds = _sessions
                .Where(kvp => kvp.Value.LastActivityAt < cutoffTime)
                .Select(kvp => kvp.Key)
                .ToList();

            int removedCount = 0;
            foreach (var sessionId in expiredSessionIds)
            {
                if (_sessions.TryRemove(sessionId, out var session))
                {
                    _logger.LogInformation(
                        "Session expired: SessionId={SessionId}, LastActivity={LastActivity}, Participants={ParticipantCount}",
                        sessionId, session.LastActivityAt, session.Participants.Count);

                    removedCount++;
                }
            }

            if (removedCount > 0)
            {
                _logger.LogInformation(
                    "Session cleanup complete: Removed={RemovedCount}, Remaining={RemainingCount}",
                    removedCount, _sessions.Count);
            }

            return removedCount;
        }

        /// <summary>
        /// Gets current session count (for diagnostics).
        /// </summary>
        public int GetSessionCount() => _sessions.Count;

        /// <summary>
        /// Gets all active sessions (for admin dashboard, optional).
        /// </summary>
        /// <returns>Read-only list of active sessions.</returns>
        public IReadOnlyList<ComparisonSession> GetAllSessions()
        {
            return _sessions.Values.ToList().AsReadOnly();
        }
    }
}
