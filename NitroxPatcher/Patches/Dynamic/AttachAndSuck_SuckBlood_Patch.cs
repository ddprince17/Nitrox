using System.Reflection;
using HarmonyLib;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class AttachAndSuck_SuckBlood_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = AccessTools.Method(typeof(AttachAndSuck), "SuckBlood");

    public static void Prefix(out float __state, float ___timeLastSuck)
    {
        __state = ___timeLastSuck;
    }

    public static void Postfix(float __state, AttachAndSuck __instance, float ___timeLastSuck, LiveMixin ___targetLiveMixin)
    {
        if (__state != ___timeLastSuck)
        {
            LiveMixin_TakeDamage_Patch.TryBroadcastRemotePlayerCreatureDamage(__instance.leechDamage, ___targetLiveMixin, __instance.bleeder);
        }
    }
}
