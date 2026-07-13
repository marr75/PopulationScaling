using PopulationScaling.Config;

namespace PopulationScaling.Core;

static class Services {
    internal static Configuration Config { get; private set; } = null!;
    internal static void Init(Configuration config) { Config = config; }
}
