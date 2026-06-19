using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class PowerSourceMetadataProcessor : EntityMetadataProcessor<PowerSourceMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, PowerSourceMetadata metadata)
    {
        PowerSource powerSource = gameObject.GetComponent<PowerSource>();
        if (powerSource)
        {
            // Write the field directly (not SetPower) so this doesn't re-trigger PowerSource_ModifyPower_Patch's broadcast.
            powerSource.power = metadata.Power;
        }
        else
        {
            Log.Error($"Could not find PowerSource on {gameObject.name}");
        }
    }
}
