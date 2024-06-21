using System;
using System.Collections;

using Microsoft.Xna.Framework;

using Monocle;

using Celeste;
using Celeste.Mod.Entities;

namespace Celeste.Mod.ErrandOfWednesday
{

    [Tracked]
    [TrackedAs(typeof(DreamBlock))]
    [CustomEntity("eow/VergeBlock")]
    public class VergeBlock : DreamBlock
    {
        public static bool loaded = false;

        public static void Load()
        {
            if(loaded) return;

            On.Celeste.Player.OnCollideV += on_collide_v;

            loaded = true;
        } 
        public static void Unload()
        {
            if(!loaded) return;

            On.Celeste.Player.OnCollideV -= on_collide_v;

            loaded = false;
        }

        public static void on_collide_v(On.Celeste.Player.orig_OnCollideV orig, Player player, CollisionData data)
        {
            if(player.StateMachine.State == Player.StNormal)
            {
                Vector2 dir = Vector2.UnitY * Math.Sign(player.Speed.Y);
                if(data.Hit != null && data.Hit is VergeBlock)
                {
//Logger.Log(LogLevel.Info, "eow", $"hit, {data.Hit.GetType().Name}");
                    VergeBlock block = (VergeBlock)data.Hit;
//Logger.Log(LogLevel.Info, "eow", $"{player.Speed.Y} {dir}");
                    if(block != null && Math.Abs(player.Speed.Y) > block.fall_threshold)
                    {
                        player.StateMachine.State=Player.StDreamDash;
                        player.dreamBlock = block;
                        player.Speed = dir*240f;
                        player.Position += dir;
                        player.dashAttackTimer = 0f;
                        block.activate(player);
                    }
                }
            }
        
            orig(player, data);
        }


        public float drift = 0.3f;

        public Vector2[] targets;
        public bool activated = false;

        public float fall_threshold = 200f;

        public float[] border_alpha;
        public int[] border_sequence;

        public VergeBlock(EntityData data, Vector2 offset, EntityID id) : base(data, offset)
        {

            border_alpha = new float[7];
            border_sequence = new int[7];
            for(int i = 0; i < border_alpha.Length; ++i)
            {
                border_alpha[i] = 0;
                border_sequence[i] = 0;
            }
            fall_threshold = data.Float("fall_threshold");

            targets = new Vector2[data.Nodes.Length];
            for(int i = 0; i < targets.Length; ++i)
            {
                targets[i] = data.Nodes[i]+offset;
            }

//            base.node = null;

        }

        public float get_wobble(int x_index, int y_index)
        {
            int seq_idx = Math.Abs(x_index+y_index)%border_sequence.Length;
            int sequence = border_sequence[seq_idx];
            float alpha = border_alpha[seq_idx];
            float a = (float)Math.Abs((x_index*13+y_index*27+sequence*7)%10);
            float b = (float)Math.Abs((x_index*13+y_index*27+(sequence+1)*7)%10);

            return drift * (a*(1-alpha) + b*alpha);
        }

        public override void Render()
        {
            Camera camera = SceneAs<Level>().Camera;
            if (base.Right < camera.Left || base.Left > camera.Right || base.Bottom < camera.Top || base.Top > camera.Bottom)
            {
                return;
            }

            Vector2 block_offset = base.Position + shake;

//Draw.Rect(block_offset.X, block_offset.Y, base.Width, base.Height, Color.White);


            int tile_x = (int)Math.Floor(block_offset.X/8);
            int tile_y = (int)Math.Floor(block_offset.Y/8);

            int right_tile_x = (int)Math.Floor((block_offset.X+base.Width)/8)+1;
            int bot_tile_y = (int)Math.Floor((block_offset.Y+base.Height)/8)+1;

            Color bg_color = playerHasDreamDash ? activeBackColor : disabledBackColor;

            //vertical slates
            float w;
            //Left edge
            w = 8f*(tile_x+1)-block_offset.X;
            Draw.Rect(block_offset.X, block_offset.Y, w, base.Height, bg_color);

            //Middle
            for(int x_index = tile_x+1; x_index < right_tile_x-1;++ x_index)
            {
                float wobble_top = 0;
                float wobble_bot = 0;
                wobble_top = get_wobble(x_index, tile_y);
                wobble_bot = get_wobble(x_index, bot_tile_y);
                Draw.Rect(x_index*8, block_offset.Y-wobble_top, 8, base.Height+wobble_top+wobble_bot, bg_color);
            }
            //Right edge
            w = block_offset.X+base.Width - (right_tile_x-1)*8f;
            Draw.Rect((right_tile_x-1)*8, block_offset.Y, w, base.Height, bg_color);


            //sideways slats
        
            for(int y_index = tile_y+1; y_index < bot_tile_y-1;++ y_index)
            {
                float wobble_left = 0;
                float wobble_right = 0;
                wobble_left = get_wobble(y_index, tile_x);
                wobble_right = get_wobble(y_index, right_tile_x);
                Draw.Rect(block_offset.X-wobble_left, 
                          y_index*8, 
                          base.Width+wobble_left+wobble_right, 8, bg_color);
            }
 

//            Draw.Rect(block_offset.X, block_offset.Y, base.Width, base.Height, playerHasDreamDash ? activeBackColor : disabledBackColor);
            Vector2 camera_position = SceneAs<Level>().Camera.Position;            
 
            for(int layer = 0; layer < 1; ++layer)
            {
                Vector2 offset = camera_position * (0.3f+0.25f*layer);

                for(float dx = 0; dx < base.Width+8; dx+= 8)
                {
                }
                
            }

        }

        public void activate(Player player)
        {
            if(activated) return;

            foreach(Trigger trigger in Scene.Tracker.GetEntities<Trigger>())
            {
                for(int i = 0; i < targets.Length; ++i)
                {
                    Vector2 target = targets[i];
                    if(trigger.CollidePoint(target))
                    {
                        trigger.OnEnter(player);
                        trigger.OnStay(player);
                    }
                }
            }
//            activated = true;
        }

        public override void Update()
        {
            base.Update();
            for(int i = 0; i < border_alpha.Length; ++i)
            {
                border_alpha[i] += 0.02f;
                if(border_alpha[i] >= 1)
                {
                    border_alpha[i] -= 1;
                    border_sequence[i] += 1;
                }
            }
 

        }
    }
}

