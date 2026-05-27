using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using ToggleLightsPacket = Nitrox.Model.Subnautica.Packets.ToggleLights;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class ToggleLightsProcessor(PlayerManager playerManager, EntityRegistry entityRegistry) : TransmitIfCanSeePacketProcessor<ToggleLightsPacket>(playerManager, entityRegistry)
{
    public override async Task Process(AuthProcessorContext context, ToggleLightsPacket packet) => await TransmitIfCanSeeEntitiesAsync(context, packet, [packet.Id]);
}
