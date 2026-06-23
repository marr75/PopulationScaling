using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace PopulationScaling;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin {
    internal static ManualLogSource Log = null!;

    // Master toggle. false => the prefix returns true and vanilla growth runs.
    internal static ConfigEntry<bool> PopulationScalingEnabled = null!;
    internal static ConfigEntry<double> PopMaxRate = null!;
    internal static ConfigEntry<double> PopMinRate = null!;
    internal static ConfigEntry<double> PopPlateauAvailableFraction = null!;
    internal static ConfigEntry<double> PopSupplyBufferDays = null!;
    internal static ConfigEntry<int> PopMinPopulation = null!;

    void Awake() {
        Log = Logger;

        PopulationScalingEnabled = Config.Bind("PopulationScaling", "Enabled", true,
            "Master toggle. False = vanilla growth (patch no-ops).");
        PopMaxRate = Config.Bind("PopulationScaling", "MaxRate", 0.10,
            "Peak annual growth fraction when free housing >= PlateauAvailableFraction.");
        PopMinRate = Config.Bind("PopulationScaling", "MinRate", 0.0,
            "Annual growth fraction at/approaching full capacity. 0 = logistic taper to zero (no overcrowding).");
        PopPlateauAvailableFraction = Config.Bind("PopulationScaling", "PlateauAvailableFraction", 0.50,
            "Fraction of the ceiling that must be empty to earn MaxRate. Below it, linear taper to MinRate at full.");
        PopSupplyBufferDays = Config.Bind("PopulationScaling", "SupplyBufferDays", 365.0,
            "Days of stored supply for full growth. Below it, the rate ramps down proportionally to zero at empty stock.");
        PopMinPopulation = Config.Bind("PopulationScaling", "MinPopulation", 0,
            "Population below which a colony does not grow. 0 = grow from any size (stock value was 100).");

        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} loaded.");
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();
    }
}
