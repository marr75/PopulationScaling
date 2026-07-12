# PopulationScaling

A BepInEx 5 (Harmony) plugin for **Solar Expanse** that replaces vanilla colony
population growth with a configurable, supply-and-housing-aware model:

- **Logistic growth curve** — annual growth ramps from a peak rate down toward zero as a colony fills its housing ceiling, so colonies plateau instead of overshooting.
- **Housing headroom drives the rate** — full `MaxRate` applies while enough of the ceiling is empty (`PlateauAvailableFraction`); below that it tapers linearly to `MinRate` at capacity.
- **Supply-gated** — stored supply below `SupplyBufferDays` ramps the growth rate down proportionally, reaching zero at empty stock.
- **Grows from any size** — removes the vanilla floor that stalled growth below 100 population (`MinPopulation`, default 0).
- **Master toggle + debug logging** — disable to restore vanilla growth; optional per-tick log lines for diagnosing growth decisions.

Plugin GUID: `marr75.solarexpanse.populationscaling`

## Build

1. Set the `SOLAR_EXPANSE_DIR` environment variable to your Solar Expanse install path
   (the folder containing `Solar Expanse_Data\Managed`).
2. Run `dotnet build`.

The post-build `DeployToPlugins` target copies the DLL to
`%SOLAR_EXPANSE_DIR%\BepInEx\plugins\PopulationScaling\`, so a successful build is a deployed build.

Override the path per-build without the env var via `dotnet build -p:GameDir="..."`.

## License

License: MIT (see LICENSE)
