using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Monocle;

using Celeste;
using Celeste.Mod.Entities;

namespace Celeste.Mod.ErrandOfWednesday
{
    public class SDTimerDisplay : Entity
    {
        public static float time_remaining = 600f;
        public static float checkpoint_time = 600f;

        public static bool hooked = false;

        public bool inited = false;

        public static SDTimerDisplay instance = null;

        public SDTimerDisplay()
        {
            base.Tag = Tags.HUD | Tags.Global;
            instance = this;
            Load();
        }

        public static SDTimerDisplay create()
        {
            save_session();
            if(instance != null)
            {
                return instance;
            }
            return new SDTimerDisplay();
        }

        public static void Load()
        {
            if(hooked) return;
            Everest.Events.Player.OnSpawn += on_spawn;
            Everest.Events.Level.OnTransitionTo += on_transition;
            hooked = true;
        }
 
        public static void Unload()
        {
            if(!hooked) return;
            Everest.Events.Player.OnSpawn -= on_spawn;
            Everest.Events.Level.OnTransitionTo -= on_transition;
            hooked = false;
        }

        public static void set_time(float duration)
        {
            checkpoint_time = time_remaining = duration;
            save_session();
        }

        public static void save_session()
        {
            ErrandOfWednesdayModule.Session.sd_active = true;
            ErrandOfWednesdayModule.Session.sd_checkpoint_time = checkpoint_time;
        }

        public static void load_session()
        {
            checkpoint_time = ErrandOfWednesdayModule.Session.sd_checkpoint_time;
            time_remaining = checkpoint_time;
        }

        public static void on_transition(Level level, LevelData next, Vector2 direction)
        {
            checkpoint_time = time_remaining;
            save_session();
        }

        public static void on_spawn(Player player)
        {
            time_remaining = checkpoint_time;
        }

        public override void Update()
        {
            base.Update();

            if(!SceneAs<Level>().Paused)
            {
                time_remaining -= Engine.RawDeltaTime;
            }

        }
    
        public override void Render()
        {
            int seconds = (int)(Math.Round(time_remaining));
            string text = $"T-{seconds} S";
            ActiveFont.Draw(text, 
                new Vector2(160f, 90f)*6f,
                new Vector2(0.5f, 0.5f), Vector2.One,
                Color.Green
                );
        }

    }


    [Tracked]
    [CustomEntity("eow/SelfDestructActivateTrigger")]
    public class SelfDestructActivateTrigger : Trigger 
    {
        public bool triggered = false;

        public float timer_duration;

        public SelfDestructActivateTrigger (EntityData data, Vector2 offset) : base(data, offset)
        {
            timer_duration = data.Float("timer_duration");
        }

        public void activate()
        {
            if(triggered)
            {
                return;
            }

            SDTimerDisplay timer = SDTimerDisplay.create();
            SDTimerDisplay.set_time(timer_duration);
            SceneAs<Level>().Add(timer);
            triggered = true;
            //TODO DNL
        }

        public override void OnEnter(Player player)
        {
            activate();
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
        }

    }
    

}
