using HarmonyLib;
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
        }
    }
}
