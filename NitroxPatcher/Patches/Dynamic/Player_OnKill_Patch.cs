using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Player_OnKill_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Player t) => t.OnKill(default(DamageType)));

    private static readonly MethodInfo SKIP_METHOD = Reflect.Method(() => GameModeUtils.IsPermadeath());

    public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> instructionList = instructions.ToList();
        // Forces GameModeUtils.IsPermadeath() to false inside OnKill so the local client skips
        // SaveLoadManager.ClearSlotAsync (the only thing in OnKill's permadeath branch); ResetPlayerOnDeath still ends the game.
        for (int i = 0; i < instructionList.Count; i++)
        {
            CodeInstruction instr = instructionList[i];

            if (instr.opcode == OpCodes.Call && instr.operand.Equals(SKIP_METHOD))
            {
                CodeInstruction newInstr = new(OpCodes.Ldc_I4_0);
                newInstr.labels = instr.labels;
                yield return newInstr;
            }
            else
            {
                yield return instr;
            }
        }
    }

    // Player.OnKill always runs on death, whereas playerDeathEvent (which PlayerDeathBroadcaster listens on) is NOT
    // triggered in permadeath/hardcore: Player.ResetPlayerOnDeath calls EndGame() and yield-breaks before Trigger.
    // Broadcasting here ensures remote players are notified and the server marks hardcore players dead in every mode.
    public static void Postfix(Player __instance)
    {
        Resolve<LocalPlayer>().BroadcastDeath(__instance.transform.position);
    }
}
