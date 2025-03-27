
using System;

using Microsoft.Xna.Framework;

using Monocle;

using Celeste;
using Celeste.Mod.Entities;

using System.Text.RegularExpressions;
using System.IO;

namespace Celeste.Mod.ErrandOfWednesday
{

    [Tracked]
    [CustomEntity("eow/BistableDecal")]
    public class BistableDecal : Entity
    {
        public EntityID eid;


        public Sprite sprite;
        public string control_flag;
        public bool control_flag_inverted;
        public string activated_flag;

        public string idle_loop = "idle";
        public string open_anim = "open";
        public string close_anim = "close";
        public string done_loop = "done";

        public bool at_least_once = true;

        public bool global = false;

        public bool actual = false;

        public bool activated = false;

        public BistableDecal(EntityData data, Vector2 offset, EntityID eid) : base(data.Position + offset)
            {

            this.eid = eid;

            string sprite_name = data.Attr("sprite", "");

            if(GFX.SpriteBank.Has(sprite_name))
            {
                Add(sprite=GFX.SpriteBank.Create(sprite_name));

                sprite.Rotation = (float)(data.Float("rotation", 0)*Math.PI/180);

                sprite.Color = Calc.HexToColor(data.Attr("color", "ff0000ff"));
                sprite.Scale = new Vector2(data.Float("scaleX", 1f), data.Float("scaleY", 1f));
            }

            base.Depth = data.Int("depth", 9000);

            control_flag_inverted = Flagic.process_flag(data.Attr("control_flag", ""), out control_flag);
 
            activated_flag = $"eow_bistable_decal_active_{eid}";

            idle_loop = data.Attr("idle_loop", "idle");
            done_loop = data.Attr("done_loop", "done");
            open_anim = data.Attr("open_anim", "open");
            close_anim = data.Attr("close_anim", "close");

            at_least_once = data.Bool("at_least_once", false);
            global = data.Bool("global", false);
            if(global) 
            {
                base.Tag = Tags.Global;
                actual = false;
            }
            else
            {
                actual = true;
            }

        }


        public override void Awake(Scene scene)
        {
            base.Awake(scene);

            if(!actual || sprite == null)
            {
                RemoveSelf();
Logger.Log(LogLevel.Info, "eow", "Bistable decal removing self");
                return;
            }


            Level level = scene as Level;

            if(Flagic.test_flag(level.Session, control_flag, control_flag_inverted))
            {
                if(at_least_once && !Flagic.test_flag(level.Session, activated_flag, false))
                { //this one hasn't activated and the control flag is true
                    sprite.Play(open_anim);
                }
                else
                {
                    sprite.Play(done_loop);
                }
                activated = true;
                level.Session.SetFlag(activated_flag, true);
            }
            else
            {
                if(at_least_once && Flagic.test_flag(level.Session, activated_flag, false))
                {//this one has activated
                    sprite.Play(close_anim);
                }
                else
                {
                    sprite.Play(idle_loop);
                }
                level.Session.SetFlag(activated_flag, false);
                activated = false;


            }
            
        } 

        public static void set_flag(On.Celeste.Session.orig_SetFlag orig, Session self, string flag, bool val)
        {
            orig(self, flag, val);

             foreach (BistableDecal entity in Engine.Scene.Tracker.GetEntities<BistableDecal>())
            {

                if(!entity.activated && flag == entity.control_flag && val != entity.control_flag_inverted)
                {
                    entity.sprite.Play(entity.open_anim);
                    entity.activated = true;
                    self.SetFlag(entity.activated_flag, true);
                    //Set flag
                }
                else if(entity.activated && flag == entity.control_flag && val == entity.control_flag_inverted)
                {
                    entity.sprite.Play(entity.close_anim);
                    entity.activated = false;
                    self.SetFlag(entity.activated_flag, false);
                    //Clear flag
                } 

            }
           

        }

        public static bool loaded = false;

        public static void try_load()
        {
            if(loaded){return;}
            On.Celeste.Session.SetFlag += set_flag;
 
            loaded = true;
        }

        public static void level_load(Level level)
        {
            if(!loaded){return;}
Logger.Log(LogLevel.Debug, "eow", "Loading global bistable decals.");


            foreach(LevelData level_data in level.Session.MapData.Levels)
            {
                Vector2 offset = new Vector2(level_data.Bounds.Left, level_data.Bounds.Top);
                foreach(EntityData entity_data in level_data.Entities)
                {
                    if(entity_data.Name == "eow/BistableDecal" && entity_data.Bool("global", false))
                    {

                        if (Level.EntityLoaders.TryGetValue(entity_data.Name, out var value))
                        {
                            Entity entity = value(level, level.Session.LevelData, offset, entity_data);
                            ((BistableDecal)entity).actual = true;
                            level.Add(entity);


                        }                       
                    }
                }
            }

        }

        public static void unload()
        {
            if(!loaded){return;}
            On.Celeste.Session.SetFlag -= set_flag;
            loaded = false;
        }

    }
}
