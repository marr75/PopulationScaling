using System;
using Game.ObjectInfoDataScripts;
using HarmonyLib;
using PopulationScaling.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PopulationScaling.Patches;

[HarmonyPatch(typeof(ObjectInfoData), "OnEachYearPopulationGrowth")]
static class PopulationGrowthPatch {
    [HarmonyPrefix]
    static bool Prefix(ObjectInfoData __instance) {
        if (!Services.Config.Enabled.Value) { return true; }
        try {
            var est = GrowthMath.Estimate(__instance);
            if (!est.Eligible) {
                Log(__instance, est.Pop, 0, 0, 0, 0, 0, 0, 0, "min-pop-gated");
                return false;
            }

            var d = est.ExpectedBirths;
            var floor = Mathd.FloorToInt(d);
            var births = floor + (Random.Range(0f, 1f) < d - floor ? 1 : 0); // vanilla stochastic

            Log(__instance, est.Pop, est.HousingRate, est.SupplyFactor, est.Rate, est.CommModifier, d, floor, births, "skip-vanilla");
            __instance.crewResource.Value = Math.Max(0.0, est.Pop + births);
            return false;
        }
        catch (Exception e) {
            Plugin.Log.LogWarning($"PopulationScaling failed; falling back to vanilla growth: {e}");
            return true;
        }
    }

    static void Log(
        ObjectInfoData inst,
        double pop,
        double housingRate,
        double supplyFactor,
        double rate,
        double growthModifier,
        double d,
        int floor,
        int births,
        string decision
    ) {
        if (!Services.Config.DebugLogging.Value) { return; }
        Plugin.Log.LogInfo(
            $"[grow] {Identity(inst)} pop={pop:F0} minPop={Services.Config.MinPopulation.Value} "
            + $"housingRate={housingRate:F3} supplyFactor={supplyFactor:F3} rate={rate:F3} "
            + $"growthMod={growthModifier:F3} d={d:F3} floor={floor} births={births} -> {decision}"
        );
    }

    static string Identity(ObjectInfoData inst) {
        try {
            var company = inst.company != null ? inst.company.ID : "?";
            var obj = inst.objectInfo != null ? inst.objectInfo.ObjectName : "?";
            return $"{company}/{obj}";
        }
        catch { return "?"; }
    }
}
