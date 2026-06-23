using System;
using Game.ObjectInfoDataScripts;
using HarmonyLib;
using UnityEngine;

namespace PopulationScaling.Patches;

[HarmonyPatch(typeof(ObjectInfoData), "OnEachYearPopulationGrowth")]
static class PopulationGrowthPatch {
    [HarmonyPrefix]
    static bool Prefix(ObjectInfoData __instance) {
        if (!Plugin.PopulationScalingEnabled.Value) { return true; }
        try {
            double pop = __instance.crewResource.Value;

            if (pop < Plugin.PopMinPopulation.Value) { return false; }

            var (inHab, capacity) = __instance.GetPopulationHabitats();
            double available = capacity > 0 ? Clamp01(1.0 - (double)inHab / capacity) : 0.0;

            double max = Plugin.PopMaxRate.Value, min = Plugin.PopMinRate.Value;
            double plateau = Plugin.PopPlateauAvailableFraction.Value;
            double housingRate = available >= plateau
                ? max
                : min + (max - min) * (plateau > 0 ? available / plateau : 0.0);

            double demand = __instance.GetSupplyDemandPerDay();
            double supplyFactor;
            if (demand <= 0.0 || __instance.supplyResource == null) {
                supplyFactor = 1.0; // no consumption (or no supply resource yet) -> always fed
            } else {
                supplyFactor = Clamp01((__instance.supplyResource.Value / demand) / Plugin.PopSupplyBufferDays.Value);
            }

            double rate = housingRate * supplyFactor;
            double d = rate * pop;

            int floor = Mathd.FloorToInt(d);
            int births = floor + (UnityEngine.Random.Range(0f, 1f) < d - floor ? 1 : 0); // vanilla stochastic

            __instance.crewResource.Value = Math.Max(0.0, pop + births);
            return false;
        } catch (Exception e) {
            Plugin.Log.LogWarning($"PopulationScaling failed; falling back to vanilla growth: {e}");
            return true;
        }
    }

    static double Clamp01(double x) => x < 0 ? 0 : x > 1 ? 1 : x;
}
