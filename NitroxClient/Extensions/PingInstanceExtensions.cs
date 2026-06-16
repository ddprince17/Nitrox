namespace NitroxClient.Extensions;

internal static class PingInstanceExtensions
{
    extension(PingInstance self)
    {
        /// <summary>
        ///     If true, ping instance should not be synchronized to remote players.
        /// </summary>
        public bool IsLocalOnly
        {
            // Each local-only ping needs a unique id so multiple coexisting ones (e.g. several death beacons)
            // don't collide on the same PingManager dictionary key. The "local" prefix keeps them non-networked
            // (see PlayerPreferencesInitialSyncProcessor.TryGetKeyForPingInstance) and survives PingInstance.Initialize
            // (which only assigns a new id when _id is empty).
            set => self._id = $"local_{System.Guid.NewGuid()}";
            get => self._id != null && self._id.StartsWith("local");
        }
    }
}
