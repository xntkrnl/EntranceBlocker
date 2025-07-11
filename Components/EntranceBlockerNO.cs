using EntranceBlocker.Utils;
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

            var globalPosition = UtilsStuff.DoMath(__instance); //globalposition for lookat()
            gameobject.transform.position = __instance.transform.position; //position for lookat()
            gameobject.transform.LookAt(globalPosition);

            gameobject.transform.position = UtilsStuff.DoRaycast(__instance); //true position

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

        public static bool CheckEntrances(EntranceTeleport entrance)
        {
            //EntranceBlockerPlugin.mls.LogInfo("CheckEntrances called!");
            if (!EntranceBlockerPlugin.networkManager.entranceTeleports.TryGetValue(entrance, out EntranceTeleport exit))
                return false;
            return entrance.triggerScript.interactable && exit.triggerScript.interactable;
        }
    }
}
