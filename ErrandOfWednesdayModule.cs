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
            On.Celeste.Level.LoadLevel += uniqueGliderHook;
        }

        public override void Unload() {
            On.Celeste.Level.LoadLevel -= uniqueGliderHook;
        }

        private void uniqueGliderHook(On.Celeste.Level.orig_LoadLevel orig, 
                                        Level level, Player.IntroTypes playerIntro, 
                                        bool isFromLoader)
        {
            orig(level, playerIntro, isFromLoader);
            Player player = level.Tracker.GetEntity<Player>();
            UniqueGlider nearest = null;
            Glider held = null;
            float best_dist = -1;
            if (player != null)
            {
                //Find the nearest non-held unique jellyfish
                foreach(UniqueGlider e in level.Tracker.GetEntities<UniqueGlider>())
                {
                    if(player.Holding != null && player.Holding == e.Hold)
                    {
                        held = e;
                    }
                    else if(level.IsInBounds(e))
                    {
                    
                        float dist = Vector2.DistanceSquared(player.Position, e.Position);
                        if(dist < best_dist || best_dist < 0)
                        {
                            best_dist = dist;
                            nearest = e;
                        }
                    }
                }

                //determine which one to keep
                if(nearest != null && held != null)
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

                //remove all the rest
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
