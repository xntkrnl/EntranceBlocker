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

        public static void AddEntrances(EntranceTeleport entrance, EntranceTeleport exit) => EntranceBlockerPlugin.networkManager.AddEntrancesToDictionary(entrance, exit);

        private void AddEntrancesToDictionary(EntranceTeleport entrance, EntranceTeleport exit) => entranceTeleports.TryAdd(entrance, exit);
    }
}
