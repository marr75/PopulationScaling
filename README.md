<p align="center">
  <img src="banner.png" alt="Population Scaling banner">
</p>

# Population Scaling

Why should a colony need 100 crew before it grows at all? PopulationScaling removes that arbitrary floor and lets you hitch onto real logistic growth: over-provision housing and supplies well beyond what's needed and your colonies grow on their own, instead of you shipping in population by hand.

![Showing off the updated tooltip which shows how much growth to expect and what factors are influencing it](docs/images/populationscaling-tooltip.png)

Hover a colony's population and the tooltip now shows this mod's own numbers instead of vanilla's (which no longer match how the mod grows colonies). The **+N / yr** line is the population you can expect the colony to gain at its next growth tick. Below it is the multiplication that produces that number: the base growth rate, times a housing factor (0 to 1, how much empty housing there is), times a supply factor (0 to 1, how well-stocked it is), times the CommSat coverage multiplier — each factor tagged with its habitat, supply, and comm-satellite icon.

![Two charts: a colony's logistic growth curve from 25 crew to a capacity of 400, with the annual growth rate holding near 10%/yr until housing passes half full and then tapering to zero (left), and the same model's response to a +50 crew immigration shock at year 15, which dips the growth rate before it re-converges toward capacity (right)](docs/images/populationscaling-charts.png)

## What it does

- Growth speeds up when you have empty housing and slows down as it fills, a smooth "S-curve" (in math terms, a
  _logistic_ curve) instead of vanilla's flat rate. Think of it like a savings account that compounds fast when there's plenty of room to grow, then tapers off as you approach the limit, rather than overshooting and leaving people crammed in.
- Growth also depends on how much stored supply (food/consumables) a colony is sitting on. Plenty of buffer stock means full growth; a colony running low ramps growth down instead of starving itself into a death spiral.
- Small colonies grow too. Vanilla stops population growth entirely below 100 crew; this mod lets even brand-new outposts grow from any size (configurable).
- Fully reversible: turn the mod off and growth reverts to vanilla's flat-rate behavior.

## Before / after

Vanilla: population grows at a flat rate regardless of how full housing is, and can overcrowd a colony past its housing capacity. This mod: growth peaks when housing has room, tapers smoothly to zero as capacity fills, and eases off if stored supply runs thin, with no overcrowding or starvation spirals.

## Configuration

The knobs most worth touching:

- **MaxRate**: the fastest a colony can grow (as a fraction of its population per year) when it has plenty of empty housing and supply. Defaults to 5%. Raise this for a faster-paced game, lower it to slow colonies down.
- **CommSatGrowthFix**: on by default. Restores the communication-satellite growth bonus, which the base game currently reads from the satellite's own orbit (always zero coverage) so it never actually applies. On, the bonus comes from the colony's own communication coverage. Turn it off to defer to the base game's (currently no-op) behavior.
- **CommSatGrowthBonus**: how much full communication coverage speeds up growth, as a fraction. 0.10 (the default) means a fully-covered colony grows 10% faster; partial coverage scales down linearly. Coverage is colony-wide, so extra satellites only help by covering more people — they never stack.
- **PlateauAvailableFraction**: how much empty housing a colony needs (as a fraction of its total capacity) to hit MaxRate. Below that threshold, growth tapers off the closer housing gets to full. Lower this if you want colonies to keep growing fast even with less room to spare.
- **SupplyBufferDays**: how many days of stored supply a colony needs on hand to grow at full speed. Running below that many days' worth of buffer proportionally slows growth. Raise this if you want colonies to play it safer with their stockpiles before growing.

Everything else (MinRate, MinPopulation, DebugLogging) is a fine-tuning or diagnostic knob. See the full recommendations doc for details if you want to dig deeper.

The config file lives at
`BepInEx/config/marr75.solarexpanse.populationscaling.cfg` and can also be edited in-game if you have the Configuration Manager mod installed.

## Requirements

- Solar Expanse + BepInEx 5 (Mono/x64).

## Install

1. Install BepInEx 5.
2. Drop the `PopulationScaling` folder into `BepInEx/plugins/`.

## Building (developers)

`dotnet build` deploys the DLL to the game's plugins folder via the post-build target. See
`AGENTS.md` for the full build/deploy setup.
