using System.Reflection;
using RimMind.Bridge.RimChat.Cooldown;
using Xunit;

namespace RimMind.Bridge.RimChat.Tests
{
    public class SharedIncidentCooldownTests
    {
        public SharedIncidentCooldownTests()
        {
            ResetSharedIncidentCooldown();
            Verse.Find.TickManager.TicksGame = 100000;
        }

        private static void ResetSharedIncidentCooldown()
        {
            var field = typeof(SharedIncidentCooldown).GetField("_lastIncidentTick",
                BindingFlags.NonPublic | BindingFlags.Static);
            if (field != null)
                field.SetValue(null, -99999);
        }

        [Fact]
        public void IsOnCooldown_NoIncidentRecorded_ReturnsFalse()
        {
            Assert.False(SharedIncidentCooldown.IsOnCooldown(60000));
        }

        [Fact]
        public void IsOnCooldown_JustRecorded_ReturnsTrue()
        {
            SharedIncidentCooldown.RecordIncident();
            Assert.True(SharedIncidentCooldown.IsOnCooldown(60000));
        }

        [Fact]
        public void IsOnCooldown_AfterCooldownExpires_ReturnsFalse()
        {
            SharedIncidentCooldown.RecordIncident();
            Verse.Find.TickManager.TicksGame = 100000 + 60001;
            Assert.False(SharedIncidentCooldown.IsOnCooldown(60000));
        }

        [Fact]
        public void IsOnCooldown_ExactlyAtCooldown_ReturnsFalse()
        {
            SharedIncidentCooldown.RecordIncident();
            Verse.Find.TickManager.TicksGame = 100000 + 60000;
            Assert.False(SharedIncidentCooldown.IsOnCooldown(60000));
        }

        [Fact]
        public void IsOnCooldown_OneTickBeforeExpiry_ReturnsTrue()
        {
            SharedIncidentCooldown.RecordIncident();
            Verse.Find.TickManager.TicksGame = 100000 + 59999;
            Assert.True(SharedIncidentCooldown.IsOnCooldown(60000));
        }

        [Fact]
        public void IsOnCooldown_ZeroCooldown_ReturnsFalse()
        {
            SharedIncidentCooldown.RecordIncident();
            Assert.False(SharedIncidentCooldown.IsOnCooldown(0));
        }

        [Fact]
        public void RecordIncident_UpdatesLastTick()
        {
            Verse.Find.TickManager.TicksGame = 200000;
            SharedIncidentCooldown.RecordIncident();
            Assert.True(SharedIncidentCooldown.IsOnCooldown(60000));
        }

        [Fact]
        public void RecordIncident_MultipleRecords_LastWins()
        {
            Verse.Find.TickManager.TicksGame = 100000;
            SharedIncidentCooldown.RecordIncident();
            Verse.Find.TickManager.TicksGame = 150000;
            SharedIncidentCooldown.RecordIncident();
            Verse.Find.TickManager.TicksGame = 150000 + 30000;
            Assert.True(SharedIncidentCooldown.IsOnCooldown(60000));
        }

        [Fact]
        public void Reset_ViaReflection_ResetsToInitial()
        {
            SharedIncidentCooldown.RecordIncident();
            ResetSharedIncidentCooldown();
            Assert.False(SharedIncidentCooldown.IsOnCooldown(60000));
        }
    }
}
