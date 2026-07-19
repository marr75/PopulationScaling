using BepInEx.Configuration;

namespace PopulationScaling.Config;

sealed class Configuration {
    public readonly ConfigEntry<bool> DebugLogging;
    public readonly ConfigEntry<bool> Enabled;
    public readonly ConfigEntry<double> MaxRate;
    public readonly ConfigEntry<int> MinPopulation;
    public readonly ConfigEntry<double> MinRate;
    public readonly ConfigEntry<double> PlateauAvailableFraction;
    public readonly ConfigEntry<double> SupplyBufferDays;

    public Configuration(ConfigFile c) {
        const string enabledDescription =
            "Turn this mod's growth model on or off. Off = colonies grow exactly like vanilla.";
        Enabled = c.Bind("PopulationScaling", "Enabled", true, enabledDescription);

        const string maxRateDescription =
            "The fastest a colony can grow in a year (as a fraction of its current population) when it "
            + "has plenty of empty housing and stored supply. 0.10 means up to 10% growth a year at best.";
        MaxRate = c.Bind("PopulationScaling", "MaxRate", 0.10, maxRateDescription);

        const string minRateDescription =
            "The slowest a colony can grow once housing is nearly full or stored supply is nearly out. "
            + "0 means growth tapers all the way down to nothing near capacity (no overcrowding); a "
            + "small positive number keeps a trickle of growth going even near the limit.";
        MinRate = c.Bind("PopulationScaling", "MinRate", 0.0, minRateDescription);

        const string plateauAvailableFractionDescription =
            "How much of a colony's housing needs to be empty (as a fraction of total capacity) before "
            + "growth hits its fastest rate (MaxRate). Below this threshold, growth eases off the closer "
            + "housing gets to full. 0.50 means half the housing must be empty for max-speed growth.";
        PlateauAvailableFraction = c.Bind(
            "PopulationScaling",
            "PlateauAvailableFraction",
            0.50,
            plateauAvailableFractionDescription
        );

        const string supplyBufferDaysDescription =
            "How many days' worth of stored supply (food/consumables) a colony needs banked to grow at "
            + "full speed. Falling short of that buffer proportionally slows growth, so a colony living "
            + "hand-to-mouth grows more cautiously. 365 means a full year of buffer stock is needed for "
            + "unslowed growth.";
        SupplyBufferDays = c.Bind("PopulationScaling", "SupplyBufferDays", 365.0, supplyBufferDaysDescription);

        const string minPopulationDescription =
            "The smallest a colony can be and still grow at all. 0 (the default) lets even a brand-new "
            + "outpost grow; vanilla's built-in floor was 100 crew, meaning small colonies never grew on "
            + "their own.";
        MinPopulation = c.Bind("PopulationScaling", "MinPopulation", 0, minPopulationDescription);

        const string debugLoggingDescription =
            "Writes one log line per colony every time this mod's growth calculation runs, showing the "
            + "housing and supply factors that produced the result. Only useful for reporting a bug "
            + "about growth behaving oddly.";
        DebugLogging = c.Bind("Debug", "DebugLogging", false, debugLoggingDescription);
    }
}
