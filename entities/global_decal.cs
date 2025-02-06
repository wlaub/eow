
using System;

using Microsoft.Xna.Framework;

using Monocle;

using Celeste;
using Celeste.Mod.Entities;

namespace Celeste.Mod.ErrandOfWednesday
{

    [Tracked]
    [CustomEntity("eow/GlobalDecal")]
    public class GlobalDecal : Decal
    {
        public EntityID eid;

        public string flag;
        public bool flag_inverted;

        public bool actual = false;

        public GlobalDecal(EntityData data, Vector2 offset, EntityID eid) : base(
                data.Attr("sprite"), 
                data.Position + offset, 
                new Vector2(data.Float("scaleX", 1f), data.Float("scaleY", 1f)),
                data.Int("depth", 9000),
                data.Float("rotation", 0),
                data.Attr("color", "ffffffff")
                )
        {
            this.eid = eid;

            flag = data.Attr("flag");
            if(!string.IsNullOrWhiteSpace(flag) && flag[0] == '!')
            {
                flag_inverted = true;
                flag = flag.Substring(1);
            }
            base.Tag = Tags.Global;
        /*
            Depth = data.Int("depth");

            string sprite_name = "";
            if(data.Has("sprite"))
            {
                sprite_name = data.Attr("sprite");
            }
            float delay = 1f/12;
            sprite = new Sprite(GFX.Game, "decals/");
            sprite.AddLoop("enabled", sprite_name, delay);
            sprite.CenterOrigin();

            sprite.Scale = new Vector2(data.Float("scaleX", 1f), data.Float("scaleY", 1f));
            sprite.Rotation = (float) (data.Float("rotation", 0f) * Math.PI / 180f);
            Add(sprite);
*/
        }

        public static void try_load(Level level)
        {
Logger.Log(LogLevel.Debug, "eow", "Loading global decals.");
            foreach(Entity entity in MyLevelInspect.create_all_entity(level, "eow/GlobalDecal"))
            {
                ((GlobalDecal)entity).actual = true;
                level.Add(entity);
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if(!actual)
            {
                RemoveSelf();
            }
            (scene as Level).Session.DoNotLoad.Add(eid);
        }

        public override void Update()
        {
            base.Update();

            Visible = (string.IsNullOrWhiteSpace(flag) || SceneAs<Level>().Session.GetFlag(flag) != flag_inverted);
//  Logger.Log(LogLevel.Info, "eow", $"Decal {eid} is {Visible}"); 
        }

        public override void Render()
        {
            base.Render();
//Logger.Log(LogLevel.Info, "eow", $"Decal {eid} rendered"); 
        }

    }
}
