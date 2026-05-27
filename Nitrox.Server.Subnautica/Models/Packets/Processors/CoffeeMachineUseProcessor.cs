using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class CoffeeMachineUseProcessor(PlayerManager playerManager, EntityRegistry entityRegistry) : TransmitIfCanSeePacketProcessor<CoffeeMachineUse>(playerManager, entityRegistry)
{
    public override async Task Process(AuthProcessorContext context, CoffeeMachineUse packet) => await TransmitIfCanSeeEntitiesAsync(context, packet, [packet.Id]);
}
