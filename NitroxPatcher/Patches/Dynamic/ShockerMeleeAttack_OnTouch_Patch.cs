using System.Reflection;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class ShockerMeleeAttack_OnTouch_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((ShockerMeleeAttack t) => t.OnTouch(default));

    public static void Prefix(out float __state, float ___timeLastElectricalDamage)
    {
        __state = ___timeLastElectricalDamage;
    }

    public static void Postfix(float __state, ShockerMeleeAttack __instance, Collider collider, float ___timeLastElectricalDamage)
    {
        if (__state == ___timeLastElectricalDamage)
        {
            return;
        }

        GameObject target = __instance.GetTarget(collider);
        if (!target || !target.TryGetComponent(out LiveMixin liveMixin))
        {
            return;
        }

        bool isCyclops = target.GetComponent<SubControl>() != null;
        float damage = isCyclops ? __instance.cyclopsDamage : __instance.electricalDamage;
        LiveMixin_TakeDamage_Patch.TryBroadcastRemotePlayerCreatureDamage(damage, liveMixin, __instance.creature);
    }
}
