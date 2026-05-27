using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class OpenableStateChangedProcessor(PlayerManager playerManager, EntityRegistry entityRegistry) : TransmitIfCanSeePacketProcessor<OpenableStateChanged>(playerManager, entityRegistry)
{
    public override async Task Process(AuthProcessorContext context, OpenableStateChanged packet) => await TransmitIfCanSeeEntitiesAsync(context, packet, [packet.Id]);
}
