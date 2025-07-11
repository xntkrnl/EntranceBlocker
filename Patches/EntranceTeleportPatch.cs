using EntranceBlocker.Components;
using HarmonyLib;
using Steamworks.Data;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace EntranceBlocker.Patches
{
    internal class EntranceTeleportPatch
    {
        [HarmonyTranspiler, HarmonyPatch(typeof(EntranceTeleport), nameof(EntranceTeleport.FindExitPoint))]
        static IEnumerable<CodeInstruction> FindExitPointTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeMatcher = new CodeMatcher(instructions);
            var captureMethod = AccessTools.Method(typeof(EntranceBlockerNO), nameof(EntranceBlockerNO.AddEntrances));

            //we ball
            //not*
            //var captureMethod2 = AccessTools.Method(typeof(EntranceBlockerNO), nameof(EntranceBlockerNO.CheckEntrances));

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

            /*codeMatcher.MatchForward(
                false,
                new CodeMatch(OpCodes.Ldc_I4_1))
            .RemoveInstruction()
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, captureMethod2)
            );*/

            return codeMatcher.InstructionEnumeration();
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(EntranceTeleport), nameof(EntranceTeleport.Update))]
        static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var captureMethod = AccessTools.Method(typeof(EntranceBlockerNO), nameof(EntranceBlockerNO.CheckEntrances));

            var codeMatcher = new CodeMatcher(instructions);
            codeMatcher.MatchForward(
                false,
                new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(EntranceTeleport), "FindExitPoint")))
            .RemoveInstruction()
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_0))
            .SetInstruction(
                new CodeInstruction(OpCodes.Call, captureMethod)
            );

            return codeMatcher.InstructionEnumeration();
        }


        [HarmonyPostfix, HarmonyPatch(typeof(EntranceTeleport), nameof(EntranceTeleport.FindExitPoint))]
        static void FindExitPointPostfix(ref bool __result, EntranceTeleport __instance)
        {
            if (__result == true)
                if (EntranceBlockerPlugin.networkManager.entranceTeleports.TryGetValue(__instance, out EntranceTeleport exit))
                    __result = __result && exit.triggerScript.interactable && __instance.triggerScript.interactable;// <-- somehow expensive?????????
                    //IM IDIOT,ENTRANCETELEPORT TRYING TO FIND SHIT IN UPDATE()
        }
    }
}
