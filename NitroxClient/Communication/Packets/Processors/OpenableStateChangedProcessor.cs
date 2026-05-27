using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class OpenableStateChangedProcessor : IClientPacketProcessor<OpenableStateChanged>
{
    public Task Process(ClientProcessorContext context, OpenableStateChanged packet)
    {
        if (!NitroxEntity.TryGetObjectFrom(packet.Id, out GameObject gameObject))
        {
            Log.Warn($"[{nameof(OpenableStateChangedProcessor)}] Could not find entity with id: {packet.Id}.");
            return Task.CompletedTask;
        }

        if (!gameObject.TryGetComponent(out Openable openable))
        {
            Log.Warn($"[{nameof(OpenableStateChangedProcessor)}] Entity with id: {packet.Id} has no Openable component.");
            return Task.CompletedTask;
        }

        using (PacketSuppressor<OpenableStateChanged>.Suppress())
        {
            openable.PlayOpenAnimation(packet.IsOpen, packet.Duration);
        }
        return Task.CompletedTask;
    }
}
