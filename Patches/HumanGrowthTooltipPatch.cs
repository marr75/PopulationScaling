using System;
using Game.ObjectInfoDataScripts;
using HarmonyLib;
using Language;
using PopulationScaling.Core;
using ScriptableObjectScripts;

namespace PopulationScaling.Patches;

// Replaces vanilla's misleading GetBirthChance() percentage on the Human-resource tooltip with the mod's
// own projected growth and its factors. Gated in-body on the master toggle (like the growth patch) so the
// tooltip stays truthful and consistent with actual growth under live config changes.
[HarmonyPatch(typeof(ResourceDefinition), nameof(ResourceDefinition.GetTooltipText))]
static class HumanGrowthTooltipPatch {
    [HarmonyPostfix]
    static void Postfix(ResourceDefinition __instance, ObjectInfoData objectInfoData, ref string __result) {
        if (!Services.Config.Enabled.Value) { return; }
        if (__instance.resourceType != ResourceDefinition.EResourceType.Human || objectInfoData == null) { return; }
        try {
            var block = Format(GrowthMath.Estimate(objectInfoData));
            var label = LEManager.Get("id_resource_human_BirthChance");
            var idx = string.IsNullOrEmpty(label) ? -1 : __result.LastIndexOf(label, StringComparison.Ordinal);
            var head = idx >= 0 ? __result.Substring(0, idx).TrimEnd() : __result.TrimEnd();
            __result = head + "\n" + block;
        }
        catch (Exception e) {
            Plugin.Log.LogWarning($"PopulationScaling tooltip patch failed: {e}");
        }
    }

    static string Format(GrowthEstimate est) {
        if (!est.Eligible) {
            return $"<b>Projected growth</b> +0 / yr  (pop below {Services.Config.MinPopulation.Value})";
        }
        var births = (int)est.ExpectedBirths;
        var baseRate = Services.Config.MaxRate.Value;
        var housingFactor = baseRate > 0.0 ? est.HousingRate / baseRate : 0.0;
        return $"<b>Projected growth</b> +{births} / yr\n"
            + $"{baseRate * 100.0:0}% x {housingFactor:0.0} {GameIcons.Habitat} x "
            + $"{est.SupplyFactor:0.0} {GameIcons.Supply} x {est.CommModifier:0.0} {GameIcons.CommSat}";
    }
}
