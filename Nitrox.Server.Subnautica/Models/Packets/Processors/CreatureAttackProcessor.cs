using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class CreatureAttackProcessor(PlayerManager playerManager, EntityRegistry entityRegistry, SimulationOwnershipData simulationOwnershipData) : IAuthPacketProcessor<CreatureAttack>
{
    private readonly PlayerManager playerManager = playerManager;
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly SimulationOwnershipData simulationOwnershipData = simulationOwnershipData;

    public async Task Process(AuthProcessorContext context, CreatureAttack packet)
    {
        if (packet.Damage <= 0f ||
            packet.TargetSessionId == context.Sender.SessionId ||
            !playerManager.TryGetPlayerBySessionId(packet.TargetSessionId, out Player? targetPlayer) ||
            !entityRegistry.TryGetEntityById(packet.CreatureId, out Entity creatureEntity) ||
            !targetPlayer.CanSee(creatureEntity) ||
            !simulationOwnershipData.TryGetLock(packet.CreatureId, out SimulationOwnershipData.PlayerLock playerLock) ||
            playerLock.Player != context.Sender)
        {
            return;
        }

        await context.SendAsync(packet, targetPlayer.SessionId);
    }
}
