//===============================================================
// File: ComparisonHub.cs
// Author: Claude (Phase 10.6 - Real-Time Collaboration)
// Date: 2025-11-15
// Purpose: SignalR hub for real-time collaborative comparison sessions.
//
// PHASE 10.6: SIGNALR HUB IMPLEMENTATION
//   - WebSocket-based real-time communication
//   - Session creation and participant management
//   - Real-time state synchronization (uploads, tolerance, results)
//   - Annotation broadcasting for instructor feedback
//
// WHY THIS MATTERS:
//   - Students learn SignalR hub architecture (server → client RPC)
//   - Demonstrates WebSocket vs. HTTP long-polling
//   - Shows connection lifecycle management
//   - Real-world pattern for multi-user applications
//
// TEACHING VALUE:
//   - Graduate: SignalR hub methods, WebSocket communication
//   - PhD: Distributed systems, eventual consistency, network partitions
//
// AUDIENCE: Graduate / PhD (distributed systems, real-time collaboration)
//===============================================================
#nullable enable

using Microsoft.AspNetCore.SignalR;
using SymbolLabsForge.UI.Web.Services;

namespace SymbolLabsForge.UI.Web.Hubs
{
    /// <summary>
    /// SignalR hub for real-time collaborative comparison sessions.
    /// </summary>
    /// <remarks>
    /// <para><b>Phase 10.6: Real-Time Collaboration</b></para>
    /// <para>Hub methods are invoked by clients (JavaScript HubConnection.invoke).</para>
    /// <para>Hub broadcasts state changes to all clients in a session via Clients.Group.</para>
    /// <para><b>Teaching Moment (Graduate):</b></para>
    /// <para>SignalR provides server → client RPC over WebSocket (or fallback to HTTP long-polling).</para>
    /// <para>Groups allow broadcasting to subsets of connections (e.g., all participants in a session).</para>
    /// </remarks>
    public class ComparisonHub : Hub
    {
        private readonly SessionStore _sessionStore;
        private readonly ILogger<ComparisonHub> _logger;

        /// <summary>
        /// Initializes a new instance of the ComparisonHub class.
        /// </summary>
        /// <param name="sessionStore">Session storage service (singleton).</param>
        /// <param name="logger">Logger for diagnostic output.</param>
        public ComparisonHub(SessionStore sessionStore, ILogger<ComparisonHub> logger)
        {
            _sessionStore = sessionStore;
            _logger = logger;
        }

        //==============================================================
        // SESSION LIFECYCLE METHODS
        //==============================================================

        /// <summary>
        /// Creates a new collaborative session.
        /// </summary>
        /// <param name="displayName">Display name of user creating session.</param>
        /// <param name="role">Role of creator (Instructor, Student, Viewer).</param>
        /// <returns>Session ID for shareable URL.</returns>
        /// <remarks>
        /// <para><b>Teaching Moment (Graduate):</b></para>
        /// <para>Server generates unique session ID (GUID), creates session in SessionStore,
        /// and adds creator as first participant.</para>
        /// <para>Client receives session ID and can share URL: /collaboration/{sessionId}</para>
        /// </remarks>
        public async Task<string> CreateSession(string displayName, ParticipantRole role = ParticipantRole.Instructor)
        {
            var session = _sessionStore.CreateSession(displayName);

            // Add creator as first participant
            var participant = new SessionParticipant
            {
                ConnectionId = Context.ConnectionId,
                DisplayName = displayName,
                Role = role
            };
            _sessionStore.AddParticipant(session.SessionId, participant);

            // Join SignalR group for broadcasting
            await Groups.AddToGroupAsync(Context.ConnectionId, session.SessionId);

            _logger.LogInformation(
                "Session created: SessionId={SessionId}, Creator={DisplayName}, ConnectionId={ConnectionId}",
                session.SessionId, displayName, Context.ConnectionId);

            return session.SessionId;
        }

        /// <summary>
        /// Joins an existing collaborative session.
        /// </summary>
        /// <param name="sessionId">Session ID to join.</param>
        /// <param name="displayName">Display name of joining user.</param>
        /// <param name="role">Role of joining user.</param>
        /// <returns>True if joined successfully, false if session not found.</returns>
        /// <remarks>
        /// <para><b>Teaching Moment (Graduate):</b></para>
        /// <para>SignalR Groups allow broadcasting to all connections in a session.</para>
        /// <para>When user joins, server broadcasts "ParticipantJoined" to notify all existing participants.</para>
        /// </remarks>
        public async Task<bool> JoinSession(string sessionId, string displayName, ParticipantRole role = ParticipantRole.Student)
        {
            var session = _sessionStore.GetSession(sessionId);
            if (session == null)
            {
                _logger.LogWarning("Join failed: SessionId={SessionId} not found", sessionId);
                return false;
            }

            var participant = new SessionParticipant
            {
                ConnectionId = Context.ConnectionId,
                DisplayName = displayName,
                Role = role
            };

            if (_sessionStore.AddParticipant(sessionId, participant))
            {
                // Join SignalR group
                await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);

                // Notify all participants in session
                await Clients.Group(sessionId).SendAsync("ParticipantJoined", participant);

                _logger.LogInformation(
                    "Participant joined: SessionId={SessionId}, DisplayName={DisplayName}, ConnectionId={ConnectionId}",
                    sessionId, displayName, Context.ConnectionId);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Leaves a collaborative session.
        /// </summary>
        /// <param name="sessionId">Session ID to leave.</param>
        /// <remarks>
        /// <para><b>Teaching Moment (Graduate):</b></para>
        /// <para>When user leaves, server removes participant from session and broadcasts update.</para>
        /// <para>SignalR automatically handles disconnections via OnDisconnectedAsync.</para>
        /// </remarks>
        public async Task LeaveSession(string sessionId)
        {
            if (_sessionStore.RemoveParticipant(sessionId, Context.ConnectionId))
            {
                // Remove from SignalR group
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionId);

                // Notify remaining participants
                await Clients.Group(sessionId).SendAsync("ParticipantLeft", Context.ConnectionId);

                _logger.LogInformation(
                    "Participant left: SessionId={SessionId}, ConnectionId={ConnectionId}",
                    sessionId, Context.ConnectionId);
            }
        }

        //==============================================================
        // STATE SYNCHRONIZATION METHODS
        //==============================================================

        /// <summary>
        /// Broadcasts uploaded image to all session participants.
        /// </summary>
        /// <param name="sessionId">Session ID.</param>
        /// <param name="imageData">Uploaded image data (byte array).</param>
        /// <param name="symbolType">Selected symbol type.</param>
        /// <remarks>
        /// <para><b>Teaching Moment (Graduate):</b></para>
        /// <para>Large binary data (images) transmitted over WebSocket.</para>
        /// <para>Server updates session state and broadcasts to all participants.</para>
        /// <para><b>Performance Consideration:</b> For large images, consider storing server-side
        /// and broadcasting URL instead of raw bytes.</para>
        /// </remarks>
        public async Task BroadcastUpload(string sessionId, byte[] imageData, SymbolType symbolType)
        {
            var updated = _sessionStore.UpdateSessionState(sessionId, session =>
            {
                session.UploadedImageData = imageData;
                session.SymbolType = symbolType;
            });

            if (updated)
            {
                // Notify all participants
                await Clients.Group(sessionId).SendAsync("ImageUploaded", imageData, symbolType);

                _logger.LogInformation(
                    "Image broadcast: SessionId={SessionId}, SymbolType={SymbolType}, Size={Size} bytes",
                    sessionId, symbolType, imageData.Length);
            }
        }

        /// <summary>
        /// Broadcasts tolerance change to all session participants.
        /// </summary>
        /// <param name="sessionId">Session ID.</param>
        /// <param name="tolerance">New tolerance value (0.0 = exact, 0.01 = 1% difference).</param>
        public async Task BroadcastToleranceChange(string sessionId, double tolerance)
        {
            var updated = _sessionStore.UpdateSessionState(sessionId, session =>
            {
                session.Tolerance = tolerance;
            });

            if (updated)
            {
                await Clients.Group(sessionId).SendAsync("ToleranceChanged", tolerance);

                _logger.LogInformation(
                    "Tolerance broadcast: SessionId={SessionId}, Tolerance={Tolerance}%",
                    sessionId, tolerance * 100);
            }
        }

        /// <summary>
        /// Broadcasts comparison result to all session participants.
        /// </summary>
        /// <param name="sessionId">Session ID.</param>
        /// <param name="result">Comparison result.</param>
        public async Task BroadcastComparisonResult(string sessionId, ComparisonResult result)
        {
            var updated = _sessionStore.UpdateSessionState(sessionId, session =>
            {
                session.Result = result;
            });

            if (updated)
            {
                await Clients.Group(sessionId).SendAsync("ComparisonCompleted", result);

                _logger.LogInformation(
                    "Comparison result broadcast: SessionId={SessionId}, Similar={Similar}, Similarity={Similarity:F2}%",
                    sessionId, result.Similar, result.SimilarityPercent);
            }
        }

        //==============================================================
        // ANNOTATION METHODS
        //==============================================================

        /// <summary>
        /// Sends annotation to all session participants (instructor feedback).
        /// </summary>
        /// <param name="sessionId">Session ID.</param>
        /// <param name="annotation">Annotation to broadcast.</param>
        /// <remarks>
        /// <para><b>Teaching Moment (Graduate):</b></para>
        /// <para>Annotations allow instructor to draw on diff images in real-time.</para>
        /// <para>All students see annotations immediately (< 500ms latency target).</para>
        /// <para><b>Data Format:</b> Annotation.Data is JSON-serialized drawing coordinates.</para>
        /// </remarks>
        public async Task SendAnnotation(string sessionId, Annotation annotation)
        {
            if (_sessionStore.AddAnnotation(sessionId, annotation))
            {
                // Broadcast to all participants EXCEPT sender (Clients.OthersInGroup)
                await Clients.OthersInGroup(sessionId).SendAsync("AnnotationReceived", annotation);

                _logger.LogInformation(
                    "Annotation broadcast: SessionId={SessionId}, AnnotationId={AnnotationId}, Type={Type}, CreatedBy={CreatedBy}",
                    sessionId, annotation.AnnotationId, annotation.Type, annotation.CreatedBy);
            }
        }

        /// <summary>
        /// Clears all annotations in session (instructor only).
        /// </summary>
        /// <param name="sessionId">Session ID.</param>
        public async Task ClearAnnotations(string sessionId)
        {
            var session = _sessionStore.GetSession(sessionId);
            if (session != null)
            {
                lock (session.Annotations)
                {
                    session.Annotations.Clear();
                }

                await Clients.Group(sessionId).SendAsync("AnnotationsCleared");

                _logger.LogInformation("Annotations cleared: SessionId={SessionId}", sessionId);
            }
        }

        //==============================================================
        // CONNECTION LIFECYCLE HANDLERS
        //==============================================================

        /// <summary>
        /// Called when client connects to hub.
        /// </summary>
        /// <remarks>
        /// <para><b>Teaching Moment (Graduate):</b></para>
        /// <para>OnConnectedAsync is invoked when WebSocket connection established.</para>
        /// <para>Context.ConnectionId is unique identifier for this connection.</para>
        /// </remarks>
        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation(
                "Client connected: ConnectionId={ConnectionId}",
                Context.ConnectionId);

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Called when client disconnects from hub.
        /// </summary>
        /// <param name="exception">Exception if disconnect was due to error.</param>
        /// <remarks>
        /// <para><b>Teaching Moment (Graduate):</b></para>
        /// <para>OnDisconnectedAsync is invoked when WebSocket connection closed (normal or error).</para>
        /// <para>Server removes participant from all sessions to prevent stale connections.</para>
        /// <para><b>Network Partition Handling:</b> Client may reconnect with same ConnectionId
        /// if reconnection grace period hasn't expired (default: 30 seconds).</para>
        /// </remarks>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Find all sessions this connection participated in
            var allSessions = _sessionStore.GetAllSessions();
            foreach (var session in allSessions)
            {
                if (session.Participants.Any(p => p.ConnectionId == Context.ConnectionId))
                {
                    // Remove participant
                    _sessionStore.RemoveParticipant(session.SessionId, Context.ConnectionId);

                    // Notify remaining participants
                    await Clients.Group(session.SessionId).SendAsync("ParticipantLeft", Context.ConnectionId);

                    _logger.LogInformation(
                        "Participant removed on disconnect: SessionId={SessionId}, ConnectionId={ConnectionId}",
                        session.SessionId, Context.ConnectionId);
                }
            }

            if (exception != null)
            {
                _logger.LogWarning(
                    exception,
                    "Client disconnected with error: ConnectionId={ConnectionId}",
                    Context.ConnectionId);
            }
            else
            {
                _logger.LogInformation(
                    "Client disconnected: ConnectionId={ConnectionId}",
                    Context.ConnectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
