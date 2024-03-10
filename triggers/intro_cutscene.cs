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
        public float alpha_title = 0f;
        public float alpha_sub = 0f;
        public Color color_title = Color.White;
        public Color color_sub = Color.White;
        public float title_size = 2.5f;
        public float sub_size = 1.25f;


        public TitleMessage(string title, string sub_title, float title_size, float sub_size, Color title_color, Color sub_color)
        {
            
            base.Tag = Tags.HUD;
    //      string[] array = Dialog.Clean("app_ending").Split(new char[2] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
    //      text = array[data.Int("line")];
            this.title = title;
            this.sub_title = sub_title;
            this.title_size = title_size;
            this.sub_size = sub_size;
            this.color_title = title_color;
            this.color_sub = sub_color;
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Render()
        {
            Vector2 position = new Vector2(160f, 90f);

            if(sub_title != "")
            {
                float margin = 0f;//6f*4;
                Vector2 vector = position*6f + new Vector2(0f, -margin);
                ActiveFont.Draw(title, vector, new Vector2(0.5f, 1f), Vector2.One * title_size, color_title * alpha_title);

                Vector2 vector2 = position*6f + new Vector2(0f, margin);
                ActiveFont.Draw(sub_title, vector2, new Vector2(0.5f, 0f), Vector2.One * sub_size, color_sub * alpha_sub);

                Vector2 size = ActiveFont.Measure(title)*title_size;
                Vector2 size2 = ActiveFont.Measure(sub_title)*sub_size;
                float width = (size2.X + size.X)/2;
                width = Calc.Max(width, size2.X);
                Draw.Line(
                    position*6f + new Vector2(-width/2, 0f), 
                    position*6f + new Vector2(width/2, 0f), 
                    color_sub*alpha_sub, 6);


            }
            else
            {
                ActiveFont.Draw(title, position*6f, new Vector2(0.5f, 0.5f), Vector2.One * 2.5f, color_title * alpha_title);


            }

        }
    }

    [Tracked]
    [CustomEntity("eow/AreaIntroCutscene")]
    public class AreaIntroCutscene : Trigger 
    {

        public float speed;
        public float pause;
        public float hold;
        public float fade_in_time;
        public float title_size;
        public float sub_title_size;
        public string title;
        public string sub_title;
        public string next_room;
        public bool once;
        public bool initial;
        public Color title_color;
        public Color sub_title_color;
        public Vector2[] nodes;

        public bool triggered = false;
        public bool done = false;

        public EntityID eid;

        public static bool in_cutscene = false;
        public static Vector2 starting_position;
        public static string starting_room;

        public static float title_alpha = 0f;
        public static float sub_alpha = 0f;

        public bool _raise_alpha = false;
        public bool _lower_alpha = false;
        public float _raise_value;

        public float _path_length = 0;
        public float _effective_speed;

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
            fade_in_time = data.Float("fade_in_time");

            title_size = data.Float("title_size");
            sub_title_size = data.Float("sub_title_size");
            title_color = Calc.HexToColor(data.Attr("title_color"));
            sub_title_color = Calc.HexToColor(data.Attr("sub_title_color"));
            
            title = data.Attr("title");
            sub_title = data.Attr("sub_title");

            next_room = data.Attr("next_room");

            if (fade_in_time <= 0)
            {
                _raise_value = 1;
            }
            else
            {
                _raise_value = 1f/(fade_in_time*60f);
            }

            Vector2 prev = nodes[0];
            for(int idx = 1; idx < nodes.Length; ++idx)
            {
                Vector2 next = nodes[idx];
                _path_length += Vector2.Distance(prev, next);
                prev = next;
            }

            if(initial)
            {
                title_alpha = 0;
                sub_alpha = 0;
            }
        }

        public void setup(Player player)
        {
            if(!in_cutscene)
            {
                Level level = SceneAs<Level>();
                in_cutscene = true;
//                starting_position = player.Position;
                starting_position = nodes[0];
                starting_room = level.Session.Level;
//Logger.Log(LogLevel.Info, "eow", $"initializing cutscene, {starting_position.X}, {starting_position.Y}");
//Logger.Log(LogLevel.Info, "eow", level.Session.Level);
                level.StartCutscene(cleanup); 
            }

            player.StateMachine.State = Player.StDummy;
            player.Speed.X = 0;
            player.Speed.Y = 0;
            player.DummyGravity = false;
            player.Sprite.Visible = (player.Hair.Visible = false);
            player.Collidable = false;

            //Prevents them from disappearing
            foreach (Follower follower in player.Leader.Followers)
            {
                if(follower.Entity != null)
                {
                    follower.Entity.AddTag(Tags.Global);
                }
            }

            if(next_room == "")
            {
//Logger.Log(LogLevel.Info, "eow", $"attempting to go home, {starting_position.X}, {starting_position.Y}");
                player.Position = starting_position;
                Level level = SceneAs<Level>();
                if(level.Session.Level == starting_room)
                {
                    show_player_ending(player);
                }
            }


//            if(title != "")
            {
                _title = new TitleMessage(Dialog.Clean(title), Dialog.Clean(sub_title), title_size, sub_title_size, title_color, sub_title_color);
                update_alpha();
                Scene.Add(_title);
            }

        }
        public static void cleanup(Level level)
        {
//Logger.Log(LogLevel.Info, "eow", "doing cleanup");
//Logger.Log(LogLevel.Info, "eow", $"doing cleanup, {starting_position.X}, {starting_position.Y}");
//            Level level = SceneAs<Level>();

            if(!in_cutscene)
            {
                return;
            }
            
            in_cutscene = false;
            title_alpha = 0f;
            sub_alpha = 0f;
            level.ZoomSnap(Vector2.Zero, 1f);

            Player player = level.Tracker.GetEntity<Player>();
            if(player != null)
            {
                player.Sprite.Visible = (player.Hair.Visible = true);
                player.StateMachine.State = 0;
                player.Collidable = true;

                if(starting_room != null)
                {
                    level.OnEndOfFrame += delegate {
    //    Logger.Log(LogLevel.Info, "eow", $"going home, {starting_position.X}, {starting_position.Y}");
                        level.TeleportTo(player, starting_room, Player.IntroTypes.Transition, starting_position);
                        player.Position = starting_position;
                        level.Camera.Position = player.CameraTarget;
                        level.Session.HitCheckpoint = true;
                        level.Session.RespawnPoint = starting_position;
                        // 
                        foreach (Follower follower in player.Leader.Followers)
                        {
                            if(follower.Entity != null)
                            {
                                follower.Entity.RemoveTag(Tags.Global);
                            }
                        }



                        };
                }
            }
            if(level.InCutscene)
            {
                level.EndCutscene(); 
            }

        }

        public static void on_exit(Level level)
        {
            cleanup(level);
        }

        public override void SceneEnd(Scene scene)
        {
//     Logger.Log(LogLevel.Info, "eow", $"scene ending");           
            cleanup(scene as Level);
            base.SceneEnd(scene);
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

        public void update_alpha()
        {
            if(_title != null)
            {
                _title.alpha_title = title_alpha;
                _title.alpha_sub = sub_alpha;
            }
        }

        public void show_player_ending(Player player)
        {
            player.DummyAutoAnimate = false;
            player.Sprite.Visible = (player.Hair.Visible = true);
            player.ResetSprite(player.Sprite.Mode);
            player.Sprite.Play("idle");
        }

        public IEnumerator do_cutscene(Player player)
        {
            Level level = SceneAs<Level>();

            setup(player);

            if(next_room == "")
            { 
                show_player_ending(player);
            }
            _raise_alpha = true;

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
                    float segment_length = Vector2.Distance(next, prev);
                    float segment_duration = speed*segment_length/_path_length;
                    for (float t = 0f; t < segment_duration; t += Engine.RawDeltaTime)
                    {
                        Vector2 cam = Vector2.Lerp(prev, next, t/segment_duration);
                        set_camera_position(level, cam);
     
                        yield return null;

                    }            
                    prev = next;                

                }

                _raise_alpha = false;
                if(next_room == "")
                {
                    _lower_alpha = true;
                }
                for (float t = 0f; t < hold; t += Engine.RawDeltaTime)        
                {
                    yield return null;
                }


            }
            done = true;

            if(once || initial)
            {
//Logger.Log(LogLevel.Info, "eow", $"Do not load {eid}");
                (base.Scene as Level).Session.DoNotLoad.Add(eid);
            }

            yield return null;
        }

        public void inc_alpha(float amount)
        {
                if(title != "")
                {
                    title_alpha += amount;
                    title_alpha = Calc.Clamp(title_alpha, 0, 1) ;
                }
                if(sub_title != "")
                {
                    if(amount > 0 && title_alpha < 0.9)
                    {
                        return;
                    }
                    sub_alpha += amount;
                    sub_alpha = Calc.Clamp(sub_alpha, 0, 1) ;
            }
 
        }


        public override void Update()
        {
            base.Update();

            if(!in_cutscene)
            {
                return;
            }

            if(_raise_alpha)
            {
                inc_alpha(_raise_value);
            }
            if(_lower_alpha)
            {
                if(hold > 0)
                {
                    inc_alpha(-Calc.Max(1f, 1f/hold)/60);
                }
            }
            update_alpha();
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
//Logger.Log(LogLevel.Info, "eow", $"{player.Sprite.HasHair} {player.Sprite.HairFrame}");
            if(player.Sprite.Texture != null)
            {
//Logger.Log(LogLevel.Info, "eow", $"{player.Sprite.Texture.AtlasPath}");
            }
            if(done)
            {
//                Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
                do_next(player);
            }
        }

        public void do_next(Player player)
        {
            if(next_room == "")
            {
                Level level = SceneAs<Level>();
                level.SkipCutscene();
//                cleanup(level);
            }
            else
            {
//Logger.Log(LogLevel.Info, "eow", $"moving to next room {next_room}");
                Level level = SceneAs<Level>();
                level.OnEndOfFrame += delegate {
                    level.TeleportTo(player, next_room, Player.IntroTypes.Transition, starting_position);
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

    }
    

}
