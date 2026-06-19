using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using Nitrox.Model.DataStructures;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Synchronizes a base <see cref="PowerSource" />'s stored <c>power</c> (solar/thermal/reactor/relay charge) so a base
/// drained on one client no longer shows full to others or after a reload/join. Only the simulation owner broadcasts,
/// and only when the integer power value changes, to bound packet rate (mirrors <see cref="Battery_charge_set_Patch" />).
/// Power changes flow through <see cref="PowerSource.ModifyPower" /> (generation adds, consumption subtracts).
/// </summary>
public sealed partial class PowerSource_ModifyPower_Patch : NitroxPatch, IDynamicPatch
{
    // ModifyPower has an out parameter, so resolve it by name rather than an expression tree.
    public static readonly MethodInfo TARGET_METHOD = AccessTools.Method(typeof(PowerSource), nameof(PowerSource.ModifyPower));

    public static void Prefix(PowerSource __instance, out float __state)
    {
        __state = __instance.power;
    }

    public static void Postfix(PowerSource __instance, float __state)
    {
        if (Math.Abs(Math.Floor(__state) - Math.Floor(__instance.power)) > 0.0 &&
            __instance.TryGetNitroxId(out NitroxId id) &&
            Resolve<SimulationOwnership>().HasAnyLockType(id))
        {
            Resolve<Entities>().EntityMetadataChangedThrottled(__instance, id);
        }
    }
}
