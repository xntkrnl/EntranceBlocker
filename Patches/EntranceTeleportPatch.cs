using EntranceBlocker.Components;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace EntranceBlocker.Patches
{
    internal class EntranceTeleportPatch
    {
        [HarmonyTranspiler, HarmonyPatch(typeof(EntranceTeleport), nameof(EntranceTeleport.FindExitPoint))]
        static IEnumerable<CodeInstruction> FindExitPointTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeMatcher = new CodeMatcher(instructions);
            var captureMethod = AccessTools.Method(typeof(EntranceBlockerNO), nameof(EntranceBlockerNO.AddEntrances));

            codeMatcher.MatchForward(
                false,
                new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(EntranceTeleport), "exitPoint")))
            .Advance(1)
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Ldelem_Ref),
                new CodeInstruction(OpCodes.Call, captureMethod)
            );

            return codeMatcher.InstructionEnumeration();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(EntranceTeleport), nameof(EntranceTeleport.FindExitPoint))]
        static void FindExitPointPostfix(ref bool __result, EntranceTeleport __instance)
        {
            EntranceBlockerPlugin.networkManager.entranceTeleports.TryGetValue(__instance, out EntranceTeleport exit);
            __result = __result && __instance.triggerScript.interactable && exit.triggerScript.interactable;
        }
    }
}
