using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Monocle;

using Celeste;
using Celeste.Mod.Entities;

namespace Celeste.Mod.ErrandOfWednesday
{

    [Tracked]
    [CustomEntity("eow/StageMirror")]
    public class StageMirror : Solid
    {
        public Vector2? last_pos = null;

        public StageMirror(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, safe:false)
        {
            Collidable=false;
        }

        public override void Update()
        {
            base.Update();

            Level level = Scene as Level;
            Player player = level.Tracker.GetEntity<Player>();
            if(player != null)
            {
                Vector2 eye_pos = new Vector2(player.Center.X, player.Top);

                if(last_pos is not null)
                {
                    bool was_left = last_pos.Value.X < Center.X;
                    bool is_left = eye_pos.X < Center.X;

                    if(eye_pos.Y >= Top && eye_pos.Y <= Bottom && 
                        (was_left != is_left)
                        )
                    {
                        float dx = player.Center.X - Center.X;
                        player.Position.X -=2*dx;
                        player.Speed.X *= -1;
                        player.DashDir.X *= -1;
                        player.Facing = (Facings)(-(int)player.Facing);

                        float cdx = level.Camera.Position.X-Center.X+160f;
                        level.Camera.Position += new Vector2(-2*cdx,0f);
 
                        SaveData.Instance.Assists.MirrorMode = !SaveData.Instance.Assists.MirrorMode;
Input.MoveX.Inverted = (Input.Aim.InvertedX = (Input.Feather.InvertedX = SaveData.Instance.Assists.MirrorMode));
                 Logger.Log(LogLevel.Info, "eow", $"doing flip {was_left} {last_pos} {eye_pos} {player.Speed} {level.Camera.Position} {Center}");
                        
                    }
                }


                last_pos = new Vector2(player.Center.X, player.Top);


            }




        }

        public override void Render()
        {
            base.Render();
        }

    }
}
