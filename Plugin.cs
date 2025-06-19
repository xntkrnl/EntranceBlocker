using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using EntranceBlocker.Compatibility.EntranceTeleportOptimizations;
using EntranceBlocker.Components;
using EntranceBlocker.Patches;
using HarmonyLib;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace EntranceBlocker
{
    [BepInDependency(EntranceTeleportOptimizations.MyPluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(modGUID, modName, modVersion)]
    public class EntranceBlockerPlugin : BaseUnityPlugin
    {
        public const string modGUID = "mborsh.EntranceBlocker";
        public const string modName = "EntranceBlocker";
        public const string modVersion = "1.0.0";

        public static EntranceBlockerPlugin Instance = null!;
        internal static ManualLogSource mls = null!;
        internal static readonly Harmony harmony = new Harmony(modGUID);

        internal static AssetBundle assetBundle = null!;
        internal static string assemblyLocation = null!;

        internal static GameObject networkManagerPrefab = null!;
        internal static GameObject entranceBlockerPrefab = null!;
        internal static EntranceBlockerNO networkManager = null!;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;

            mls = Logger;

            var cfg = new ConfigFile(Path.Combine(Paths.ConfigPath, $"{modGUID}.cfg"), true);
            EBConfig.CreateConfig(cfg);

            assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            assetBundle = AssetBundle.LoadFromFile(Path.Combine(assemblyLocation, "entranceblockerassetbundle"));

            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("mattymatty.EntranceTeleportOptimizations"))
                harmony.PatchAll(typeof(mattymattyEntranceTeleportPatch));
            else
            {
                mls.LogWarning("You are trying to run this mod without EntranceTeleportOptimizations. Expect lags.");
                harmony.PatchAll(typeof(EntranceTeleportPatch));
            }

            harmony.PatchAll(typeof(EntranceTeleportMainPatch));
            harmony.PatchAll(typeof(GameNetworkManagerPatch));
            NetcodePatcher();
        }

        private static void NetcodePatcher()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
        }
    }
}
