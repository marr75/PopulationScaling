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
            var pop = __instance.crewResource.Value;

            if (pop < Plugin.PopMinPopulation.Value) {
                Log(__instance, pop, 0, 0, 0, 0, 0, 0, "min-pop-gated");
                return false;
            }

            var (inHab, capacity) = __instance.GetPopulationHabitats();
            var available = capacity > 0 ? Clamp01(1.0 - (double)inHab / capacity) : 0.0;

            var max = Plugin.PopMaxRate.Value;
            var min = Plugin.PopMinRate.Value;
            var plateau = Plugin.PopPlateauAvailableFraction.Value;
            var housingRate = available >= plateau
                ? max
                : min + (max - min) * (plateau > 0 ? available / plateau : 0.0);

            var demand = __instance.GetSupplyDemandPerDay();
            double supplyFactor;
            if (demand <= 0.0 || __instance.supplyResource == null) {
                supplyFactor = 1.0; // no consumption (or no supply resource yet) -> always fed
            }
            else {
                supplyFactor = Clamp01((__instance.supplyResource.Value / demand) / Plugin.PopSupplyBufferDays.Value);
            }

            var rate = housingRate * supplyFactor;
            var d = rate * pop;

            var floor = Mathd.FloorToInt(d);
            var births = floor + (UnityEngine.Random.Range(0f, 1f) < d - floor ? 1 : 0); // vanilla stochastic

            Log(__instance, pop, housingRate, supplyFactor, rate, d, floor, births, "skip-vanilla");
            __instance.crewResource.Value = Math.Max(0.0, pop + births);
            return false;
        }
        catch (Exception e) {
            Plugin.Log.LogWarning($"PopulationScaling failed; falling back to vanilla growth: {e}");
            return true;
        }
    }

    static void Log(
        ObjectInfoData inst, double pop, double housingRate, double supplyFactor,
        double rate, double d, int floor, int births, string decision) {
        if (!Plugin.DebugLogging.Value) { return; }
        Plugin.Log.LogInfo(
            $"[grow] {Identity(inst)} pop={pop:F0} minPop={Plugin.PopMinPopulation.Value} " +
            $"housingRate={housingRate:F3} supplyFactor={supplyFactor:F3} rate={rate:F3} " +
            $"d={d:F3} floor={floor} births={births} -> {decision}");
    }

    static string Identity(ObjectInfoData inst) {
        try {
            var company = inst.company != null ? inst.company.ID : "?";
            var obj = inst.objectInfo != null ? inst.objectInfo.ObjectName : "?";
            return $"{company}/{obj}";
        }
        catch { return "?"; }
    }

    static double Clamp01(double x) => x < 0 ? 0 : x > 1 ? 1 : x;
}
