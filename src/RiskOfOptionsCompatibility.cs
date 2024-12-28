using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using System.Runtime.CompilerServices;

namespace AWUChestReward
{
    internal class RiskOfOptionsCompatibility
    {
        private static bool? _enabled;

        public static bool Enabled
        {
            get
            {
                _enabled ??= BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions");
                return (bool)_enabled;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void AddConfig()
        {
            ModSettingsManager.AddOption(new CheckBoxOption(AWUConfig.Enabled));
            ModSettingsManager.AddOption(new IntSliderOption(AWUConfig.ChestCost, new IntSliderConfig() { min = 0, max = 1000}));
            ModSettingsManager.AddOption(new ChoiceOption(AWUConfig.ChestCostScalingMode));
            ModSettingsManager.AddOption(new CheckBoxOption(AWUConfig.ScaleWithPlayerCount));
        }
    }
}
