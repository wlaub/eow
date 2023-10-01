using System;
using System.Collections;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Monocle;

using Celeste;
using Celeste.Mod.Entities;

namespace Celeste.Mod.ErrandOfWednesday
{

    public class TitleMessage: Entity
    {
        public string title;
        public string sub_title;
        public float alpha = 1f;

        public TitleMessage(string title, string sub_title, float alpha)
        {
            
            base.Tag = Tags.HUD;
    //      string[] array = Dialog.Clean("app_ending").Split(new char[2] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
    //      text = array[data.Int("line")];
            this.title = title;
            this.sub_title = sub_title;
            this.alpha = alpha;
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Render()
        {
Logger.Log(LogLevel.Info, "eow", "lol");
            Vector2 position = (base.Scene as Level).Camera.Position;
            Vector2 vector = position + new Vector2(160f, 90f);
            ActiveFont.Draw(title, vector, new Vector2(0.5f, 0.5f), Vector2.One * 2.5f, Color.White * alpha);

            Vector2 vector2 = vector + new Vector2(160f, 140f);
            ActiveFont.Draw(sub_title, vector, new Vector2(0.5f, 0.5f), Vector2.One * 1.25f, Color.White * alpha);
 
        }
    }

    [Tracked]
    [CustomEntity("eow/AreaIntroCutscene")]
    public class AreaIntroCutscene : Trigger 
    {

        public float speed;
        public float pause;
        public float hold;
        public string title;
        public string sub_title;
        public string next_room;
        public bool once;
        public bool initial;
        public Vector2[] nodes;

        public bool triggered = false;
        public bool done = false;

        public EntityID eid;

        public static bool in_cutscene = false;
        public static Vector2 starting_position;
        public static string starting_room;

        public float _alpha = 1;

        public TitleMessage _title;

        public AreaIntroCutscene (EntityData data, Vector2 offset, EntityID eid) : base(data, offset)
        {
            this.eid = eid;

            nodes = data.NodesOffset(offset);

            once = data.Bool("once");
            initial = data.Bool("initial");

            speed = data.Float("speed");
            pause = data.Float("pause");
            hold = data.Float("hold");
            
            title = data.Attr("title");
            sub_title = data.Attr("sub_title");
            next_room = data.Attr("next_room");
        }

        public void setup(Player player)
        {
            if(!in_cutscene)
            {
                Level level = SceneAs<Level>();
                in_cutscene = true;
                starting_position = player.Position;
                starting_room = level.Session.Level;
Logger.Log(LogLevel.Info, "eow", "initializing cutscene");
Logger.Log(LogLevel.Info, "eow", level.Session.Level);
 
            }

            player.StateMachine.State = Player.StDummy;
            player.DummyGravity = false;
            player.Visible = false;
            player.Collidable = false;

//            if(title != "")
            {
                _title = new TitleMessage(title, sub_title, 1);
                Scene.Add(_title);
            }

        }
        public void cleanup(Player player)
        {
Logger.Log(LogLevel.Info, "eow", "doing cleanup");
            Level level = SceneAs<Level>();
            player.StateMachine.State = 0;
            player.Visible = true;
            player.Collidable = true;
            in_cutscene = false;
            level.ZoomSnap(Vector2.Zero, 1f);

            level.OnEndOfFrame += delegate {
                level.TeleportTo(player, starting_room, Player.IntroTypes.Transition, starting_position);
                    };
 

        }


        public void set_camera_position(Level level, Vector2 target)
        {
            Vector2 cam = target;
            cam.X -= 160;
            cam.Y -= 90;
            cam.X = Calc.Clamp(cam.X, level.Bounds.Left, level.Bounds.Right - 320);
            cam.Y = Calc.Clamp(cam.Y, level.Bounds.Top, level.Bounds.Bottom - 180);
            level.Camera.Position = cam;

        }

        public IEnumerator do_cutscene(Player player)
        {
            Level level = SceneAs<Level>();

            setup(player);

            if(next_room == "")
            {
                player.Visible = true;
            }

            if(!initial)
            {
                Vector2 prev = nodes[0];
                set_camera_position(level, prev);

                for (float t = 0f; t < pause; t += Engine.RawDeltaTime)        
                {
                    yield return null;
                }

                for(int idx = 1; idx < nodes.Length; ++idx)
                {
                    Vector2 next = nodes[idx];

                    for (float t = 0f; t < speed; t += Engine.RawDeltaTime)
                    {
                        Vector2 cam = Vector2.Lerp(prev, next, t/speed);
                        set_camera_position(level, cam);
                        yield return null;

                    }            
                    prev = next;                

                }

                for (float t = 0f; t < hold; t += Engine.RawDeltaTime)        
                {
                    yield return null;
                }


            }
            done = true;

            if(once || initial)
            {
Logger.Log(LogLevel.Info, "eow", $"Do not load {eid}");
                (base.Scene as Level).Session.DoNotLoad.Add(eid);
            }

            yield return null;
        }

        public override void Update()
        {
            base.Update();
            if(done)
            {
                Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
                do_next(player);
            }
        }

        public void do_next(Player player)
        {
            if(next_room == "")
            {
                cleanup(player);
            }
            else
            {
Logger.Log(LogLevel.Info, "eow", $"moving to next room {next_room}");
                Level level = SceneAs<Level>();
                Vector2? spawn = null;
                if(next_room == starting_room)
                {
                    spawn = starting_position;
                }
                level.OnEndOfFrame += delegate {
                    level.TeleportTo(player, next_room, Player.IntroTypes.Transition, spawn);
                    };
            }
 
        }

        public void start_cutscene(Player player)
        {
            if(!triggered)
            {
                triggered = true;
                Add(new Coroutine(do_cutscene(player))); 
            }
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);


            start_cutscene(player);

        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);

            //I don't know why just adding the eid to the do not load doesn't work
            if(SceneAs<Level>().Session.DoNotLoad.Contains(eid))
            {
                RemoveSelf();
                return;
            }

            if(in_cutscene || initial)
            {
                Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
                start_cutscene(player);
            }
        }

        public override void Render()
        {
Logger.Log(LogLevel.Info, "eow", title);
            Level level = SceneAs<Level>();
            ActiveFont.Draw(title, level.Camera.Position, new Vector2(0.5f, 0.5f), Vector2.One * 1.25f, Color.White * _alpha);
        }

    }
    

}
