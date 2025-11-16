//===============================================================
// File: ComparisonSession.cs
// Author: Claude (Phase 10.6 - Real-Time Collaboration)
// Date: 2025-11-15
// Purpose: Session state model for collaborative comparison sessions.
//
// PHASE 10.6: REAL-TIME COLLABORATION
//   - Shareable comparison sessions with unique session IDs
//   - Multi-user participation tracking
//   - Real-time state synchronization (uploads, tolerance, results)
//   - Annotation support for instructor feedback
//
// WHY THIS MATTERS:
//   - Students can collaborate on symbol comparisons in real-time
//   - Instructors can provide live feedback via annotations
//   - Demonstrates distributed state management patterns
//   - Real-world pattern for multi-user applications
//
// TEACHING VALUE:
//   - Graduate: Distributed state, WebSocket communication
//   - PhD: Eventual consistency, conflict resolution
//
// AUDIENCE: Graduate / PhD (distributed systems, real-time collaboration)
//===============================================================
#nullable enable

using SymbolLabsForge.UI.Web.Services;

namespace SymbolLabsForge.UI.Web.Hubs
{
    /// <summary>
    /// Represents a collaborative comparison session with multiple participants.
    /// </summary>
    /// <remarks>
    /// <para><b>Phase 10.6: Real-Time Collaboration</b></para>
    /// <para>A session maintains shared state across multiple users:</para>
    /// <list type="bullet">
    /// <item>Session ID (unique GUID for shareable URL)</item>
    /// <item>Comparison state (uploaded image, symbol type, tolerance, results)</item>
    /// <item>Participants (list of connected users with roles)</item>
    /// <item>Annotations (instructor feedback overlays)</item>
    /// </list>
    /// <para><b>Teaching Value (Graduate):</b> Demonstrates distributed state management.</para>
    /// </remarks>
    public class ComparisonSession
    {
        /// <summary>
        /// Unique session identifier (GUID without hyphens for shareable URLs).
        /// </summary>
        /// <remarks>
        /// <para>Example: "a1b2c3d4e5f678901234567890abcdef"</para>
        /// <para>Used in URL: /collaboration/{sessionId}</para>
        /// </remarks>
        public string SessionId { get; init; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// Timestamp when session was created (UTC).
        /// </summary>
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

        /// <summary>
        /// Display name of user who created the session.
        /// </summary>
        public string CreatedBy { get; init; } = "Unknown";

        /// <summary>
        /// Uploaded symbol image data (base64 or byte array).
        /// </summary>
        /// <remarks>
        /// <para><b>Teaching Moment (Graduate):</b></para>
        /// <para>Large binary data (images) can be stored as byte[] or base64 string.</para>
        /// <para>Consider trade-offs: byte[] = smaller in memory, base64 = easier to serialize.</para>
        /// </remarks>
        public byte[]? UploadedImageData { get; set; }

        /// <summary>
        /// Selected symbol type for comparison.
        /// </summary>
        public SymbolType? SymbolType { get; set; }

        /// <summary>
        /// Tolerance for comparison (0.0 = exact, 0.01 = 1% difference allowed).
        /// </summary>
        public double Tolerance { get; set; } = 0.01;

        /// <summary>
        /// Comparison result (null if not yet compared).
        /// </summary>
        public ComparisonResult? Result { get; set; }

        /// <summary>
        /// List of participants currently in this session.
        /// </summary>
        /// <remarks>
        /// <para><b>Teaching Moment (Graduate):</b></para>
        /// <para>Concurrent collections (e.g., ConcurrentBag) provide thread-safe access for multi-user scenarios.</para>
        /// <para>For simplicity, we use List with manual locking in SessionStore.</para>
        /// </remarks>
        public List<SessionParticipant> Participants { get; set; } = new();

        /// <summary>
        /// List of annotations added by participants (instructor feedback).
        /// </summary>
        public List<Annotation> Annotations { get; set; } = new();

        /// <summary>
        /// Timestamp of last activity in this session (UTC).
        /// Used for session cleanup (remove inactive sessions).
        /// </summary>
        public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Whether session is read-only (embedded mode for LMS).
        /// </summary>
        /// <remarks>
        /// <para><b>Teaching Moment (Graduate):</b></para>
        /// <para>Read-only mode prevents modifications in embedded views (Canvas LMS, Moodle).</para>
        /// <para>URL parameter: ?readonly=true</para>
        /// </remarks>
        public bool IsReadOnly { get; set; }
    }

    /// <summary>
    /// Represents a participant in a collaborative session.
    /// </summary>
    public class SessionParticipant
    {
        /// <summary>
        /// SignalR connection ID (unique per browser connection).
        /// </summary>
        /// <remarks>
        /// <para><b>Teaching Moment (Graduate):</b></para>
        /// <para>SignalR assigns unique connection ID to each WebSocket connection.</para>
        /// <para>Used to target specific clients in broadcast operations.</para>
        /// </remarks>
        public string ConnectionId { get; init; } = string.Empty;

        /// <summary>
        /// Display name entered by participant.
        /// </summary>
        public string DisplayName { get; init; } = "Anonymous";

        /// <summary>
        /// Role of participant in session (Instructor, Student, Viewer).
        /// </summary>
        public ParticipantRole Role { get; init; } = ParticipantRole.Student;

        /// <summary>
        /// Timestamp when participant joined session (UTC).
        /// </summary>
        public DateTime JoinedAt { get; init; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Role of participant in collaborative session.
    /// </summary>
    /// <remarks>
    /// <para><b>Teaching Moment (Graduate):</b></para>
    /// <para>Role-based access control (RBAC) pattern.</para>
    /// <para>Instructor: Full permissions (annotations, state changes)</para>
    /// <para>Student: Limited permissions (view, upload own symbols)</para>
    /// <para>Viewer: Read-only (LMS embedded mode)</para>
    /// </remarks>
    public enum ParticipantRole
    {
        /// <summary>Instructor with full permissions.</summary>
        Instructor,

        /// <summary>Student with limited permissions.</summary>
        Student,

        /// <summary>Viewer with read-only access.</summary>
        Viewer
    }

    /// <summary>
    /// Represents an annotation added to comparison results.
    /// </summary>
    /// <remarks>
    /// <para><b>Teaching Moment (Graduate):</b></para>
    /// <para>Annotations provide instructor feedback overlays on diff images.</para>
    /// <para>Serialized as JSON for SignalR transmission.</para>
    /// </remarks>
    public class Annotation
    {
        /// <summary>
        /// Unique annotation identifier (GUID).
        /// </summary>
        public string AnnotationId { get; init; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// Display name of user who created annotation.
        /// </summary>
        public string CreatedBy { get; init; } = "Unknown";

        /// <summary>
        /// Timestamp when annotation was created (UTC).
        /// </summary>
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

        /// <summary>
        /// Type of annotation (Pen, Highlighter, Comment).
        /// </summary>
        public AnnotationType Type { get; init; } = AnnotationType.Pen;

        /// <summary>
        /// Annotation data (JSON serialized drawing coordinates or comment text).
        /// </summary>
        /// <remarks>
        /// <para><b>Example (Pen):</b> {"points": [[10,20], [15,25], ...], "color": "#ff0000"}</para>
        /// <para><b>Example (Comment):</b> {"text": "Focus on this region", "x": 100, "y": 150}</para>
        /// </remarks>
        public string Data { get; init; } = "{}";
    }

    /// <summary>
    /// Type of annotation.
    /// </summary>
    public enum AnnotationType
    {
        /// <summary>Pen drawing (freehand line).</summary>
        Pen,

        /// <summary>Highlighter (semi-transparent overlay).</summary>
        Highlighter,

        /// <summary>Text comment (positioned label).</summary>
        Comment
    }
}
