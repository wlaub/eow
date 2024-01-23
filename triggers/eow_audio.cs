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
        public bool is_3d = true;

        public EntityID eid;

        public MySourceEntity(string path, Vector2 position, EntityID eid, bool is_3d = true)
        {
            base.Tag = Tags.Global;
            Position = position;
            audio_event=path;
            this.is_3d = is_3d;
            this.eid = eid;
        }
        
        public override void Added(Scene scene)
        {
            base.Added(scene);

            if(is_3d)
            {
                instance = Audio.Play(audio_event, Position);
            }
            else
            {
                instance = Audio.Play(audio_event);
            }
            MyAudioTrigger.playing_ids.Add(eid);

            if (instance != null && is_3d)
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
            if(is_3d)
            {
                Audio.Position(instance, Position);
            }
            instance.getPlaybackState(out var state);
            if (state == PLAYBACK_STATE.STOPPED)
            {
                instance.release();
                instance = null;
                RemoveSelf();
                MyAudioTrigger.playing_ids.Remove(eid);
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
        public bool once_per_save;
        public bool no_repeat;

        public bool triggerable;
        public bool dnlable;
        public bool recover_on_exit;
        public bool recover_on_transition;

        public Vector2[] nodes;

        public bool triggered = false;

        public EventInstance audio = null;

        public static List<SoundSource> sources = new();

        public static HashSet<EntityID> session_ids = new();
        public static HashSet<EntityID> room_ids = new();

        public static HashSet<EntityID> playing_ids = new();

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
            once_per_save = data.Bool("once_per_save");
            no_repeat = data.Bool("no_repeat");

            triggerable = once_per_run || once_per_room || once_per_session || once_per_save;
            dnlable = once_per_room || once_per_session || once_per_save;
            recover_on_exit = once_per_session && !once_per_save;
            recover_on_transition = once_per_room && !once_per_session && !once_per_save;
        }

        public static void clear_session(Level level)
        {
            foreach (EntityID id in session_ids)
            {
                level.Session.DoNotLoad.Remove(id);
            }
            session_ids.Clear();
        }
        public static void clear_rooms(Level level)
        {
            foreach (EntityID id in room_ids)
            {
                level.Session.DoNotLoad.Remove(id);
            }
            room_ids.Clear();
        }

        public static void on_transition(Level level)
        {
            clear_rooms(level);
        }
        public static void on_exit(Level level)
        {
            clear_rooms(level);
            clear_session(level);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if(SceneAs<Level>().Session.DoNotLoad.Contains(eid))
            {
                triggered = true;
            }
        } 
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
        }

        public void activate()
        {

            Level level = SceneAs<Level>();

            if(nodes.Length == 0)
            {
                SceneAs<Level>().Add(new MySourceEntity(audio_event, Position, eid, false));
            }
            else
            {
               for(int idx = 0; idx < nodes.Length; ++idx) 
                {
                    
                    SceneAs<Level>().Add(new MySourceEntity(audio_event, nodes[idx], eid));
                }
            }


            if(triggerable)
            {
                triggered = true;
            }
            if(dnlable)
            {
                level.Session.DoNotLoad.Add(eid);
            }
            if(recover_on_transition)
            {
                room_ids.Add(eid);
            }
            if(recover_on_exit)
            {
                session_ids.Add(eid);
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
            if(no_repeat)
            {
                foreach (EntityID id in playing_ids)
                {
                }
            }
            if(no_repeat && playing_ids.Contains(eid))
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
