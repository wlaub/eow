using System.Collections;

using Microsoft.Xna.Framework;

using Monocle;

using Celeste;
using Celeste.Mod.Entities;

namespace Celeste.Mod.ErrandOfWednesday
{

    [Tracked]
    [CustomEntity("eow/MirrorBlock")]
    public class MirrorBlock : LockBlock
    {

        public MirrorBlock(EntityData data, Vector2 offset, EntityID id)
            : base(data.Position + offset, id, data.Bool("stepMusicProgress"), data.Attr("sprite", "wood"), data.Attr("unlock_sfx", null))
        {
            base.Depth=data.Int("depth");

            string mirrormask = data.Attr("mirrormask");
            if(mirrormask!= "")
            {
                foreach (MTexture mask in GFX.Game.GetAtlasSubtextures("mirrormasks/" + mirrormask))
                {
                    MirrorSurface surface = new MirrorSurface();
                    surface.ReflectionOffset = new Vector2(Calc.Random.Range(5, 14) * Calc.Random.Choose(1, -1), Calc.Random.Range(2, 6) * Calc.Random.Choose(1, -1));;
                    surface.OnRender = delegate
                    {
                        mask.DrawCentered(Position + new Vector2(16f,16f), surface.ReflectionColor, 1, 0);
                    };
                    Add(surface);
                }            
            }
        }


    }
}
