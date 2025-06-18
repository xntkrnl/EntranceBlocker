using HarmonyLib;
using UnityEngine;

namespace EntranceBlocker.Patches
{
    internal class EntranceTeleportMainPatch
    {
        [HarmonyPostfix, HarmonyPatch(typeof(EntranceTeleport), nameof(EntranceTeleport.Awake))]
        static void EntranceTeleportAwakePatch(EntranceTeleport __instance)
        {
            if (EBConfig.blacklistedMoonsList.Contains(StartOfRound.Instance.currentLevel.PlanetName)) return;

            if (__instance.isEntranceToBuilding && __instance.entranceId == 0 && !EBConfig.blockOutsideMainEntrance.Value) return;
            if (__instance.isEntranceToBuilding && __instance.entranceId != 0 && !EBConfig.blockOutsideFireExit.Value) return;
            if (!__instance.isEntranceToBuilding && __instance.entranceId == 0 && !EBConfig.blockInsideMainEntrance.Value) return;
            if (!__instance.isEntranceToBuilding && __instance.entranceId != 0 && !EBConfig.blockInsideFireExit.Value) return;

            if (Random.Range(0f, 1f) > EBConfig.blockChance.Value) return;

            var gameobject = GameObject.Instantiate(EntranceBlockerPlugin.entranceBlockerPrefab);
            gameobject.GetComponent<Components.EntranceBlocker>().entranceTeleport = __instance;

            var globalPosition = DoMath(__instance); //globalposition for lookat()
            gameobject.transform.position = __instance.transform.position; //fake position for lookat()
            gameobject.transform.LookAt(globalPosition);

            Vector3 start = __instance.entrancePoint.position;
            Vector3 end = __instance.transform.position;
            Vector3 direction = (end - start).normalized;
            float distance = Vector3.Distance(start, end);
            RaycastHit[] hits = Physics.RaycastAll(start, direction, distance);
            foreach (RaycastHit hit in hits)
                if (hit.collider.transform == __instance.transform)
                {
                    gameobject.transform.position = new Vector3(hit.point.x, gameobject.transform.position.y, hit.point.z); //true position
                    break;
                }

            gameobject.transform.localScale = new Vector3(__instance.entranceId == 0 ? 1.3f : 0.75f, 1f, 1f);

            EntranceBlockerPlugin.mls.LogInfo($"Spawn entranceBlocker!\nEntrance name: {__instance.gameObject.name}" +
                $"\nEntranceID: {__instance.entranceId}\nentranceBlocker position: {gameobject.transform.position}\nLookAt() position: {globalPosition}\n");
        }

        static Vector3 DoMath(EntranceTeleport __instance)
        {
            //in short:
            //we imagine the door as a point in the center of a circle
            //and the entry point as a point on the edge of this circle
            //and we calculate and round the angle taking into account that the door may be at an angle
            //and finally we calculate position of rounded angle (kinda bad explanation lol)

            Vector3 direction = __instance.transform.InverseTransformPoint(__instance.entrancePoint.position);
            direction.y = 0;
            float distance = direction.magnitude;
            float rawAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float snappedAngle = Mathf.Round(rawAngle / 90f) * 90f;
            Vector3 snappedDir = Quaternion.Euler(0, snappedAngle, 0) * Vector3.forward;
            snappedDir = snappedDir.normalized * distance;
            return __instance.transform.TransformPoint(snappedDir);
        }
    }
}
