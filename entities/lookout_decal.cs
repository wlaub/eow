
using System;

using Microsoft.Xna.Framework;

using Monocle;

using Celeste;
using Celeste.Mod.Entities;

namespace Celeste.Mod.ErrandOfWednesday
{

    [Tracked]
    [CustomEntity("eow/WatchtowerDecal")]
    public class LookoutDecal : Entity
    {

        public Sprite sprite;
        public string flag;
        public bool flag_inverted;
        public bool lookout;
        public bool not_lookout;

        public LookoutDecal(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            flag = data.Attr("flag");
            flag_inverted = data.Bool("inverted");
            lookout = data.Bool("lookout");
            not_lookout = data.Bool("not_lookout");

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

        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            sprite.Play("enabled");
            Visible = not_lookout;
//Logger.Log(LogLevel.Info, "eow", "lookout: "+lookout+", not lookout: "+not_lookout);
        }

        public override void Update()
        {
            base.Update();

            bool enable = true;
            if(flag == "") 
            {
                enable = true;
            }
            else
            {
                enable = SceneAs<Level>().Session.GetFlag(flag) != flag_inverted;
            }

            if(!enable)
            {
                Visible = false;
                return;
            }

            bool is_lookout = ErrandOfWednesdayModule.fully_lookout;
            bool is_not_lookout = !ErrandOfWednesdayModule.lookout;

            if(is_lookout && lookout || is_not_lookout && not_lookout)
            {
                Visible = true;
            }
            else
            {
                Visible = false;
            }
        }


    }
}
