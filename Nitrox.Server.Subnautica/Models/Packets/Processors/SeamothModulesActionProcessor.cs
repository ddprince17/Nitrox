using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class SeamothModulesActionProcessor(PlayerManager playerManager, EntityRegistry entityRegistry) : TransmitIfCanSeePacketProcessor<SeamothModulesAction>(playerManager, entityRegistry)
{
    public override async Task Process(AuthProcessorContext context, SeamothModulesAction packet) => await TransmitIfCanSeeEntitiesAsync(context, packet, [packet.Id]);
}
