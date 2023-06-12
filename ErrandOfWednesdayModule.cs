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
            On.Celeste.Level.LoadLevel += onLoadLevel;
        }

        public override void Unload() {
            On.Celeste.Level.LoadLevel -= onLoadLevel;
            // TODO: unapply any hooks applied in Load()
        }

        private void onLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level level, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            orig(level, playerIntro, isFromLoader);
            Player player = level.Tracker.GetEntity<Player>();
            UniqueGlider nearest = null;
            Glider held = null;
            float best_dist = -1;
            if (player != null)
            {
                foreach(UniqueGlider e in level.Tracker.GetEntities<UniqueGlider>())
                {
                    if(player.Holding != null && player.Holding == e.Hold)
                    {
                        held = e;
                    }
                    else
                    {
                        float dist = Vector2.DistanceSquared(player.Position, e.Position);
                        if(dist < best_dist || best_dist < 0)
                        {
                            best_dist = dist;
                            nearest = e;
                        }
                    }
                }

                if(nearest != null)
                {
                    if(held != null)
                    {
                        if(nearest.confiscate)
                        {
                            player.Drop();
                            level.Remove(held);
                            held = null;
                        }
                        else
                        {
                            nearest = null;
                        }
                    }
                }

                foreach(UniqueGlider e in level.Tracker.GetEntities<UniqueGlider>())
                {
                    if(e != nearest && e != held)
                    {
                        level.Remove(e);
                    }
                } 
               
            }
        }

    }
}
