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
        private bool isWaitingForSpawn = true;

        public bool Hit(int force, Vector3 hitDirection, PlayerControllerB playerWhoHit, bool playHitSFX, int hitID)
        {
            if (playerWhoHit != null)
                EntranceBlockerPlugin.mls.LogInfo($"Hit! Source: {playerWhoHit.playerUsername}, canHit: {canHit}");
            else EntranceBlockerPlugin.mls.LogInfo($"Hit! Source: NOT PLAYER, canHit: {canHit}");

            if (!canHit)
                return true;

            var noRef = EntranceBlockerPlugin.networkManager.reverseBlockersDict[this];

            EntranceBlockerPlugin.networkManager.OnHitServerRpc(noRef);

            return true;
        }

        private IEnumerator WaitForNetworkSpawn()
        {
            yield return new WaitUntil(() => entranceTeleport != null);
            EntranceBlockerPlugin.mls.LogInfo($"EntranceTeleport {entranceTeleport.gameObject.name} with id: {entranceTeleport.entranceId} is now networked and ready for block");

            EntranceBlockerPlugin.networkManager.blockersDict.Add(entranceTeleport.NetworkObjectId, this);
            EntranceBlockerPlugin.networkManager.reverseBlockersDict.Add(this, new NetworkObjectReference(entranceTeleport.NetworkObject));
            canHit = true;
            isWaitingForSpawn = false;
            entranceTeleport.triggerScript.interactable = false;
        }

        void Awake()
        {
            EntranceBlockerPlugin.mls.LogInfo($"EntranceBlocker awake!");
            StartCoroutine(WaitForNetworkSpawn());
        }

        void OnDestroy()
        {
            entranceTeleport.triggerScript.interactable = true;

            EntranceBlockerPlugin.networkManager.blockersDict.Remove(entranceTeleport.NetworkObjectId);
            EntranceBlockerPlugin.networkManager.reverseBlockersDict.Remove(this);
            EntranceBlockerPlugin.networkManager.entranceTeleports.Remove(entranceTeleport);

            EntranceBlockerPlugin.mls.LogInfo("Entrance blocker destroyed");
        }
    }
}
