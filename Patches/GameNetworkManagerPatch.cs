using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace EntranceBlocker.Patches
{
    internal class GameNetworkManagerPatch
    {
        //i hope testaccount dont mind that i just copy-paste network stuff from doorbreach because im lazy

        [HarmonyPostfix, HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.Start))]
        static void GameNetworkManagerStartPatch()
        {
            EntranceBlockerPlugin.entranceBlockerPrefab = EntranceBlockerPlugin.assetBundle.LoadAsset<GameObject>("planks.prefab");
            EntranceBlockerPlugin.networkManagerPrefab = EntranceBlockerPlugin.assetBundle.LoadAsset<GameObject>("nobject.prefab");
            NetworkManager.Singleton.AddNetworkPrefab(EntranceBlockerPlugin.networkManagerPrefab);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.Disconnect))]
        public static void GameNetworkManagerDisconnectPatch(GameNetworkManager __instance)
        {
            if (!__instance.isHostingGame)
            {
                EntranceBlockerPlugin.networkManager = null!;
                return;
            }

            EntranceBlockerPlugin.networkManager.NetworkObject.Despawn();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.SetLobbyJoinable))]
        public static void GameNetworkManagerSetLobbyJoinablePatch(GameNetworkManager __instance)
        {
            if (!__instance.isHostingGame) return;

            if (EntranceBlockerPlugin.networkManager && EntranceBlockerPlugin.networkManager.NetworkObject)
            {
                EntranceBlockerPlugin.mls.LogDebug("Network manager already exists! Destroying...");
                EntranceBlockerPlugin.networkManager.NetworkObject.Despawn();
            }

            var networkManagerObject = Object.Instantiate(EntranceBlockerPlugin.networkManagerPrefab);
            var networkObject = networkManagerObject.GetComponent<NetworkObject>();
            networkObject.Spawn();
            Object.DontDestroyOnLoad(networkManagerObject);
        }
    }
}
