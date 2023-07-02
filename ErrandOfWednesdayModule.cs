using System;
using System.Collections;

using Microsoft.Xna.Framework;

using MonoMod.Utils;

using Monocle;

using Celeste;

namespace Celeste.Mod.ErrandOfWednesday {
    public class ErrandOfWednesdayModule : EverestModule {
        public static ErrandOfWednesdayModule Instance { get; private set; }

        public override Type SettingsType => typeof(ErrandOfWednesdayModuleSettings);
        public static ErrandOfWednesdayModuleSettings Settings => (ErrandOfWednesdayModuleSettings) Instance._Settings;

        public override Type SessionType => typeof(ErrandOfWednesdayModuleSession);
        public static ErrandOfWednesdayModuleSession Session => (ErrandOfWednesdayModuleSession) Instance._Session;


        public static bool lookout = false;
        public static float lookout_value = 0;
        public static bool fully_lookout = false;
        public static bool fully_not_lookout = false;

        public static bool all_lookouts = false;

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
            On.Celeste.Lookout.Update += lookout_stop;
            On.Monocle.Engine.Update += Update;
        }

        public override void Unload() {
            On.Celeste.Lookout.Update -= lookout_stop;
            On.Monocle.Engine.Update -= Update;
        }

        public void lookout_stop(On.Celeste.Lookout.orig_Update orig, Lookout self)
        { //I can't believe I have to do it like this.
            orig(self);
            if(all_lookouts) return;
            DynamicData data = new DynamicData(self);
            if(data.Get<bool>("interacting"))
            {
                Level level = self.SceneAs<Level>();
                all_lookouts = true;
                lookout_value = level.ScreenPadding/15f;
            }
        }

        public static void Update(On.Monocle.Engine.orig_Update orig, Engine self, GameTime gameTime)
        {
            all_lookouts = false;
            orig(self, gameTime);
            lookout = all_lookouts;
            if(lookout)  
            {
                fully_lookout = lookout_value == 1;
            }
        }
    }
}
