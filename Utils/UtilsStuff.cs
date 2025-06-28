using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

namespace EntranceBlocker.Utils
{
    internal class UtilsStuff
    {
        internal static Vector3 DoMath(EntranceTeleport __instance)
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

        internal static Vector3 DoRaycast(EntranceTeleport __instance)
        {
            Vector3 start = __instance.entrancePoint.position;
            Vector3 end = __instance.transform.position;
            start.y = end.y;
            Vector3 direction = (end - start).normalized;
            float distance = Vector3.Distance(start, end);

            RaycastHit[] hits = Physics.RaycastAll(start, direction, distance, 257);
            foreach (RaycastHit hit in hits)
                if (hit.transform.gameObject.layer == 0 || hit.transform.gameObject.layer == 8)
                    return new Vector3(hit.point.x, __instance.triggerScript.GetComponent<BoxCollider>().bounds.center.y, hit.point.z);

            //failsafe
            return Vector3.Lerp(start, end, 0.85f);
        }
    }
}
