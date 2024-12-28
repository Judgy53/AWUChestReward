using BepInEx.Configuration;

namespace AWUChestReward
{
    public class AWUConfig
    {
        public enum ScalingMode
        {
            None,
            StageStart,
            OnKill
        }

        public static ConfigEntry<bool> Enabled;
        public static ConfigEntry<int> ChestCost;
        public static ConfigEntry<ScalingMode> ChestCostScalingMode;
        public static ConfigEntry<bool> ScaleWithPlayerCount;

        public static void Init(ConfigFile config)
        {
            Enabled = config.Bind("Configuration", "Enabled", true,
                "True: mod is enabled and AWU will summon a chest on death.\nFalse: mod is disabled and default behaviour is restored.");

            ChestCost = config.Bind("Chest Cost", "Base Cost", 0,
                "Cost of the chest. \nDefault Legendary chest cost is 400.\n0 is free.");
            ChestCostScalingMode = config.Bind("Chest Cost", "Scaling Mode", ScalingMode.StageStart,
                "Defines how the cost of chest scales :\n- None: Cost doesn't scale at all.\nStageStart: Cost scales with the difficulty at the start of the stage.\nOnKill: Cost scales with the difficulty at the time AWU is killed.");

            ScaleWithPlayerCount = config.Bind("Multiplayer", "Scale With Player Count", true,
                "True: spawns one chest for each player.\nFalse: spawns only one chest disregarding the amount of players.");

            if (RiskOfOptionsCompatibility.Enabled)
                RiskOfOptionsCompatibility.AddConfig();
        }
    }
}
