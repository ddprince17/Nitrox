using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class MedicalCabinetMetadataExtractor : EntityMetadataExtractor<MedicalCabinet, MedicalCabinetMetadata>
{
    public override MedicalCabinetMetadata Extract(MedicalCabinet entity)
    {
        return new(entity.hasMedKit, entity.timeSpawnMedKit);
    }
}
