using Game.ObjectInfoDataScripts;

namespace PopulationScaling.Core;

readonly struct GrowthEstimate {
    public readonly bool Eligible;
    public readonly double Pop;
    public readonly double HousingRate;
    public readonly double SupplyFactor;
    public readonly double Rate;
    public readonly double CommModifier;
    public readonly double ExpectedBirths;

    public GrowthEstimate(
        bool eligible,
        double pop,
        double housingRate,
        double supplyFactor,
        double rate,
        double commModifier,
        double expectedBirths
    ) {
        Eligible = eligible;
        Pop = pop;
        HousingRate = housingRate;
        SupplyFactor = supplyFactor;
        Rate = rate;
        CommModifier = commModifier;
        ExpectedBirths = expectedBirths;
    }
}

// Single source of the mod's growth math, shared by the growth patch and the tooltip so they can never
// disagree. Returns the expected (pre-rounding) births; callers do the stochastic round.
static class GrowthMath {
    public static GrowthEstimate Estimate(ObjectInfoData inst) {
        var pop = inst.crewResource.Value;
        if (pop < Services.Config.MinPopulation.Value) {
            return new GrowthEstimate(false, pop, 0, 0, 0, 1, 0);
        }

        var (inHab, capacity) = inst.GetPopulationHabitats();
        var available = capacity > 0 ? Clamp01(1.0 - (double)inHab / capacity) : 0.0;

        var max = Services.Config.MaxRate.Value;
        var min = Services.Config.MinRate.Value;
        var plateau = Services.Config.PlateauAvailableFraction.Value;
        var housingRate = available >= plateau
            ? max
            : min + (max - min) * (plateau > 0 ? available / plateau : 0.0);

        var demand = inst.GetSupplyDemandPerDay();
        double supplyFactor;
        if (demand <= 0.0 || inst.supplyResource == null) {
            supplyFactor = 1.0; // no consumption (or no supply resource yet) -> always fed
        }
        else {
            supplyFactor = Clamp01(inst.supplyResource.Value / demand / Services.Config.SupplyBufferDays.Value);
        }

        var rate = housingRate * supplyFactor;
        var commModifier = CommModifier(inst, pop);
        var d = rate * pop;
        if (d > 0.0) { d *= commModifier; }

        return new GrowthEstimate(true, pop, housingRate, supplyFactor, rate, commModifier, d);
    }

    // Fix on: our own per-body bonus from this colony's coverage, replacing the vanilla accumulator so it
    // is never double-applied. Off: defer to the (currently no-op) vanilla facility modifiers.
    static double CommModifier(ObjectInfoData inst, double pop) {
        if (!Services.Config.CommSatGrowthFix.Value) {
            inst.UpdateFacilityRelatedSummaries(false, true);
            return inst.accumNaturalGrowthModifiers;
        }
        var coverage = pop > 0.0 ? Clamp01(inst.CurrentCommunicationHumanAmount / pop) : 0.0;
        return 1.0 + coverage * Services.Config.CommSatGrowthBonus.Value;
    }

    static double Clamp01(double x) => x < 0 ? 0 : x > 1 ? 1 : x;
}
