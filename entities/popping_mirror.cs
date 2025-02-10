using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

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

        static Random random = new();

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
            loaded = false;
        }


        public static void set_flag(On.Celeste.Session.orig_SetFlag orig, Session self, string flag, bool val)
        {
            orig(self, flag, val);

            PoppingMirror nearest = null;
            float min_delay = 10000f;
            foreach (PoppingMirror entity in Engine.Scene.Tracker.GetEntities<PoppingMirror>())
            {

                
                if(!entity.activated && flag == entity.control_flag && val != entity.control_flag_inverted)
                {
                    float delay = entity.activate();
                    if(delay > 0 && (delay < min_delay || nearest == null))
                    {
                        min_delay = delay;
                        nearest = entity;
                    }
                }
            }

            //TODO play sound based on nearest

        }



        public EntityID eid;

        public string sprite_directory;
        public float rate;

        public string control_flag;
        public bool control_flag_inverted;
        public string contact_flag;
        public bool contact_flag_inverted;

        public bool on_contact;
        public bool at_least_once;

        public bool activated = false;

        public Sprite idle_sprite;
        public MirrorSurface mirror_surface;

        public PoppingMirror(EntityData data, Vector2 offset, EntityID id)
            : base(data, offset)
        {
            base.Depth=data.Int("depth", 9500);
            Visible = true;
 
            rate = data.Float("rate");
          
            control_flag_inverted = Flagic.process_flag(data.Attr("control_flag", ""), out control_flag);
            contact_flag_inverted = Flagic.process_flag(data.Attr("on_contact_flag", ""), out contact_flag);
 
            on_contact = data.Bool("on_contact"); 
            at_least_once = data.Bool("on_contact"); 
            //will have to hook set_flag
 
            sprite_directory = data.Attr("sprite_directory");

            int nw = (int)(Width/8);
            int nh = (int)(Width/8);

            string texture_key = $"{nw}.{nh}";
            string texture = $"{sprite_directory}{nw}.{nh}.";

//////////////////
            Dictionary<string, List<string>> texture_map = new();

            foreach(string path in GFX.Game.Textures.Keys)
            {
                if(path.StartsWith(sprite_directory))
                {
                    string name = path.Substring(sprite_directory.Length);
                    string key = name.Substring(0, 3);
                    if(!texture_map.ContainsKey(key)) 
                    {
                        texture_map[key] = new List<string>();
                    }
                    texture_map[key].Add(name);
   Logger.Log(LogLevel.Debug, "eow", $"{key}");
                }
            }
/////////////////

        //TODO load static texture map on map load instead
            if(texture_map.ContainsKey(texture_key))
            {
                string name = Calc.Random.Choose<string>(texture_map[texture_key]);
//                string name = texture_map[texture_key][idx];
                texture = sprite_directory + name;

   Logger.Log(LogLevel.Debug, "eow", $"{name}");
                idle_sprite = new Sprite(GFX.Game, "");
                idle_sprite.AddLoop("enabled", texture, 0.08f);
    //            idle_sprite.CenterOrigin();

                idle_sprite.Scale = new Vector2(data.Float("scaleX", 1f), data.Float("scaleY", 1f));
                idle_sprite.Rotation = (float) (data.Float("rotation", 0f) * Math.PI / 180f);
                idle_sprite.Play("enabled");
                Add(idle_sprite);


            MTexture mask = GFX.Game.GetAtlasSubtexturesAt($"{sprite_directory}mirror/{name}", 0);
            mirror_surface = new MirrorSurface();
            mirror_surface.ReflectionOffset = new Vector2(Calc.Random.Range(5, 14) * Calc.Random.Choose(1, -1), Calc.Random.Range(2, 6) * Calc.Random.Choose(1, -1));;
            mirror_surface.OnRender = delegate
            {
                mask.DrawJustified(Position, new Vector2(0f,0f), mirror_surface.ReflectionColor, 1, 0);
            };
            Add(mirror_surface);


            }



        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            //Check flag and set to post-activation state
            //if flag and it not at least once

            //Hide main sprite
            //show shatter sprites after animation
        }

        public float activate()
        {
            if(activated){return -1;}

            activated = true;

            //Get player
            //Get distance to player
            //Start a coroutine with a delay based on distance


            return 0f;
        }

        public IEnumerator pop_routine(Player player, float delay)
        {

            yield return delay;

            //Get direction to player
            //Hide main sprite
            //Show shatter sprites/start animation
            //cast shatter sprites away for a few couple frames

            yield break;
        }

    }
}
