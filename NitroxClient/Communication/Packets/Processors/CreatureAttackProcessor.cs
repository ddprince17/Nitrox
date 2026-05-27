using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class CreatureAttackProcessor : IClientPacketProcessor<CreatureAttack>
{
    public Task Process(ClientProcessorContext context, CreatureAttack packet)
    {
        if (packet.Damage > 0f && Player.main && Player.main.liveMixin)
        {
            Player.main.liveMixin.TakeDamage(packet.Damage);
        }
        return Task.CompletedTask;
    }
}
