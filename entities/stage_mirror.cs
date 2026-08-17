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
        public const string MIRROR_FLAG = "eow_stage_mirrored";
        public const string FROM_LEFT_FLAG = "eow_stage_mirrored_from_left";
        public const string FROM_RIGHT_FLAG = "eow_stage_mirrored_from_right";
        public const string N_FROM_LEFT_FLAG = "eow_stage_normal_from_left";
        public const string N_FROM_RIGHT_FLAG = "eow_stage_normal_from_right";



        public bool was_left;

        //TODO make persistent

        public StageMirror(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, safe:false)
        {
            Collidable=false;

            

        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);

            Level level = Scene as Level;
            Player player = level.Tracker.GetEntity<Player>();
            if(player != null)
            {
                Vector2 eye_pos = new Vector2(player.Center.X, player.Top);
                was_left = eye_pos.X < Center.X;
            }

   
        }

        public override void Update()
        {
            base.Update();

            Level level = Scene as Level;
            Player player = level.Tracker.GetEntity<Player>();
            if(player != null)
            {
                Vector2 eye_pos = new Vector2(player.Center.X, player.Top);

                bool is_left = eye_pos.X < Center.X;

                if( (eye_pos.Y >= Top && eye_pos.Y <= Bottom) && 
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
             Logger.Log(LogLevel.Info, "eow", $"doing flip {was_left} {eye_pos} {player.Speed} {level.Camera.Position} {Center}");

                    bool is_mirrored = SaveData.Instance.Assists.MirrorMode;
                    level.Session.SetFlag(MIRROR_FLAG, is_mirrored);
                    level.Session.SetFlag(FROM_LEFT_FLAG, was_left&&is_mirrored);
                    level.Session.SetFlag(FROM_RIGHT_FLAG, !was_left&&is_mirrored);
                    level.Session.SetFlag(N_FROM_LEFT_FLAG, was_left&&!is_mirrored);
                    level.Session.SetFlag(N_FROM_RIGHT_FLAG, !was_left&&!is_mirrored);


                    is_left = !is_left;
                }
                was_left = is_left;
            }
        }

        public override void Render()
        {
            base.Render();
        }

    }
}
