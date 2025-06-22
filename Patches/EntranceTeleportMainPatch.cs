using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace EntranceBlocker.Patches
{
    internal class EntranceTeleportMainPatch
    {
        [HarmonyPostfix, HarmonyPatch(typeof(EntranceTeleport), nameof(EntranceTeleport.Awake))]
        static void EntranceTeleportAwakePatch(EntranceTeleport __instance)
        {
            //todo: run this code only for host and spawn blockers in client rpc
            if (!GameNetworkManager.Instance.isHostingGame) return;

            if (__instance.isEntranceToBuilding && __instance.entranceId == 0 && (EBConfig.blacklistedMoonsList.Contains(StartOfRound.Instance.currentLevel.PlanetName) || !EBConfig.blockOutsideMainEntrance.Value)) return;
            if (__instance.isEntranceToBuilding && __instance.entranceId != 0 && (EBConfig.blacklistedMoonsList.Contains(StartOfRound.Instance.currentLevel.PlanetName) || !EBConfig.blockOutsideFireExit.Value)) return;
            if (!__instance.isEntranceToBuilding && __instance.entranceId == 0 && (EBConfig.blacklistedMoonsInsideList.Contains(StartOfRound.Instance.currentLevel.PlanetName) || !EBConfig.blockInsideMainEntrance.Value)) return;
            if (!__instance.isEntranceToBuilding && __instance.entranceId != 0 && (EBConfig.blacklistedMoonsInsideList.Contains(StartOfRound.Instance.currentLevel.PlanetName) || !EBConfig.blockInsideFireExit.Value)) return;

            if (Random.Range(0f, 1f) > EBConfig.blockChance.Value) return;

            EntranceBlockerPlugin.networkManager.StartCoroutine(EntranceBlockerPlugin.networkManager.WaitForNetworkSpawnServer(__instance));
        }
    }
}
