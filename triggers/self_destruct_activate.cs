using System;
using System.Collections;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Monocle;

using Celeste;
using Celeste.Mod.Entities;

using FMOD.Studio;

namespace Celeste.Mod.ErrandOfWednesday
{
    public class SDTimerDisplay : Entity
    {
        public const string sd_active_flag = "eow_sd_active";

        public static float time_remaining = 420f;
        public static float checkpoint_time = 420f;
        public static string countdown_sound;
        public static string death_sound;
        public static string timer_color_base;
        public static Color timer_color;

        public static bool hooked = false;

        public static bool failstate = false;
        public static bool dying = false;
        public static bool sd_active = false;
        public static SDTimerDisplay instance = null;

        public static bool countdown_configured = false;
        public static float countdown_duration;
        public static float death_alpha = 0f;

        public EventInstance countdown_event;

        public SDTimerDisplay()
        {
            base.Tag = Tags.HUD | Tags.Global;
            instance = this;
            death_alpha = 0f;
            dying = false;
            failstate = false;
            countdown_configured = false;
            sd_active = false;
            Load();
        }

        public static SDTimerDisplay create()
        {
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
            instance = null;
            hooked = false;
            sd_active = false;
            countdown_configured = false;
            
        }

        public static void configure(EntityData data)
        {
            float duration = data.Float("timer_duration");
            checkpoint_time = time_remaining = duration;
            death_sound = data.Attr("death_sound");
            set_countdown_sound(data.Attr("countdown_sound"));
            set_timer_color(data.Attr("timer_color"));
            sd_active = true;
            save_session();
    
        }

        public static void set_timer_color(string color)
        {
            timer_color_base = color;
            timer_color = Calc.HexToColor(color);
        }

        public static void set_countdown_sound(string sound)
        {
            countdown_sound = sound;
            if(countdown_sound != "")
            {
                EventDescription desc = Audio.GetEventDescription(countdown_sound);
                if(desc != null)
                {
                    int base_length;
                    desc.getLength(out base_length);
                    countdown_duration = base_length/1000f;
                    countdown_configured = true;
                }
                else
                {
                    Logger.Log(LogLevel.Error, "eow", $"Couldn't load countdown sound {countdown_sound}");
                }

            }
            save_session();
        }

        public static void save_session()
        {
            ErrandOfWednesdayModule.Session.sd_active = sd_active;
            ErrandOfWednesdayModule.Session.sd_countdown_sound = countdown_sound;
            ErrandOfWednesdayModule.Session.sd_death_sound = death_sound;
            ErrandOfWednesdayModule.Session.sd_checkpoint_time = checkpoint_time;
            ErrandOfWednesdayModule.Session.sd_timer_color = timer_color_base;
        }

        public static void load_session()
        {
            if(sd_active)
            {
                return;
            }
            string timer_color = ErrandOfWednesdayModule.Session.sd_timer_color;
            checkpoint_time = ErrandOfWednesdayModule.Session.sd_checkpoint_time;
            death_sound = ErrandOfWednesdayModule.Session.sd_death_sound;
            set_countdown_sound(ErrandOfWednesdayModule.Session.sd_countdown_sound);
            set_timer_color(timer_color);
            time_remaining = checkpoint_time;
            sd_active = true;
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

        public static void cancel_sd()
        {
            if(time_remaining <= 0 || instance == null)
            {
                return;
            }
            if(instance.countdown_event != null)
            {
                Audio.Stop(instance.countdown_event);
            }
            instance.clear_failstate();
            instance.clear_flag();
            instance.RemoveSelf();
            instance = null;
            sd_active = false;
            save_session();
            Unload();
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            set_flag();
        }

        public void clear_flag()
        {
            SceneAs<Level>().Session.SetFlag(sd_active_flag, false);
        }
        public void set_flag()
        {
            SceneAs<Level>().Session.SetFlag(sd_active_flag, true);
        }

        public void clear_failstate()
        {
            if(failstate)
            {
                SceneAs<Level>().PauseLock = false;
                failstate = false;
            }
        }

        public static void set_failstate(Level level)
        {
            failstate = true;
            level.PauseLock = true;
        }

        public IEnumerator death_routine(Player player)
        {
            EventInstance death_event = null;
            if(death_sound != "")
            {
                death_event = Audio.Play(death_sound);
            }
            

            
            player.StateMachine.State = Player.StDummy;
            player.DummyMoving = true;
            player.DummyAutoAnimate = false;
            player.DummyGravity = false;
            player.Speed = Vector2.Zero;

            Level level = SceneAs<Level>();

            level.RegisterAreaComplete();
            Vector2 base_cam = level.Camera.Position;

            for(float t = 0f; t < 5f; t+= Engine.RawDeltaTime)
            {
                death_alpha = t/5;
                float shake_amt = death_alpha * 6;
                
                level.Camera.Position = base_cam + new Vector2(
        Calc.Random.NextFloat(death_alpha*2)-death_alpha, 
        Calc.Random.NextFloat(shake_amt*2)-shake_amt
                    );

                yield return null;
            }

            while(Audio.IsPlaying(death_event))
            {
                yield return null;    
            }

            level.CompleteArea(false, true, true);       
 
        }

        public override void Update()
        {
            base.Update();

            Level level = SceneAs<Level>();

            if(countdown_configured && !failstate && time_remaining < countdown_duration)
            {
                set_failstate(level);
                countdown_event = Audio.Play(countdown_sound);
            }

            if(time_remaining <= 0 && !dying)
            {
                dying = true;
                set_failstate(level);
                Player player = level.Tracker.GetEntity<Player>();
                Add(new Coroutine(death_routine(player)));
            }

            if(!level.Paused)
            {
                time_remaining -= Engine.RawDeltaTime;
            }

        }
    
        public override void Render()
        {
            int seconds = (int)(Math.Ceiling(time_remaining));
            if(seconds < 0)
            {
                seconds = 0;
            }
            string text = $"T-{seconds} S";
            ActiveFont.Draw(text, 
                new Vector2(160f, 45f)*6f,
                new Vector2(0.5f, 0.5f), Vector2.One,
                timer_color
                );
 
            if(dying)
            {
                Draw.Rect(0,0,1920,1080, Color.White*(death_alpha));
            }


        }

    }


    [Tracked]
    [CustomEntity("eow/SelfDestructActivateTrigger")]
    public class SelfDestructActivateTrigger : Trigger 
    {
        public EntityID eid;
        
        public bool triggered = false;

        public float timer_duration;
        public string start_sound;

        public EntityData data;

        public SelfDestructActivateTrigger (EntityData data, Vector2 offset, EntityID eid) : base(data, offset)
        {
            this.eid = eid;
            this.data = data;
            timer_duration = data.Float("timer_duration");
            start_sound = data.Attr("start_sound");
        }

        public void activate()
        {
            if(triggered)
            {
                return;
            }

            if(start_sound != "")
            {
                Audio.Play(start_sound);
            }

            SDTimerDisplay timer = SDTimerDisplay.create();
            SceneAs<Level>().Add(timer);
            SDTimerDisplay.configure(data);

            triggered = true;
            SceneAs<Level>().Session.DoNotLoad.Add(eid);
        }

        public override void OnEnter(Player player)
        {
            activate();
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);

            if(SceneAs<Level>().Session.DoNotLoad.Contains(eid))
            {
                RemoveSelf();
                return;
            }
        }

    }
   


    [Tracked]
    [CustomEntity("eow/SelfDestructCancelTrigger")]
    public class SelfDestructCancelTrigger : Trigger 
    {
        public bool triggered = false;

        public SelfDestructCancelTrigger (EntityData data, Vector2 offset) : base(data, offset)
        {
        }

        public void activate()
        {
            if(triggered)
            {
                return;
            }

            SDTimerDisplay.cancel_sd();
            triggered = true;
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
