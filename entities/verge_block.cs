using System;
using System.Collections;
using System.Collections.Generic;

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

        public static string path_join(string a, string b)
        {
            if(a.EndsWith("/"))
            {
                return a+b;
            }
            return a+"/"+b;
        }

        public static Dictionary<string, MTexture[]> texture_registry = new();
        public static Dictionary<string, MTexture[]> outline_registry = new();

        public static int N_layers = 3;

        public class VergeBlockTexture
        {
            public string texture_name;

            public MTexture[] fill;
            public MTexture[] outline_h;
            public MTexture[] outline_v;
            public MTexture[] corner_ul;
            public MTexture[] corner_ur;
            public MTexture[] corner_lr;
            public MTexture[] corner_ll;

            public int fill_count;
            public int hedge_count;
            public int vedge_count;
            public int corner_count;

            public VergeBlockTexture(string texture_name)
            {
                this.texture_name = texture_name;
                setup_textures();
            }

            public int get_index(int x, int y, int i, int count)
            {
                int result = (x*7 + y*17 + i*23)%count;
                if(result < 0)
                {
                    result += count;
                }
                return result;
            }

            public int get_fill_index(int x, int y, int i)
            {
                return get_index(x,y,i, fill_count);
            }
            public int get_corner_index(int x, int y, int i)
            {
                return get_index(x,y,i, corner_count);
            }
            public int get_hedge_index(int x, int y, int i)
            {
                return get_index(x,y,i, hedge_count);
            }
            public int get_vedge_index(int x, int y, int i)
            {
                return get_index(x,y,i, vedge_count);
            }

            public void setup_textures()
            {
                MTexture base_texture;
                //fill
                base_texture = GFX.Game[path_join(texture_name, "fill")] ;
                int Nx = (int)(base_texture.Width/8);
                int Ny = (int)(base_texture.Height/8);

                fill_count = Nx*Ny;
                fill = new MTexture[fill_count];
                
                for(int x = 0; x < Nx; ++x)
                {
                    for(int y = 0; y < Ny; ++y)
                    {
                        fill[x+y*Nx] = base_texture.GetSubtexture(x*8, y*8, 8,8);
                    }
                }

                //corners
                base_texture = GFX.Game[path_join(texture_name, "corners")] ;
                Nx = (int)(base_texture.Width/32);
                Ny = (int)(base_texture.Height/32);

                corner_count = Nx*Ny;
                corner_ul = new MTexture[corner_count];
                corner_ur = new MTexture[corner_count];
                corner_lr = new MTexture[corner_count];
                corner_ll = new MTexture[corner_count];

                for(int x = 0; x < Nx; ++x)
                {
                    for(int y = 0; y < Ny; ++y)
                    {
                        int idx = x+y*Nx;
                        corner_ul[idx] = base_texture.GetSubtexture(x*32, y*32, 16,16);
                        corner_ur[idx] = base_texture.GetSubtexture(x*32+32, y*32, 16,16);
                        corner_lr[idx] = base_texture.GetSubtexture(x*32+32, y*32+32, 16,16);
                        corner_ll[idx] = base_texture.GetSubtexture(x*32, y*32+32, 16,16);
                    }
                }

                //horizontal lines

                base_texture = GFX.Game[path_join(texture_name, "hedges")] ;
                Nx = (int)((base_texture.Width-16)/8);

                hedge_count = Nx;
                outline_h = new MTexture[Nx];

                for(int x = 0; x < Nx; ++x)
                {
                    outline_h[x] = base_texture.GetSubtexture(x*8+8, 0, 8, 16);
                }

                //vertical lines
                base_texture = GFX.Game[path_join(texture_name, "vedges")] ;
                Ny = (int)((base_texture.Height-16)/8);

                vedge_count = Ny;
                outline_v = new MTexture[Ny];

                for(int y = 0; y < Ny; ++y)
                {
                    outline_v[y] = base_texture.GetSubtexture(0, y*8+8, 16, 8);
                }

            }

        }
       

        public Vector2[] targets;
        public bool activated = false;

        public float fall_threshold = 200f;

        public string texture_name;

        public VergeBlockTexture textures; 

        public VergeBlock(EntityData data, Vector2 offset, EntityID id) : base(data, offset)
        {

            fall_threshold = data.Float("fall_threshold");

            texture_name = data.Attr("texture", "objects/eow/VergeBlock");
            textures = new(texture_name);

            //TODO node modes
            targets = new Vector2[data.Nodes.Length];
            for(int i = 0; i < targets.Length; ++i)
            {
                targets[i] = data.Nodes[i]+offset;
            }

            base.node = null;

        }
       
        public override void Awake(Scene scene)
        {
            base.Awake(scene);

            Rectangle bounds = new((int)Position.X, (int)Position.Y, (int)Width+1, (int)Height+1);

            foreach(VergeBlock block in scene.Tracker.GetEntities<VergeBlock>())
            {
                if(block == this)
                {
                    continue;
                }

                Rectangle isect = Rectangle.Intersect(bounds, new Rectangle((int)block.Position.X, (int)block.Position.Y, (int)block.Width+1, (int)block.Height+1));
 Logger.Log(LogLevel.Info, "eow", $"{isect.X} {isect.Y} {isect.Width} {isect.Height}");

            }
        } 

        public override void Render()
        {
            Camera camera = SceneAs<Level>().Camera;
            if (base.Right < camera.Left || base.Left > camera.Right || base.Bottom < camera.Top || base.Top > camera.Bottom)
            {
                return;
            }

            Vector2 block_offset = base.Position + shake;

            //background
            Draw.Rect(block_offset.X, block_offset.Y, base.Width, base.Height, playerHasDreamDash ? activeBackColor : disabledBackColor);
            Vector2 camera_position = SceneAs<Level>().Camera.Position;            
            int scale;
 
            for(int layer = 0; layer < 2; ++layer)
            {
//                scale = 4*(2-layer);
                scale = 8;
                Vector2 offset = camera_position * (0.3f+0.25f*layer);
                //The upper left corner of the upper left tile that is complete inside this block
                Vector2 idx_offset = (block_offset-offset)/scale;
                idx_offset.X = (float)Math.Ceiling(idx_offset.X)*scale;
                idx_offset.Y = (float)Math.Ceiling(idx_offset.Y)*scale;
                int x_idx_base = (int)(idx_offset.X/scale);
                int y_idx_base = (int)(idx_offset.Y/scale);

                idx_offset += offset;
/*
Logger.Log(LogLevel.Info, "eow", $"{block_offset} {idx_offset}");


texture = textures.fill[0];
Draw.SpriteBatch.Draw(texture.Texture.Texture_Safe, block_offset, texture.ClipRect, Color.White);
 */

                MTexture texture;
                int x, y;
                int dx, dy;
                int idx;
                float xpos, ypos;
                Rectangle clip_mask;
                x = x_idx_base;

                //Leftern edge

                dx = (int)Math.Round((block_offset.X-(idx_offset.X-scale))*8/scale, MidpointRounding.AwayFromZero);

                int dy_top = (int)Math.Round((block_offset.Y-(idx_offset.Y-scale))*8/scale, MidpointRounding.AwayFromZero);


                //UL corner
                idx = textures.get_fill_index(x_idx_base-1,y_idx_base-1,0);
                texture = textures.fill[idx];

                clip_mask = texture.GetRelativeRect(dx, dy_top, texture.ClipRect.Width-dx, texture.ClipRect.Height-dy_top);
                Draw.SpriteBatch.Draw(texture.Texture.Texture_Safe, 
                        new Vector2(block_offset.X, block_offset.Y), clip_mask, 
                        Color.White, 0, new Vector2(0,0), scale/8, 0, 0
                        );

                //Inner edge
                y = y_idx_base;
                for(ypos = idx_offset.Y; ypos+scale < block_offset.Y+base.Height; ypos += scale)
                {
                    idx = textures.get_fill_index(x_idx_base-1,y,0);
                    texture = textures.fill[idx];
                    clip_mask = texture.GetRelativeRect(dx, 0, texture.ClipRect.Width-dx, texture.ClipRect.Height);
                    Draw.SpriteBatch.Draw(texture.Texture.Texture_Safe, 
                            new Vector2(block_offset.X, ypos), clip_mask, 
                            Color.White, 0, new Vector2(0,0), scale/8, 0, 0
                            );
                    ++y;
                }

                //LL corner
                dy = (int)Math.Round((block_offset.Y+base.Height-ypos)*8/scale, MidpointRounding.AwayFromZero);
                if(dy > 0)
                {
                    idx = textures.get_fill_index(x_idx_base-1,y,0);
                    texture = textures.fill[idx];

                    clip_mask = texture.GetRelativeRect(dx, 0, texture.ClipRect.Width-dx, dy);
                    Draw.SpriteBatch.Draw(texture.Texture.Texture_Safe, 
                            new Vector2(block_offset.X, ypos), clip_mask, 
                            Color.White, 0, new Vector2(0,0), scale/8, 0, 0
                            );
                }

                //Lengthwise interior
                for(xpos = idx_offset.X; xpos+scale < block_offset.X+base.Width; xpos += scale)
                {
                    idx = textures.get_fill_index(x,y_idx_base-1,0);
                    texture = textures.fill[idx];

                    clip_mask = texture.GetRelativeRect(0, dy_top, texture.ClipRect.Width, texture.ClipRect.Height-dy_top);
                    Draw.SpriteBatch.Draw(texture.Texture.Texture_Safe, 
                            new Vector2(xpos, block_offset.Y), clip_mask, 
                            Color.White, 0, new Vector2(0,0), scale/8, 0, 0
                            );


                    y = y_idx_base;
                    for(ypos = idx_offset.Y; ypos+scale < block_offset.Y+base.Height; ypos += scale)
                    {


                        idx = textures.get_fill_index(x,y,0);
                        texture = textures.fill[idx];
                        Draw.SpriteBatch.Draw(texture.Texture.Texture_Safe, 
                                new Vector2(xpos, ypos), texture.ClipRect, 
                                Color.White, 0, new Vector2(0,0), scale/8, 0, 0
                                );
                        ++y;
                    }
                    dy = (int)Math.Round((block_offset.Y+base.Height-ypos)*8/scale, MidpointRounding.AwayFromZero);
                    if(dy > 0)
                    {
                        idx = textures.get_fill_index(x,y,0);
                        texture = textures.fill[idx];

                        clip_mask = texture.GetRelativeRect(0, 0, texture.ClipRect.Width, dy);
                        Draw.SpriteBatch.Draw(texture.Texture.Texture_Safe, 
                                new Vector2(xpos, ypos), clip_mask, 
                                Color.White, 0, new Vector2(0,0), scale/8, 0, 0
                                );
                    }


                    ++x;
                }

                //Rightmost edge

                dx = (int)Math.Round((block_offset.X+base.Width-xpos)*8/scale, MidpointRounding.AwayFromZero);
                if(dx > 0)
                {

                    //UR corner
                    idx = textures.get_fill_index(x,y_idx_base-1,0);
                    texture = textures.fill[idx];

                    clip_mask = texture.GetRelativeRect(0, dy_top, dx, texture.ClipRect.Height-dy_top);
                    Draw.SpriteBatch.Draw(texture.Texture.Texture_Safe, 
                            new Vector2(xpos, block_offset.Y), clip_mask, 
                            Color.White, 0, new Vector2(0,0), scale/8, 0, 0
                            );

                    //Inner edge
                    y = y_idx_base;
                    for(ypos = idx_offset.Y; ypos+scale < block_offset.Y+base.Height; ypos += scale)
                    {


                        idx = textures.get_fill_index(x,y,0);
                        texture = textures.fill[idx];
                        clip_mask = texture.GetRelativeRect(0, 0, dx, texture.ClipRect.Height);
                        Draw.SpriteBatch.Draw(texture.Texture.Texture_Safe, 
                                new Vector2(xpos, ypos), clip_mask, 
                                Color.White, 0, new Vector2(0,0), scale/8, 0, 0
                                );
                        ++y;
                    }

                    //LR corner
                    dy = (int)Math.Round((block_offset.Y+base.Height-ypos)*8/scale, MidpointRounding.AwayFromZero);
                    if(dy > 0)
                    {
                        idx = textures.get_fill_index(x,y,0);
                        texture = textures.fill[idx];

                        clip_mask = texture.GetRelativeRect(0, 0, dx, dy);
                        Draw.SpriteBatch.Draw(texture.Texture.Texture_Safe, 
                                new Vector2(xpos, ypos), clip_mask, 
                                Color.White, 0, new Vector2(0,0), scale/8, 0, 0
                                );
                    }

                }

            }

            //Border

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
        }
    }
}

