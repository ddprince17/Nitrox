using System.Reflection;
using Nitrox.Model.DataStructures;
using NitroxClient.GameLogic;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class MedicalCabinet_OnHandClick_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((MedicalCabinet t) => t.OnHandClick(default(GUIHand)));

    public static void Postfix(MedicalCabinet __instance)
    {
        Resolve<MedkitFabricator>().Clicked(__instance);

        // Persist the medkit state (has-medkit + next-spawn time) as entity metadata so the server stores it and
        // late-joining players see the correct cabinet state instead of an always-present (possibly already-consumed) medkit.
        if (__instance.TryGetIdOrWarn(out NitroxId id))
        {
            Resolve<Entities>().EntityMetadataChanged(__instance, id);
        }
    }
}
