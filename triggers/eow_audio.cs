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
            }
            else
            {
               for(int idx = 0; idx < nodes.Length; ++idx) 
                {
                    audio = Audio.Play(audio_event, nodes[idx]);
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
