using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

using Microsoft.Xna.Framework;

using Monocle;

using Celeste;
using Celeste.Mod.Entities;

namespace Celeste.Mod.ErrandOfWednesday
{

    [Tracked]
    [CustomEntity("eow/PoppingMirror")]
    public class PoppingMirror : Trigger
    {

        public static bool loaded = false;
        public static void try_load()
        {
            if(loaded){return;}
            On.Celeste.Session.SetFlag += set_flag;
            loaded = true;
        }
        public static void unload()
        {
            if(!loaded){return;}
            On.Celeste.Session.SetFlag -= set_flag;
            texture_map.Clear();
            loaded_textures.Clear();
            break_textures.Clear();
            loaded = false;
        }


        public static Dictionary<string, List<string>> texture_map = new();
        public static HashSet<string> loaded_textures = new();
        public static List<string> break_textures = new();

        public static void try_load_textures(string sprite_directory)
        {
            if(loaded_textures.Contains(sprite_directory))
            {
                return;
            }
//   Logger.Log(LogLevel.Debug, "eow", $"Loading textures {sprite_directory}");
            string break_directory = sprite_directory + "break/";
            foreach(string path in GFX.Game.Textures.Keys)
            {
                if(path.StartsWith(sprite_directory))
                {

                    if(path.StartsWith(break_directory))
                    {
                        string name = path.Substring(break_directory.Length);

                        if(!name.Contains("/"))
                        {
                            name = Regex.Replace(name, "\\d+$", string.Empty);
                            if(!break_textures.Contains(name))
                            {
//   Logger.Log(LogLevel.Debug, "eow", $"Adding {name}");
                                break_textures.Add(name);
                            }
                        }
                    }
                    else
                    {
                        string name = path.Substring(sprite_directory.Length);
                        string key = name.Substring(0, 3);
//   Logger.Log(LogLevel.Debug, "eow", $"Adding {name}");
                        if(!name.Contains("/"))
                        {
                            name = Regex.Replace(name, "\\d+$", string.Empty);
                            if(!texture_map.ContainsKey(key)) 
                            {
                                texture_map[key] = new List<string>();
                            }
                            if(!texture_map[key].Contains(name))
                            {
//       Logger.Log(LogLevel.Debug, "eow", $"Adding {name}");
                                texture_map[key].Add(name);
                            }
                        }
                    }
//   Logger.Log(LogLevel.Debug, "eow", $"{key}");
                }
            }

            loaded_textures.Add(sprite_directory);
        }



        public static bool lockout = false;
        public static void set_flag(On.Celeste.Session.orig_SetFlag orig, Session self, string flag, bool val)
        {
            orig(self, flag, val);

            if(lockout)
            {
                return;
            }

            PoppingMirror nearest = null;
            float min_delay = 10000f;
            string found_trigger_sound = "";
            bool was_activated = false;
            foreach (PoppingMirror entity in Engine.Scene.Tracker.GetEntities<PoppingMirror>())
            {

                
                if(!entity.activated && flag == entity.control_flag && val != entity.control_flag_inverted)
                {
                    was_activated = true;
                    float delay = entity.activate();
                    if(delay >= 0 && (delay < min_delay || nearest == null))
                    {
                        if(!string.IsNullOrWhiteSpace(entity.trigger_sound))
                        {
                            min_delay = delay;
                            nearest = entity;
 
                            found_trigger_sound = entity.trigger_sound;
                        }
                    }
                }
            }

            if(was_activated)
            {
                if(!string.IsNullOrWhiteSpace(found_trigger_sound))
                {
                    Audio.Play(found_trigger_sound);
                }
            }
        }


        public EntityID eid;

        public string sprite_directory;
        public float rate_sq;

        public float frame_delay;
        public float break_frame_delay;
        public string control_flag;
        public bool control_flag_inverted;
        public string contact_flag;
        public bool contact_flag_inverted;

        public string shatter_group;

        public bool on_contact;
        public bool at_least_once;
        public bool only_this;
        public bool only_on_contact;

        public string shatter_sound;
        public string trigger_sound;

        public int shrink;

        public float rotation;

        public bool activated = false;

        public float width, height;

        public Sprite idle_sprite;
        public MirrorSurface mirror_surface;
        public string mirror_sprite_name;
        public MTexture mirror_mask;
        public List<Shard> break_sprites = new();

        public PoppingMirror(EntityData data, Vector2 offset, EntityID id)
            : base(data, offset)
        {
            base.Depth=data.Int("depth", 9500);

            shrink = data.Int("shrink", 4);
            base.Collider = new Hitbox(Width-shrink*2, Height-shrink*2, shrink, shrink);
            Visible = true;

            width = data.Width;
            height = data.Height;

            trigger_sound = data.Attr("trigger_sound", "");
            shatter_sound = data.Attr("shatter_sound", "") ;

            float rate = data.Float("rate");
            rate_sq = rate*rate*60*60*4;
            frame_delay = data.Float("frame_delay", 0.2f);
            break_frame_delay = data.Float("break_frame_delay", 0.2f);

            control_flag_inverted = Flagic.process_flag(data.Attr("control_flag", ""), out control_flag);
            contact_flag_inverted = Flagic.process_flag(data.Attr("on_contact_flag", ""), out contact_flag);
 
            on_contact = data.Bool("on_contact", false); 
            at_least_once = data.Bool("at_least_once", true); 
            only_this = data.Bool("only_this", true);
            only_on_contact = data.Bool("only_on_contact", true);

            shatter_group = data.Attr("shatter_group");

            rotation = (float) ((data.Float("rotation", 0f)) * Math.PI / 180f);
//                 idle_sprite.Scale = new Vector2(data.Float("scaleX", 1f), data.Float("scaleY", 1f));
 
            sprite_directory = data.Attr("sprite_directory");

            try_load_textures(sprite_directory);

            int nw = (int)(width/8);
            int nh = (int)(height/8);

            setup_break_sprites(nw, nh);

            bool rotated = false;
            if(nw > nh)
            {
                int temp = nw;
                nw = nh;
                nh = temp;
                rotation -= (float)(Math.PI/2);
                rotated = true;
            }

            setup_idle_sprite(nw, nh, rotated);


        }

        public class Shard : Sprite
        {

            public Vector2 speed;
            public float angle_speed;

            public string mirror_sprite_name;
            public MTexture mirror_mask;
            public MirrorSurface mirror_surface;
           
            public bool done = false;
 
            public Shard(Atlas atlas, string whatever, string texture, float frame_delay, int x, int y) : base(atlas, whatever)
            {
//   Logger.Log(LogLevel.Debug, "eow", $"{texture}");
                if(!loaded){return;}

                Add("enabled", texture, frame_delay);

                Position = new Vector2(4f+8f*x, 4f+8f*y);
                Rotation = (float)(Calc.Random.NextFloat()*2*Math.PI);

                CenterOrigin();
                speed = new Vector2(Calc.Random.NextFloat()-0.5f, Calc.Random.NextFloat()-0.5f)/3f;
                angle_speed = (float)Math.PI*(Calc.Random.NextFloat()-0.5f)/8;


                mirror_sprite_name = $"{whatever}mirror/{texture}";

                OnFrameChange = delegate(string anim)
                {
                    mirror_mask = GFX.Game.GetAtlasSubtexturesAt(mirror_sprite_name, CurrentAnimationFrame);
                };

                mirror_mask = GFX.Game.GetAtlasSubtexturesAt(mirror_sprite_name, CurrentAnimationFrame);

                mirror_surface = new MirrorSurface();
//                mirror_surface.ReflectionOffset = new Vector2(Calc.Random.Range(5, 14) * Calc.Random.Choose(1, -1), Calc.Random.Range(2, 6) * Calc.Random.Choose(1, -1));;
                mirror_surface.OnRender = delegate
                {
                    mirror_mask.DrawCentered(Entity.Position+Position, Color.White, 1, Rotation);
                };
            }

            public override void Update()
            {
                base.Update();
                if(! done)
                {
                    Position += speed;
                    Rotation += angle_speed;
                    speed *= 0.9f;
                    angle_speed *= 0.9f;
                    if(Math.Abs(speed.X) < 1e-3 && Math.Abs(speed.Y) < 1e-3 )
                    {
                        done = true;
                    }
                }
            }
            
            public void skip()
            {
                Position += speed/(1-0.9f); 
                Rotation += angle_speed/(1-0.9f);
                speed = Vector2.Zero;
                angle_speed = 0f;
                done = true;
                Play("enabled");
                SetAnimationFrame(CurrentAnimationTotalFrames-1);
            }

        }

        public void setup_break_sprites(int nw, int nh)
        {

            for(int x = 0; x < nw; ++x)
            {
                for(int y = 0; y < nh; ++y)
                {
            string name = Calc.Random.Choose<string>(break_textures);
            string texture = sprite_directory + "break/" + name;
            
            Shard sprite = new Shard(GFX.Game, sprite_directory+"break/", name, break_frame_delay, x, y);
            break_sprites.Add(sprite);

                   
                }
            }
        }

        public void activate_break_sprites(Vector2 speed, bool skip = false)
        {
            foreach(Shard sprite in break_sprites)
            {
                sprite.speed += speed; 
                sprite.Play("enabled");
                if(skip)
                {
                    sprite.skip();
                }

                Add(sprite);
                Add(sprite.mirror_surface);

            }
        }

        public void setup_idle_sprite(int nw, int nh, bool rotated)
        {
            string texture_key = $"{nw}.{nh}";


            if(texture_map.ContainsKey(texture_key))
            {
                string name = Calc.Random.Choose<string>(texture_map[texture_key]);
                string texture = sprite_directory + name;

//   Logger.Log(LogLevel.Debug, "eow", $"{name}");
                idle_sprite = new Sprite(GFX.Game, "");
                idle_sprite.AddLoop("enabled", texture, frame_delay);
                if(rotated)
                {
                    idle_sprite.SetOrigin(
                        idle_sprite.Width,
                        0
                        );
                }
                else
                {
                    idle_sprite.SetOrigin(
                        (width-idle_sprite.Width)/2, 
                        (height-idle_sprite.Height)/2
                        );
                }

                idle_sprite.Rotation = rotation;
                idle_sprite.Play("enabled");
                
                Add(idle_sprite);

                mirror_sprite_name = $"{sprite_directory}mirror/{name}";

                idle_sprite.OnFrameChange = delegate(string anim)
                {
                    mirror_mask = GFX.Game.GetAtlasSubtexturesAt(mirror_sprite_name, idle_sprite.CurrentAnimationFrame);
                };

                mirror_mask = GFX.Game.GetAtlasSubtexturesAt(mirror_sprite_name, idle_sprite.CurrentAnimationFrame);

                mirror_surface = new MirrorSurface();
//                mirror_surface.ReflectionOffset = new Vector2(Calc.Random.Range(5, 14) * Calc.Random.Choose(1, -1), Calc.Random.Range(2, 6) * Calc.Random.Choose(1, -1));;

//                mirror_surface.ReflectionOffset = Vector2.Zero;
//   Logger.Log(LogLevel.Info, "eow", $"{mirror_surface.ReflectionColor}");
                mirror_surface.OnRender = delegate
                {
                    mirror_mask.DrawCentered(Center, Color.White, 1, idle_sprite.Rotation);
                };
                Add(mirror_surface);
            }



        }

        public override void Render()
        {
//                    mirror_mask.DrawCentered(Center, mirror_surface.ReflectionColor, 1, idle_sprite.Rotation);
 
            base.Render();
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if(!loaded)
            {
                Logger.Log(LogLevel.Warn, "eow", $"Popping Mirror must be enabled by Eye of the Wednesday to work. See Readme.");
                RemoveSelf();
                return;
            }

            Level level = scene as Level;
            if(!at_least_once && (
               Flagic.test_flag(level.Session, control_flag, control_flag_inverted)
              ))
            {
                do_pop(new Vector2(0,0), true);
            }

            if(!string.IsNullOrWhiteSpace(shatter_group))
            {
                control_flag += "_"+shatter_group;
            }
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if(on_contact && !activated) 
            {
                if(only_this && !string.IsNullOrWhiteSpace(contact_flag))
                {
                    lockout = true;
                }
                Flagic.set_flag(SceneAs<Level>().Session, contact_flag, contact_flag_inverted);
                if(!string.IsNullOrWhiteSpace(shatter_group))
                {
                 Flagic.set_flag(SceneAs<Level>().Session, control_flag, control_flag_inverted);
                }
                activate(true);
                lockout = false;
            }
        }

        public float activate(bool is_contact = false)
        {
            if(activated){return -1;}

            if(only_on_contact && !is_contact)
            {
                return -1;
            }

            activated = true;

            Level level = SceneAs<Level>();
            Player player = level.Tracker.GetEntity<Player>();
            if(player != null)
            {
                float dist = Vector2.DistanceSquared(Center, player.Center);
                float delay = dist/rate_sq;
                delay = (float)Math.Sqrt(delay);
                Add(new Coroutine(pop_routine(player, delay)));
                return delay;
            }


            return 0f;
        }

        public void do_pop(Vector2 speed, bool skip = false)
        {
            Remove(idle_sprite);
            Remove(mirror_surface);
            activate_break_sprites(speed, skip);
            activated = true;
            if(!skip && !string.IsNullOrWhiteSpace(shatter_sound))
            {
                Audio.Play(shatter_sound, Center);
            }
        }

        public IEnumerator pop_routine(Player player, float delay)
        {

            yield return delay;

            Vector2 speed = Center-player.Center;
            speed.Normalize();
            do_pop(speed);

            yield break;
        }

    }
}
