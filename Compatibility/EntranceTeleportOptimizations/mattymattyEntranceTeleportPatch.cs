using EntranceBlocker.Components;
using EntranceTeleportOptimizations;
using EntranceTeleportOptimizations.Patches;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace EntranceBlocker.Compatibility.EntranceTeleportOptimizations
{
    internal class mattymattyEntranceTeleportPatch
    {
        public static bool CheckEntrances(EntranceTeleport entrance)
        {
            var manager = EntranceBlockerPlugin.networkManager;

            //EntranceBlockerPlugin.mls.LogInfo("CheckEntrances called!");
            bool result = true;
            bool resultlocal;
            manager.entranceTeleports = EntranceTeleportPatches.TeleportMap;
            if (!manager.entranceTeleports.TryGetValue(entrance, out var exit))
                return false;

            manager.entranceTeleportsBoolMap.TryGetValue(entrance, out resultlocal);
            result = result && resultlocal;
            manager.entranceTeleportsBoolMap.TryGetValue(exit, out resultlocal);
            result = result && resultlocal;

            return result;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(EntranceTeleport), nameof(EntranceTeleport.FindExitPoint))]
        static void FindExitPointPostfix(ref bool __result, EntranceTeleport __instance)
        {
            var value = __result && CheckEntrances(__instance);
            EntranceBlockerPlugin.mls.LogInfo($"mattymatty__result = {__result}, value = {value}, check = {EntranceBlockerNO.CheckEntrances(__instance)}");
            __result = value;
        }
    }
}
