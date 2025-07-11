using EntranceBlocker.Components;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using Unity.Netcode;
using UnityEngine;

namespace EntranceBlocker.Patches
{
    internal class EntranceTeleportMainPatch
    {
        [HarmonyPostfix, HarmonyPatch(typeof(EntranceTeleport), nameof(EntranceTeleport.Awake))]
        static void EntranceTeleportAwakePatch(EntranceTeleport __instance)
        {
            if (!GameNetworkManager.Instance.isHostingGame) return;

            if (__instance.isEntranceToBuilding && __instance.entranceId == 0 && (EBConfig.blacklistedMoonsList.Contains(StartOfRound.Instance.currentLevel.PlanetName) || !EBConfig.blockOutsideMainEntrance.Value)) return;
            if (__instance.isEntranceToBuilding && __instance.entranceId != 0 && (EBConfig.blacklistedMoonsList.Contains(StartOfRound.Instance.currentLevel.PlanetName) || !EBConfig.blockOutsideFireExit.Value)) return;
            if (!__instance.isEntranceToBuilding && __instance.entranceId == 0 && (EBConfig.blacklistedMoonsInsideList.Contains(StartOfRound.Instance.currentLevel.PlanetName) || !EBConfig.blockInsideMainEntrance.Value)) return;
            if (!__instance.isEntranceToBuilding && __instance.entranceId != 0 && (EBConfig.blacklistedMoonsInsideList.Contains(StartOfRound.Instance.currentLevel.PlanetName) || !EBConfig.blockInsideFireExit.Value)) return;

            if (Random.Range(0f, 1f) > EBConfig.blockChance.Value) return;

            EntranceBlockerPlugin.networkManager.StartCoroutine(EntranceBlockerPlugin.networkManager.WaitForNetworkSpawnServer(__instance));
        }

        [HarmonyPrefix, HarmonyPatch(typeof(NetworkBehaviour), nameof(NetworkBehaviour.OnDestroy))]
        private static void OnTeleportDestroy(NetworkBehaviour __instance)
        {
            if (__instance is not EntranceTeleport teleport)
                return;

            if (EntranceBlockerPlugin.networkManager.blockersDict.TryGetValue(teleport.NetworkObjectId, out var blocker))
                GameObject.Destroy(blocker.gameObject);

            EntranceBlockerPlugin.networkManager.entranceTeleportsBoolMap.Remove(teleport);
            EntranceBlockerPlugin.networkManager.entranceTeleports.Remove(teleport);
        }

        [HarmonyBefore(EntranceTeleportOptimizations.MyPluginInfo.PLUGIN_GUID)]
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

        [HarmonyPrefix, HarmonyPatch(typeof(NetworkBehaviour), nameof(NetworkBehaviour.OnNetworkSpawn))]
        private static void OnTeleportSpawn(NetworkBehaviour __instance)
        {
            if (__instance is not EntranceTeleport teleport)
                return;

            EntranceBlockerPlugin.networkManager.entranceTeleportsBoolMap.Add(teleport, true);
        }
    }
}
