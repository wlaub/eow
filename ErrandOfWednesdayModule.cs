using System;
using Microsoft.Xna.Framework;

using Celeste;

namespace Celeste.Mod.ErrandOfWednesday {
    public class ErrandOfWednesdayModule : EverestModule {
        public static ErrandOfWednesdayModule Instance { get; private set; }

        public override Type SettingsType => typeof(ErrandOfWednesdayModuleSettings);
        public static ErrandOfWednesdayModuleSettings Settings => (ErrandOfWednesdayModuleSettings) Instance._Settings;

        public override Type SessionType => typeof(ErrandOfWednesdayModuleSession);
        public static ErrandOfWednesdayModuleSession Session => (ErrandOfWednesdayModuleSession) Instance._Session;

        public ErrandOfWednesdayModule() {
            Instance = this;
#if DEBUG
            // debug builds use verbose logging
            Logger.SetLogLevel(nameof(ErrandOfWednesdayModule), LogLevel.Verbose);
#else
            // release builds use info logging to reduce spam in log files
            Logger.SetLogLevel(nameof(ErrandOfWednesdayModule), LogLevel.Info);
#endif
        }

        public override void Load() {
        }

        public override void Unload() {
        }

    }
}
