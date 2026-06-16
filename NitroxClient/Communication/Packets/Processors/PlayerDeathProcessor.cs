using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class PlayerDeathProcessor(PlayerManager playerManager) : IClientPacketProcessor<PlayerDeathEvent>
{
    private readonly PlayerManager playerManager = playerManager;

    public Task Process(ClientProcessorContext context, PlayerDeathEvent playerDeath)
    {
        // A death packet can race the dying player's disconnect or arrive before registration; ignore if absent
        // (matches the guarded pattern used by the other player processors) instead of throwing.
        Optional<RemotePlayer> optionalPlayer = playerManager.Find(playerDeath.SessionId);
        if (!optionalPlayer.HasValue)
        {
            Log.Warn($"Received {nameof(PlayerDeathEvent)} for unknown player {playerDeath.SessionId}");
            return Task.CompletedTask;
        }
        RemotePlayer player = optionalPlayer.Value;
        Log.Debug($"{player.PlayerName} died");
        Log.InGame(Language.main.Get("Nitrox_PlayerDied").Replace("{PLAYER}", player.PlayerName));
        player.PlayerDeathEvent.Trigger(player);
        return Task.CompletedTask;

        // TODO: Add any death related triggers (i.e. scoreboard updates, rewards, etc.)
    }
}
