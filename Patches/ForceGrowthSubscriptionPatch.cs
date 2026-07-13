using Game.ObjectInfoDataScripts;
using HarmonyLib;
using PopulationScaling.Core;

namespace PopulationScaling.Patches;

// The game gates subscription of OnEachYearPopulationGrowth to the year tick on
// company.Definition.PopulationConfig.minPopulationSize (stock 100): FillResourcesRows
// (initial subscribe) and OnCrewChange (re-subscribe) both skip sub-threshold colonies,
// so PopulationGrowthPatch's Prefix never runs for them. Zeroing the shared config field
// just-in-time — before either guard reads it — forces every colony to subscribe; our
// Prefix then stays the single growth floor via PopMinPopulation.
static class ForceGrowthSubscriptionPatch {
    static void ZeroMinPopulation(ObjectInfoData instance) {
        if (!Services.Config.Enabled.Value) { return; }
        var company = instance.company;
        if (company == null) { return; }
        var config = company.Definition?.PopulationConfig;
        if (config == null || config.minPopulationSize == 0) { return; }
        config.minPopulationSize = 0;
    }

    [HarmonyPatch(typeof(ObjectInfoData), "FillResourcesRows")]
    static class FillResourcesRowsPrefix {
        [HarmonyPrefix]
        static void Prefix(ObjectInfoData __instance) => ZeroMinPopulation(__instance);
    }

    [HarmonyPatch(typeof(ObjectInfoData), "OnCrewChange")]
    static class OnCrewChangePrefix {
        [HarmonyPrefix]
        static void Prefix(ObjectInfoData __instance) => ZeroMinPopulation(__instance);
    }
}
