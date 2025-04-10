﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using Microsoft.Xna.Framework;

using MonoMod.Utils;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.ModInterop;
using Mono.Cecil.Cil;

using Monocle;

using Celeste;

namespace Celeste.Mod.ErrandOfWednesday {

    [ModImportName("GravityHelper")]
    public static class GravityHelperImports 
    {
            public static Func<int> GetPlayerGravity;
    }



    public class Flagic
    {
        public static bool process_flag(string flag, out string flag_out)
        {

            if(!string.IsNullOrWhiteSpace(flag) && flag[0] == '!')
            {
                flag_out = flag.Substring(1);
                return true;
            }
            else
            {
                flag_out = flag;
                return false;
            }
        }
        public static bool test_flag(Session session, string flag, bool flag_inverted)
        {
            return string.IsNullOrWhiteSpace(flag) || session.GetFlag(flag) != flag_inverted;
 
        }
        public static void set_flag(Session session, string flag, bool flag_inverted)
        {
            if(!string.IsNullOrWhiteSpace(flag))
            {
                session.SetFlag(flag, !flag_inverted);
            } 
        }
    }


    public class TriggerManager 
    {
        public static Dictionary<int, EntityData> data_registry = new();

        public static void clear()
        {
            data_registry.Clear();
        }
        
        public static EntityData get_trigger(Session session, int id)
        {
            if(data_registry.ContainsKey(id))
            {
                return data_registry[id];
            }
            foreach(LevelData level_data in session.MapData.Levels)
            {
                foreach(EntityData entity_data in level_data.Triggers)
                {
                    if(id == entity_data.ID)
                    {
                        data_registry.Add(id, entity_data);
                        return entity_data;
                    }
                }
            }
            Logger.Log(LogLevel.Error, "eow", $"Did not find trigger {id}");
            return null;
        }

        public static Trigger make_trigger(Level level, int id)
        {
            EntityData entity_data = get_trigger(level.Session, id);
            if(entity_data == null)
            {
                return null;
            }

            LevelData level_data = level.Session.LevelData;
            Vector2 offset = new Vector2(level_data.Bounds.Left, level_data.Bounds.Top);
            if (Level.EntityLoaders.TryGetValue(entity_data.Name, out var value))
            {
                Entity entity = value(level, level.Session.LevelData, offset, entity_data);
                return (Trigger)entity;
            }                       
            return null; 
        }
       
    }

    public class MyLevelInspect
    {
    public static Entity make_entity(EntityData entity_data, LevelData level_data, Level level)
    {
         Vector2 offset = new Vector2(level_data.Bounds.Left, level_data.Bounds.Top);

        if (Level.EntityLoaders.TryGetValue(entity_data.Name, out var value))
        {
            Entity entity = value(level, level.Session.LevelData, offset, entity_data);
            return (Entity)entity;
        }                       
        return null; 
    
    }

    public static bool entity_in_map(Session session, string name)
    {
        foreach(LevelData level_data in session.MapData.Levels)
        {
            foreach(EntityData entity_data in level_data.Entities)
            {
                if(entity_data.Name == name)
                {
                    return true;
                }
            }
        }
        return false;
    }
    public static List<EntityData> get_all_entity_data(Session session, string name)
    {
        List<EntityData> result = new();
        foreach(LevelData level_data in session.MapData.Levels)
        {
            foreach(EntityData entity_data in level_data.Entities)
            {
                if(entity_data.Name == name)
                {
                    result.Add(entity_data);
                }
            }
        }
        return result;
        
    }
    public static List<Entity> create_all_entity(Level level, string name)
    {
        List<Entity> result = new();
        foreach(LevelData level_data in level.Session.MapData.Levels)
        {
            Vector2 offset = new Vector2(level_data.Bounds.Left, level_data.Bounds.Top);
            foreach(EntityData entity_data in level_data.Entities)
            {
                if(entity_data.Name == name)
                {

                    if (Level.EntityLoaders.TryGetValue(entity_data.Name, out var value))
                    {
                        Entity entity = value(level, level.Session.LevelData, offset, entity_data);
                        result.Add((Entity)entity);
                    }                       
                }
            }
        }
        return result;
 
    }

    public static bool trigger_in_map(Session session, string name)
    {
        foreach(LevelData level_data in session.MapData.Levels)
        {
            foreach(EntityData entity_data in level_data.Triggers)
            {
                if(entity_data.Name == name)
                {
                    return true;
                }
            }
        }
        return false;
    }
    }



    public class ErrandOfWednesdayModule : EverestModule {
        public static ErrandOfWednesdayModule Instance { get; private set; }

        public override Type SettingsType => typeof(ErrandOfWednesdayModuleSettings);
        public static ErrandOfWednesdayModuleSettings Settings => (ErrandOfWednesdayModuleSettings) Instance._Settings;

        public override Type SessionType => typeof(ErrandOfWednesdayModuleSession);
        public static ErrandOfWednesdayModuleSession Session => (ErrandOfWednesdayModuleSession) Instance._Session;


        /****************************/

        /****************************/

        public static bool lookout = false;
        public static float lookout_value = 0;
        public static bool fully_lookout = false;
        public static bool fully_not_lookout = false;

        public static bool all_lookouts = false;


        public static bool sd_hooks_loaded = false;
        public static bool sd_active = false;
        public static float sd_timer = 0f;
        public static float sd_checkpoint_time = 0f;

        public ILHook bird_hook;

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

            On.Celeste.LevelLoader.ctor += level_loader_constructor;
            Everest.Events.Level.OnLoadLevel += on_load_level;
            Everest.Events.Level.OnExit += on_exit_hook;
            Everest.Events.Level.OnTransitionTo += transition_hook;

            AxisParallax.load();

            typeof(GravityHelperImports).ModInterop();

//            VergeBlock.Load();
        }


       public override void Unload() {
            On.Celeste.Lookout.Update -= lookout_stop;
            On.Monocle.Engine.Update -= Update;

            On.Celeste.LevelLoader.ctor -= level_loader_constructor;
            Everest.Events.Level.OnLoadLevel -= on_load_level;
            Everest.Events.Level.OnExit -= on_exit_hook;
            Everest.Events.Level.OnTransitionTo -= transition_hook;
            SDTimerDisplay.Unload();
            VergeBlock.Unload();
            MusicLayerSource.Unload();
            CannotTransitionTo.unload();
            RefillBubbler.unload();
            PoppingMirror.unload();
        }

        public void level_loader_constructor(On.Celeste.LevelLoader.orig_ctor orig, LevelLoader self, Session session, Vector2? startposition)
        {
            orig(self, session, startposition);
            EyeOfTheWednesday.try_load(session);
        
        }

        private void on_load_level(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            if(Session == null)
            {
   Logger.Log(LogLevel.Warn, "eow", $"Tragedy");
            }
            if(isFromLoader)
            {
                TriggerManager.clear();
                if(level.Session != null)
                {
//                    EyeOfTheWednesday.try_load(level);
                    MusicLayerSource.level_load(level);
                    GlobalDecal.level_load(level);
                    BistableDecal.level_load(level);
                }
            }
            if(Session!= null && Session.sd_active)
            {
                SDTimerDisplay timer = SDTimerDisplay.create();
                SDTimerDisplay.load_session();
                level.Add(timer);
            }
        }

        private void on_exit_hook(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow)
        {
            TriggerManager.clear();
            if(Session!= null && Session.sd_active)
            {
                SDTimerDisplay.save_session();
            }
            SDTimerDisplay.Unload();
            VergeBlock.Unload();
            MusicLayerSource.kill_audio(level);
            MusicLayerSource.Unload();
            GlobalDecal.unload();
            BistableDecal.unload();
            MyAudioTrigger.on_exit(level);
            AreaIntroCutscene.on_exit(level);
            EyeOfTheWednesday.unload();
        }


        public void transition_hook(Level level, LevelData next, Vector2 direction)
        {
            MyAudioTrigger.on_transition(level);
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
