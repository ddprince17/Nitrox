using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxPatcher.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Broadcasts entry progress and entry unlockings when they happen.
/// </summary>
/// <remarks>
/// This is the only method that needs to be tracked to sync and persist PDAScanner's data, because the other methods
/// are either unused (found out that they have no actual calls in SN's code, just as <see cref="PDAScanner.RemoveAllEntriesWhichUnlocks"/>)
/// or simply called automatically by other events (just as <see cref="PDAScanner.CompleteAllEntriesWhichUnlocks"/>).
/// </remarks>
public sealed partial class PDAScanner_Scan_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => PDAScanner.Scan());

    internal static readonly OpCode INJECTION_OPCODE = OpCodes.Callvirt;
    internal static readonly object INJECTION_OPERAND = Reflect.Method((GameObject t) => t.SendMessage(default, default, default));

    public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
    {
        foreach (CodeInstruction instruction in instructions)
        {
            yield return instruction;

            if (instruction.opcode.Equals(INJECTION_OPCODE) && instruction.operand.Equals(INJECTION_OPERAND))
            {
                yield return original.Ldloc<PDAScanner.Result>();
                yield return new(OpCodes.Ldloc_S, original.GetLocalVariableIndex<bool>(4));
                yield return new(OpCodes.Call, ((Action<PDAScanner.Result, bool>)Callback).Method);
            }
        }
    }

    // To determine which exact path the code took, we just need those two parameters
    public static void Callback(PDAScanner.Result result, bool alreadyComplete)
    {
        PDAScanner.ScanTarget scanTarget = PDAScanner.scanTarget;

        TechType techType = scanTarget.techType;
        PDAScanner.EntryData entryData = PDAScanner.GetEntryData(techType);
        if (entryData == null || (result != PDAScanner.Result.Done && result != PDAScanner.Result.Researched))
        {
            return;
        }

        // In the case the player has already fully researched the target
        if (alreadyComplete)
        {
            // Only broadcast destruction when the game itself destroys the target (destroyAfterScan). An already-complete
            // but non-destroyable target (e.g. a re-scanned creature whose encyclopedia entry is still missing) must NOT
            // be destroyed on remote clients/server, which hard-coding destroy=true here previously caused.
            if (scanTarget.hasUID)
            {
                PDAScanFinished packet = new(new(scanTarget.uid), techType.ToDto(), entryData.totalFragments, true, entryData.destroyAfterScan, true);
                Resolve<IPacketSender>().Send(packet);
            }
            return;
        }
        else
        {
            NitroxId targetId = scanTarget.hasUID ? new(scanTarget.uid) : null;

            // Case in which the unlocked progress is incremented
            if (result == PDAScanner.Result.Done)
            {
                if (!PDAScanner.GetPartialEntryByKey(techType, out PDAScanner.Entry entry))
                {
                    // Something wrong happened
                    return;
                }
                PDAScanFinished packet = new(targetId, techType.ToDto(), entry.unlocked, false, entryData.destroyAfterScan);
                Resolve<IPacketSender>().Send(packet);
            }
            // Case in which the scanned entry is fully unlocked
            else if (result == PDAScanner.Result.Researched)
            {
                PDAScanFinished packet = new(targetId, techType.ToDto(), entryData.totalFragments, true, entryData.destroyAfterScan);
                Resolve<IPacketSender>().Send(packet);
            }
        }
    }
}
