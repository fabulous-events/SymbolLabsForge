//===============================================================
// File: SessionStoreTests.cs
// Author: Claude (Phase 10.7 Workstream 4 - Testing & Documentation)
// Date: 2025-11-15
// Purpose: Unit tests for SessionStore (Phase 10.6 infrastructure).
//
// PHASE 10.7 WORKSTREAM 4: TESTING & DOCUMENTATION
//   - Thread-safe operation validation
//   - Participant management correctness
//   - Session cleanup behavior
//   - Concurrent access stress testing
//
// WHY THIS MATTERS:
//   - Ensures distributed state management is correct under concurrency
//   - Validates RBAC and session lifecycle patterns
//   - Demonstrates testing strategies for multi-threaded code
//
// TEACHING VALUE:
//   - Graduate: Unit testing concurrent code, thread-safety validation
//   - PhD: Race condition detection, stress testing patterns
//
// AUDIENCE: Graduate / PhD (concurrent programming, distributed systems testing)
//===============================================================

using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SymbolLabsForge.UI.Web.Hubs;

namespace SymbolLabsForge.Tests.UI.Web
{
    /// <summary>
    /// Unit tests for SessionStore thread-safe session storage.
    /// </summary>
    /// <remarks>
    /// <para><b>Phase 10.7 Workstream 4: Testing Infrastructure</b></para>
    /// <para>Validates:</para>
    /// <list type="bullet">
    /// <item>Thread-safe session creation and retrieval</item>
    /// <item>Participant management with concurrent access</item>
    /// <item>Session cleanup with time-based expiration</item>
    /// <item>Stress testing with multiple concurrent operations</item>
    /// </list>
    /// </remarks>
    public class SessionStoreTests
    {
        private readonly ILogger<SessionStore> _logger;

        public SessionStoreTests()
        {
            _logger = NullLogger<SessionStore>.Instance;
        }

        //==============================================================
        // SESSION CREATION & RETRIEVAL TESTS
        //==============================================================

        [Fact]
        public void CreateSession_ShouldGenerateUniqueSessionId()
        {
            // Arrange
            var store = new SessionStore(_logger);

            // Act
            var session1 = store.CreateSession("User1");
            var session2 = store.CreateSession("User2");

            // Assert
            Assert.NotEqual(session1.SessionId, session2.SessionId);
            Assert.Equal(32, session1.SessionId.Length); // GUID without hyphens
            Assert.Equal(32, session2.SessionId.Length);
        }

        [Fact]
        public void CreateSession_ShouldSetMetadata()
        {
            // Arrange
            var store = new SessionStore(_logger);
            var createdBy = "TestUser";

            // Act
            var session = store.CreateSession(createdBy);

            // Assert
            Assert.Equal(createdBy, session.CreatedBy);
            Assert.True((DateTime.UtcNow - session.CreatedAt).TotalSeconds < 1); // Created within 1 second
            Assert.True((DateTime.UtcNow - session.LastActivityAt).TotalSeconds < 1);
        }

        [Fact]
        public void GetSession_ShouldReturnExistingSession()
        {
            // Arrange
            var store = new SessionStore(_logger);
            var session = store.CreateSession("User1");

            // Act
            var retrieved = store.GetSession(session.SessionId);

            // Assert
            Assert.NotNull(retrieved);
            Assert.Equal(session.SessionId, retrieved.SessionId);
            Assert.Equal(session.CreatedBy, retrieved.CreatedBy);
        }

        [Fact]
        public void GetSession_ShouldReturnNullForNonExistentSession()
        {
            // Arrange
            var store = new SessionStore(_logger);

            // Act
            var retrieved = store.GetSession("nonexistent");

            // Assert
            Assert.Null(retrieved);
        }

        [Fact]
        public void GetSessionCount_ShouldReturnCorrectCount()
        {
            // Arrange
            var store = new SessionStore(_logger);

            // Act & Assert
            Assert.Equal(0, store.GetSessionCount());

            store.CreateSession("User1");
            Assert.Equal(1, store.GetSessionCount());

            store.CreateSession("User2");
            Assert.Equal(2, store.GetSessionCount());
        }

        //==============================================================
        // PARTICIPANT MANAGEMENT TESTS
        //==============================================================

        [Fact]
        public void AddParticipant_ShouldAddToSession()
        {
            // Arrange
            var store = new SessionStore(_logger);
            var session = store.CreateSession("User1");
            var participant = new SessionParticipant
            {
                ConnectionId = "conn1",
                DisplayName = "Alice",
                Role = ParticipantRole.Student
            };

            // Act
            var added = store.AddParticipant(session.SessionId, participant);

            // Assert
            Assert.True(added);
            var retrieved = store.GetSession(session.SessionId);
            Assert.NotNull(retrieved);
            Assert.Single(retrieved.Participants);
            Assert.Equal("conn1", retrieved.Participants[0].ConnectionId);
            Assert.Equal("Alice", retrieved.Participants[0].DisplayName);
        }

        [Fact]
        public void AddParticipant_ShouldNotAddDuplicateConnectionId()
        {
            // Arrange
            var store = new SessionStore(_logger);
            var session = store.CreateSession("User1");
            var participant1 = new SessionParticipant { ConnectionId = "conn1", DisplayName = "Alice", Role = ParticipantRole.Student };
            var participant2 = new SessionParticipant { ConnectionId = "conn1", DisplayName = "Bob", Role = ParticipantRole.Student };

            // Act
            var added1 = store.AddParticipant(session.SessionId, participant1);
            var added2 = store.AddParticipant(session.SessionId, participant2);

            // Assert
            Assert.True(added1);
            Assert.False(added2); // Duplicate connection ID
            var retrieved = store.GetSession(session.SessionId);
            Assert.NotNull(retrieved);
            Assert.Single(retrieved.Participants); // Only one participant
            Assert.Equal("Alice", retrieved.Participants[0].DisplayName); // First one remains
        }

        [Fact]
        public void AddParticipant_ShouldUpdateLastActivityAt()
        {
            // Arrange
            var store = new SessionStore(_logger);
            var session = store.CreateSession("User1");
            var originalActivity = session.LastActivityAt;

            Thread.Sleep(10); // Ensure time difference

            var participant = new SessionParticipant { ConnectionId = "conn1", DisplayName = "Alice", Role = ParticipantRole.Student };

            // Act
            store.AddParticipant(session.SessionId, participant);

            // Assert
            var retrieved = store.GetSession(session.SessionId);
            Assert.NotNull(retrieved);
            Assert.True(retrieved.LastActivityAt > originalActivity);
        }

        [Fact]
        public void RemoveParticipant_ShouldRemoveFromSession()
        {
            // Arrange
            var store = new SessionStore(_logger);
            var session = store.CreateSession("User1");
            var participant = new SessionParticipant { ConnectionId = "conn1", DisplayName = "Alice", Role = ParticipantRole.Student };
            store.AddParticipant(session.SessionId, participant);

            // Act
            var removed = store.RemoveParticipant(session.SessionId, "conn1");

            // Assert
            Assert.True(removed);
            var retrieved = store.GetSession(session.SessionId);
            Assert.NotNull(retrieved);
            Assert.Empty(retrieved.Participants);
        }

        [Fact]
        public void RemoveParticipant_ShouldReturnFalseForNonExistent()
        {
            // Arrange
            var store = new SessionStore(_logger);
            var session = store.CreateSession("User1");

            // Act
            var removed = store.RemoveParticipant(session.SessionId, "nonexistent");

            // Assert
            Assert.False(removed);
        }

        //==============================================================
        // STATE UPDATE TESTS
        //==============================================================

        [Fact]
        public void UpdateSessionState_ShouldApplyChanges()
        {
            // Arrange
            var store = new SessionStore(_logger);
            var session = store.CreateSession("User1");

            // Act
            var updated = store.UpdateSessionState(session.SessionId, s =>
            {
                s.Tolerance = 0.05;
                s.SymbolType = SymbolLabsForge.UI.Web.Services.SymbolType.Sharp;
            });

            // Assert
            Assert.True(updated);
            var retrieved = store.GetSession(session.SessionId);
            Assert.NotNull(retrieved);
            Assert.Equal(0.05, retrieved.Tolerance);
            Assert.Equal(SymbolLabsForge.UI.Web.Services.SymbolType.Sharp, retrieved.SymbolType);
        }

        [Fact]
        public void UpdateSessionState_ShouldUpdateLastActivityAt()
        {
            // Arrange
            var store = new SessionStore(_logger);
            var session = store.CreateSession("User1");
            var originalActivity = session.LastActivityAt;

            Thread.Sleep(10); // Ensure time difference

            // Act
            store.UpdateSessionState(session.SessionId, s => s.Tolerance = 0.05);

            // Assert
            var retrieved = store.GetSession(session.SessionId);
            Assert.NotNull(retrieved);
            Assert.True(retrieved.LastActivityAt > originalActivity);
        }

        [Fact]
        public void AddAnnotation_ShouldAddToSession()
        {
            // Arrange
            var store = new SessionStore(_logger);
            var session = store.CreateSession("User1");
            var annotation = new Annotation
            {
                CreatedBy = "Instructor",
                Type = AnnotationType.Pen,
                Data = "{\"points\": [[10,20], [15,25]]}"
            };

            // Act
            var added = store.AddAnnotation(session.SessionId, annotation);

            // Assert
            Assert.True(added);
            var retrieved = store.GetSession(session.SessionId);
            Assert.NotNull(retrieved);
            Assert.Single(retrieved.Annotations);
            Assert.Equal("Instructor", retrieved.Annotations[0].CreatedBy);
        }

        //==============================================================
        // SESSION CLEANUP TESTS
        //==============================================================

        [Fact]
        public void CleanupExpiredSessions_ShouldRemoveOldSessions()
        {
            // Arrange
            var store = new SessionStore(_logger);
            var session1 = store.CreateSession("User1");
            var session2 = store.CreateSession("User2");

            // Get the session object and manipulate it directly
            var retrievedSession1 = store.GetSession(session1.SessionId);
            if (retrievedSession1 != null)
            {
                // Set LastActivityAt to 25 hours ago using property setter
                retrievedSession1.LastActivityAt = DateTime.UtcNow.AddHours(-25);
            }

            // Act
            var removed = store.CleanupExpiredSessions(TimeSpan.FromHours(24));

            // Assert
            Assert.Equal(1, removed); // Only session1 should be removed
            Assert.Equal(1, store.GetSessionCount()); // Only session2 remains
            Assert.Null(store.GetSession(session1.SessionId));
            Assert.NotNull(store.GetSession(session2.SessionId));
        }

        [Fact]
        public void CleanupExpiredSessions_ShouldNotRemoveActiveSessions()
        {
            // Arrange
            var store = new SessionStore(_logger);
            var session = store.CreateSession("User1");

            // Act
            var removed = store.CleanupExpiredSessions(TimeSpan.FromHours(24));

            // Assert
            Assert.Equal(0, removed);
            Assert.Equal(1, store.GetSessionCount());
            Assert.NotNull(store.GetSession(session.SessionId));
        }

        //==============================================================
        // CONCURRENT ACCESS TESTS (Stress Testing)
        //==============================================================

        [Fact]
        public void ConcurrentSessionCreation_ShouldBeThreadSafe()
        {
            // Arrange
            var store = new SessionStore(_logger);
            var sessionIds = new System.Collections.Concurrent.ConcurrentBag<string>();

            // Act - Create 100 sessions concurrently
            Parallel.For(0, 100, i =>
            {
                var session = store.CreateSession($"User{i}");
                sessionIds.Add(session.SessionId);
            });

            // Assert
            Assert.Equal(100, store.GetSessionCount());
            Assert.Equal(100, sessionIds.Distinct().Count()); // All unique session IDs
        }

        [Fact]
        public void ConcurrentParticipantAddition_ShouldBeThreadSafe()
        {
            // Arrange
            var store = new SessionStore(_logger);
            var session = store.CreateSession("User1");

            // Act - Add 50 participants concurrently
            Parallel.For(0, 50, i =>
            {
                var participant = new SessionParticipant
                {
                    ConnectionId = $"conn{i}",
                    DisplayName = $"User{i}",
                    Role = ParticipantRole.Student
                };
                store.AddParticipant(session.SessionId, participant);
            });

            // Assert
            var retrieved = store.GetSession(session.SessionId);
            Assert.NotNull(retrieved);
            Assert.Equal(50, retrieved.Participants.Count);
        }

        [Fact]
        public void ConcurrentStateUpdates_ShouldBeThreadSafe()
        {
            // Arrange
            var store = new SessionStore(_logger);
            var session = store.CreateSession("User1");

            // Act - Update state 100 times concurrently
            Parallel.For(0, 100, i =>
            {
                store.UpdateSessionState(session.SessionId, s =>
                {
                    s.Tolerance = i * 0.01; // Last write wins
                });
            });

            // Assert
            var retrieved = store.GetSession(session.SessionId);
            Assert.NotNull(retrieved);
            Assert.InRange(retrieved.Tolerance, 0.0, 0.99); // Some value between 0 and 0.99
        }

        //==============================================================
        // EDGE CASE TESTS
        //==============================================================

        [Fact]
        public void AddParticipant_ToNonExistentSession_ShouldReturnFalse()
        {
            // Arrange
            var store = new SessionStore(_logger);
            var participant = new SessionParticipant { ConnectionId = "conn1", DisplayName = "Alice", Role = ParticipantRole.Student };

            // Act
            var added = store.AddParticipant("nonexistent", participant);

            // Assert
            Assert.False(added);
        }

        [Fact]
        public void UpdateSessionState_OnNonExistentSession_ShouldReturnFalse()
        {
            // Arrange
            var store = new SessionStore(_logger);

            // Act
            var updated = store.UpdateSessionState("nonexistent", s => s.Tolerance = 0.05);

            // Assert
            Assert.False(updated);
        }

        [Fact]
        public void GetAllSessions_ShouldReturnReadOnlyList()
        {
            // Arrange
            var store = new SessionStore(_logger);
            store.CreateSession("User1");
            store.CreateSession("User2");

            // Act
            var sessions = store.GetAllSessions();

            // Assert
            Assert.Equal(2, sessions.Count);
            Assert.IsAssignableFrom<IReadOnlyList<ComparisonSession>>(sessions);
        }
    }
}
