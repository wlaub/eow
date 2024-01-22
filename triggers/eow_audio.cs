using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Monocle;

using FMOD;
using FMOD.Studio;

using Celeste;
using Celeste.Mod.Entities;



namespace Celeste.Mod.ErrandOfWednesday
{

    public class MySourceEntity : Entity
    {
        public EventInstance instance;
        public string audio_event;


        public MySourceEntity(string path, Vector2 position)
        {
            base.Tag = Tags.Global;
            Position = position;
            audio_event=path;
        }
        
        public override void Added(Scene scene)
        {
            base.Added(scene);

            instance = Audio.Play(audio_event, Position);
            if (instance != null)
            {
                Audio.Position(instance, Position);
            }
        }

        public override void Removed(Scene scene)
        {
            if (instance != null)
            {
                instance.stop(STOP_MODE.ALLOWFADEOUT);
                instance = null;
            }
 
        }
        public override void SceneEnd(Scene scene)
        {
            if (instance != null)
            {
                instance.stop(STOP_MODE.ALLOWFADEOUT);
                instance = null;
            }
 
        }


        public override void Update()
        {
            Audio.Position(instance, Position);
            instance.getPlaybackState(out var state);
            if (state == PLAYBACK_STATE.STOPPED)
            {
                instance.release();
                instance = null;
                RemoveSelf();
            }
        }        
    }

    [Tracked]
    [CustomEntity("eow/MyAudioTrigger")]
    public class MyAudioTrigger : Trigger 
    {
        public EntityID eid;

        public string flag;
        public string audio_event;
        public bool flag_state;
        public bool once_per_run;
        public bool once_per_room;
        public bool once_per_session;
        public bool interruptable;
        public bool dont_interrupt;

        public Vector2[] nodes;

        public bool triggered = false;

        public EventInstance audio = null;

        public static List<EventInstance> audio_queue = new();
        public static List<SoundSource> sources = new();

        public MyAudioTrigger (EntityData data, Vector2 offset, EntityID eid) : base(data, offset)
        {
            this.eid = eid;
            nodes = data.NodesOffset(offset);

            flag = data.Attr("flag");
            audio_event = data.Attr("audio_event");

            flag_state = data.Bool("flag_state");
            once_per_run = data.Bool("once_per_run");
            once_per_room = data.Bool("once_per_room");
            once_per_session = data.Bool("once_per_session");

        }

        public static void stop_all()
        {

            foreach(EventInstance audio in audio_queue)
            {
                audio.stop(STOP_MODE.ALLOWFADEOUT);
            }
            audio_queue.Clear();
        }

        private static bool clear_finished(EventInstance x)
        {
            return !Audio.IsPlaying(x);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if(SceneAs<Level>().Session.DoNotLoad.Contains(eid))
            {
                triggered = true;
            }

            if(once_per_room && !once_per_session)
            {
                Everest.Events.Level.OnTransitionTo += transition_hook;
            }
        } 
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if(once_per_room)
            {
                Everest.Events.Level.OnTransitionTo -= transition_hook;
            }
            audio_queue.RemoveAll(clear_finished);
        }

        public void transition_hook(Level level, LevelData next, Vector2 direction)
        {
            level.Session.DoNotLoad.Remove(eid);
        }

        public void activate()
        {

            Level level = SceneAs<Level>();

            if(nodes.Length == 0)
            {
                audio = Audio.Play(audio_event);
                audio_queue.Add(audio);
            }
            else
            {
               for(int idx = 0; idx < nodes.Length; ++idx) 
                {
                    
                    SceneAs<Level>().Add(new MySourceEntity(audio_event, nodes[idx]));
                    
                }
            }


            if(once_per_run || once_per_room || once_per_session)
            {
                triggered = true;
            }
            if(once_per_room || once_per_session)
            {
                level.Session.DoNotLoad.Add(eid);
            }
        }

        public bool check()
        {
            if(triggered)
            {
                return false;
            }
            if(flag != "" && SceneAs<Level>().Session.GetFlag(flag) != flag_state)
            {
                return false;
            }

            return true;
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);

            if(check())
            {
                activate();
            }

        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);

       }

    }
    

}
