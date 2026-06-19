using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class MedicalCabinetMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public bool HasMedKit { get; }

    [DataMember(Order = 2)]
    public float NextSpawnTime { get; }

    [IgnoreConstructor]
    protected MedicalCabinetMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public MedicalCabinetMetadata(bool hasMedKit, float nextSpawnTime)
    {
        HasMedKit = hasMedKit;
        NextSpawnTime = nextSpawnTime;
    }

    public override string ToString()
    {
        return $"[MedicalCabinetMetadata HasMedKit: {HasMedKit}, NextSpawnTime: {NextSpawnTime}]";
    }
}
