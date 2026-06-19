using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class MedicalCabinetMetadataProcessor : EntityMetadataProcessor<MedicalCabinetMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, MedicalCabinetMetadata metadata)
    {
        if (!gameObject.TryGetComponent(out MedicalCabinet cabinet))
        {
            Log.Error($"Could not find MedicalCabinet on {gameObject.name}");
            return;
        }

        cabinet.hasMedKit = metadata.HasMedKit;
        cabinet.timeSpawnMedKit = metadata.NextSpawnTime;
        if (cabinet.medKitModel)
        {
            cabinet.medKitModel.SetActive(metadata.HasMedKit);
        }
    }
}
