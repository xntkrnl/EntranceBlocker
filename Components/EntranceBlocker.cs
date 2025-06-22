using GameNetcodeStuff;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace EntranceBlocker.Components
{
    internal class EntranceBlocker : MonoBehaviour, IHittable
    {
        public List<GameObject> blockers = new List<GameObject>();
        internal EntranceTeleport entranceTeleport = null!;
        internal EntranceTeleport exitSideTeleport = null!;
        private bool canHit = false;

        public bool Hit(int force, Vector3 hitDirection, PlayerControllerB playerWhoHit, bool playHitSFX, int hitID)
        {
            EntranceBlockerPlugin.mls.LogInfo($"Hit! Source: {playerWhoHit.playerUsername}, canHit: {canHit}");

            if (!canHit)
                return true;

            var noRef = EntranceBlockerPlugin.networkManager.reverseBlockersDict[this];

            EntranceBlockerPlugin.networkManager.OnHitServerRpc(noRef);

            return true;
        }

        private IEnumerator WaitForNetworkSpawn()
        {
            yield return new WaitUntil(() => entranceTeleport != null);

            EntranceBlockerPlugin.networkManager.blockersDict.Add(entranceTeleport.NetworkObject.NetworkObjectId, this);
            EntranceBlockerPlugin.networkManager.reverseBlockersDict.Add(this, new NetworkObjectReference(entranceTeleport.NetworkObject));
            canHit = true;
            entranceTeleport.triggerScript.interactable = false;
        }

        void Awake()
        {
            StartCoroutine(WaitForNetworkSpawn());
        }

        void OnDestroy()
        {
            entranceTeleport.triggerScript.interactable = true;

            EntranceBlockerPlugin.mls.LogInfo("Entrance blocker destroyed");
        }
    }
}
