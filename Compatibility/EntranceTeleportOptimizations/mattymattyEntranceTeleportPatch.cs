using EntranceTeleportOptimizations.Patches;
using HarmonyLib;

namespace EntranceBlocker.Compatibility.EntranceTeleportOptimizations
{
    internal class mattymattyEntranceTeleportPatch
    {
        [HarmonyPostfix, HarmonyPatch(typeof(EntranceTeleport), nameof(EntranceTeleport.FindExitPoint))]
        static void FindExitPointPostfix(ref bool __result, EntranceTeleport __instance)
        {
            EntranceTeleportPatches.TeleportMap.TryGetValue(__instance, out EntranceTeleport exit);
            __result = __result && __instance.triggerScript.interactable && exit.triggerScript.interactable;
        }
    }
}
