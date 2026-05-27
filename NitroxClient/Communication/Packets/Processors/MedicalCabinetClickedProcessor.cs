using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class MedicalCabinetClickedProcessor : IClientPacketProcessor<MedicalCabinetClicked>
{
    public Task Process(ClientProcessorContext context, MedicalCabinetClicked packet)
    {
        if (!NitroxEntity.TryGetObjectFrom(packet.Id, out GameObject gameObject))
        {
            Log.Warn($"[{nameof(MedicalCabinetClickedProcessor)}] Could not find entity with id: {packet.Id}.");
            return Task.CompletedTask;
        }

        if (!gameObject.TryGetComponent(out MedicalCabinet cabinet))
        {
            Log.Warn($"[{nameof(MedicalCabinetClickedProcessor)}] Entity with id: {packet.Id} has no MedicalCabinet component.");
            return Task.CompletedTask;
        }

        bool medkitPickedUp = !packet.HasMedKit && cabinet.hasMedKit;
        bool doorChangedState = cabinet.doorOpen != packet.DoorOpen;

        cabinet.hasMedKit = packet.HasMedKit;
        cabinet.timeSpawnMedKit = packet.NextSpawnTime;

        using (PacketSuppressor<FMODCustomEmitterPacket>.Suppress())
        using (FMODSystem.SuppressSubnauticaSounds())
        {
            if (doorChangedState)
            {
                cabinet.Invoke(nameof(MedicalCabinet.ToggleDoorState), 0f);
            }
            else if (medkitPickedUp)
            {
                cabinet.Invoke(nameof(MedicalCabinet.ToggleDoorState), 2f);
            }
        }
        return Task.CompletedTask;
    }
}
