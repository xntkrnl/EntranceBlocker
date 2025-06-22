using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace EntranceBlocker.Components
{
    internal class EntranceBlockerNO : NetworkBehaviour
    {
        internal Dictionary<EntranceTeleport, EntranceTeleport> entranceTeleports = new Dictionary<EntranceTeleport, EntranceTeleport>();
        internal Dictionary<ulong, EntranceBlocker> blockersDict = new Dictionary<ulong, EntranceBlocker>();
        internal Dictionary<EntranceBlocker, NetworkObjectReference> reverseBlockersDict = new Dictionary<EntranceBlocker, NetworkObjectReference>();

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            EntranceBlockerPlugin.networkManager = this;
        }
        Vector3 DoMath(EntranceTeleport __instance)
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

        [ClientRpc]
        internal void SpawnBlockerClientRpc(NetworkBehaviourReference noRef)
        {
            StartCoroutine(WaitForNetworkSpawnClient(noRef));
        }

        internal IEnumerator WaitForNetworkSpawnServer(EntranceTeleport entranceTeleport)
        {
            yield return new WaitUntil(() => entranceTeleport.NetworkObject.IsSpawned);
            SpawnBlockerClientRpc(entranceTeleport);
        }

        internal IEnumerator WaitForNetworkSpawnClient(NetworkBehaviourReference noRef)
        {
            yield return new WaitUntil(() => noRef.TryGet(out EntranceTeleport __instance));

            noRef.TryGet(out EntranceTeleport __instance);
            var gameobject = Instantiate(EntranceBlockerPlugin.entranceBlockerPrefab);
            gameobject.GetComponent<EntranceBlocker>().entranceTeleport = __instance;

            var globalPosition = DoMath(__instance); //globalposition for lookat()
            gameobject.transform.position = __instance.transform.position; //position for lookat()
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

            EntranceBlockerPlugin.mls.LogDebug($"Spawn entranceBlocker! Entrance name: {__instance.gameObject.name};" +
                $" EntranceID: {__instance.entranceId}; entranceBlocker position: {gameobject.transform.position}; LookAt() position: {globalPosition}");
        }

        [ServerRpc(RequireOwnership = false)]
        internal void OnHitServerRpc(NetworkObjectReference noRef)
        {
            var blocker = blockersDict[noRef.NetworkObjectId];

            if (blocker.blockers.Count == 1)
                DestroyClientRpc(noRef);
            else
            {
                int random = Random.Range(0, blocker.blockers.Count);
                OnHitClientRpc(random, noRef.NetworkObjectId);
            }
        }

        [ClientRpc]
        private void OnHitClientRpc(int random, ulong noID)
        {
            var blocker = blockersDict[noID];

            blocker.blockers[random].SetActive(false);
            blocker.blockers.Remove(blocker.blockers[random]);
        }

        [ClientRpc]
        private void DestroyClientRpc(NetworkObjectReference noRef)
        {
            var component = blockersDict[noRef.NetworkObjectId];
            noRef.TryGet(out NetworkObject no);

            entranceTeleports.Remove(no.GetComponent<EntranceTeleport>());
            reverseBlockersDict.Remove(component);
            blockersDict.Remove(noRef.NetworkObjectId);
            Destroy(component.gameObject);
        }

        public static void AddEntrances(EntranceTeleport entrance, EntranceTeleport exit) => EntranceBlockerPlugin.networkManager.entranceTeleports.TryAdd(entrance, exit);
    }
}
