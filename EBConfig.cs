using BepInEx.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace EntranceBlocker
{
    internal class EBConfig
    {
        internal static ConfigEntry<float> blockChance = null!;
        internal static ConfigEntry<string> blacklistedMoons = null!;
        internal static ConfigEntry<string> blacklistedMoonsInside = null!;
        internal static List<string> blacklistedMoonsList = new List<string>();
        internal static List<string> blacklistedMoonsInsideList = new List<string>();
        internal static ConfigEntry<bool> blockOutsideMainEntrance = null!;
        internal static ConfigEntry<bool> blockInsideMainEntrance = null!;
        internal static ConfigEntry<bool> blockOutsideFireExit = null!;
        internal static ConfigEntry<bool> blockInsideFireExit = null!;

        internal static void CreateConfig(ConfigFile cfg)
        {
            blockChance = cfg.Bind("General", "Chance", 0.6f, new ConfigDescription("...what's not clear? Why are you looking at this description?", new AcceptableValueRange<float>(0f, 1f)));
            blacklistedMoons = cfg.Bind("General", "Blacklisted moons - outside entrances and fire exits", "823 Bozoros,8 Titan,5 Embrion");
            blacklistedMoonsInside = cfg.Bind("General", "Blacklisted moons - inside entrances and fire exits", "");
            blockOutsideMainEntrance = cfg.Bind("General", "Block outside main entrance", false);
            blockInsideMainEntrance = cfg.Bind("General", "Block inside main entrance", false);
            blockOutsideFireExit = cfg.Bind("General", "Block outside fire exit", true);
            blockInsideFireExit = cfg.Bind("General", "Block inside fire exit", false);

            blacklistedMoonsList = blacklistedMoons.Value.Split(',').ToList();
            blacklistedMoonsInsideList = blacklistedMoonsInside.Value.Split(',').ToList();

            if (blockInsideMainEntrance.Value && blockInsideFireExit.Value)
                EntranceBlockerPlugin.mls.LogError("You tried to block all exits in the interior. This is not supported. Continue at your own risk.");
        }
    }
}
