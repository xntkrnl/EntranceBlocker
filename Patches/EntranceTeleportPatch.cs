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
        [HarmonyPostfix, HarmonyPatch(typeof(EntranceTeleport), nameof(EntranceTeleport.FindExitPoint))]
        static void FindExitPointPostfix(ref bool __result, EntranceTeleport __instance)
        {
            var value = __result && EntranceBlockerNO.CheckEntrances(__instance);
            EntranceBlockerPlugin.mls.LogInfo($"__result = {__result}, value = {value}, check = {EntranceBlockerNO.CheckEntrances(__instance)}");
            __result = value;
            //IM IDIOT,ENTRANCETELEPORT TRYING TO FIND SHIT IN UPDATE()
        }
    }
}
