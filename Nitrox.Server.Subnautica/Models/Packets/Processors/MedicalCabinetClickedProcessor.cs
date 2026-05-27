using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class MedicalCabinetClickedProcessor(PlayerManager playerManager, EntityRegistry entityRegistry) : TransmitIfCanSeePacketProcessor<MedicalCabinetClicked>(playerManager, entityRegistry)
{
    public override async Task Process(AuthProcessorContext context, MedicalCabinetClicked packet) => await TransmitIfCanSeeEntitiesAsync(context, packet, [packet.Id]);
}
