/*

<some suitable cosmic horror quote>

*/

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
            On.Monocle.Engine.Update += static_update;
            On.Monocle.EntityList.RenderExcept += outline_render_hook;
            Everest.Events.Level.OnExit += on_exit_hook;
            Everest.Events.Level.OnTransitionTo += transition_hook;

            loaded = true;
        } 
        public static void Unload()
        {
            if(!loaded) return;

            On.Celeste.Player.OnCollideV -= on_collide_v;
            On.Monocle.Engine.Update -= static_update;
            On.Monocle.EntityList.RenderExcept -= outline_render_hook;
            Everest.Events.Level.OnExit -= on_exit_hook;
            Everest.Events.Level.OnTransitionTo -= transition_hook;

            loaded = false;
        }

        public static void transition_hook(Level level, LevelData next, Vector2 direction)
        {
 Logger.Log(LogLevel.Info, "eow", $"transition hook {direction}");
            //TODO resume dream dash?
        }

        public static void on_exit_hook(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow)
        {
            Unload();
        }

        public static void outline_render_hook(On.Monocle.EntityList.orig_RenderExcept orig, EntityList self, int exclude_tags)
        {
            orig(self, exclude_tags);
            render_outline();
        }

        public static void static_update(On.Monocle.Engine.orig_Update orig, Engine self, GameTime gameTime)
        {
            orig(self, gameTime);
            for(int i = 0; i < N_layers; ++i)
            {
                animation_timers[i] -= Engine.DeltaTime;
                if(animation_timers[i] < 0)
                {
                    animation_timers[i] += fill_animation_rate;
                    ++animation_index[i];
                }

                outline_animation_timers[i] -= Engine.DeltaTime;
                if(outline_animation_timers[i] < 0)
                {
                    outline_animation_timers[i] += outline_animation_rate;
                    ++outline_animation_index[i];
                }
 
            }
 
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

        public static float fill_animation_rate = 0.2f;
        public static float outline_animation_rate = 0.3f;
        public static int[] animation_index = {0,0,0};
        public static float[] animation_timers = {0,fill_animation_rate/3,3*fill_animation_rate/3};
        public static int[] outline_animation_index = {0,0,0};
        public static float[] outline_animation_timers = {0,outline_animation_rate/3,2*outline_animation_rate/3};



        public struct OutlineChunk 
        {
            public MTexture[] textures;
            public int x_off;
            public int y_off;

            public MTexture get_texture(int x, int y, int i)
            {
                int count = textures.Length;
                int result = (x*7 + y*17 + i*1999)%count;
                if(result < 0)
                {
                    result += count;
                }
 
                return textures[result];
            }

            public void render(float xpos, float ypos, int x, int y, int i)
            {
                get_texture(x,y,i).Draw(new Vector2(xpos+x_off, ypos+y_off), Vector2.Zero);
            }
 
        }


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

            public OutlineChunk[] outline_lookup = new OutlineChunk[16];

            public int fill_count;
            public int hedge_count;
            public int vedge_count;
            public int corner_count;

            public int BOT = 1;
            public int RIGHT = 2;
            public int LR = 3;
            public int TOP = 4;
            public int UR = 6;
            public int LEFT = 8;
            public int LL = 9;
            public int UL = 12;

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

            public void setup_textures()
            {
//Logger.Log(LogLevel.Info, "eow", $"start");
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

                for(int i = 0; i < 16; ++i)
                {
                    outline_lookup[i] = new();
                }

                //corners
                base_texture = GFX.Game[path_join(texture_name, "corners")] ;
                Nx = (int)(base_texture.Width/32);
                Ny = (int)(base_texture.Height/32);

                outline_lookup[UL].textures = new MTexture[Nx*Ny];
                outline_lookup[UR].textures = new MTexture[Nx*Ny];
                outline_lookup[LL].textures = new MTexture[Nx*Ny];
                outline_lookup[LR].textures = new MTexture[Nx*Ny];

                outline_lookup[UL].x_off = -8;
                outline_lookup[UL].y_off = -8;
                outline_lookup[LL].x_off = -8;
                outline_lookup[LL].y_off = 0;
                outline_lookup[UR].x_off = 0;
                outline_lookup[UR].y_off = -8;
                outline_lookup[LR].x_off = 0;
                outline_lookup[LR].y_off = 0;

                for(int x = 0; x < Nx; ++x)
                {
                    for(int y = 0; y < Ny; ++y)
                    {
                        int idx = x+y*Nx;
                        outline_lookup[UL].textures[idx] = base_texture.GetSubtexture(x*32, y*32, 16,16);
                        outline_lookup[UR].textures[idx] = base_texture.GetSubtexture(x*32+16, y*32, 16,16);
                        outline_lookup[LR].textures[idx] = base_texture.GetSubtexture(x*32+16, y*32+16, 16,16);
                        outline_lookup[LL].textures[idx] = base_texture.GetSubtexture(x*32, y*32+16, 16,16);
                    }
                }

                //horizontal lines

                base_texture = GFX.Game[path_join(texture_name, "hedges")] ;
                Nx = (int)((base_texture.Width-16)/8);

                outline_lookup[TOP].textures = new MTexture[Nx];
                outline_lookup[BOT].textures = new MTexture[Nx];
                outline_lookup[TOP].x_off = 0;
                outline_lookup[TOP].y_off = -8;
                outline_lookup[BOT].x_off = 0;
                outline_lookup[BOT].y_off = 0;

                for(int x = 0; x < Nx; ++x)
                {
                    outline_lookup[TOP].textures[x] = base_texture.GetSubtexture(x*8+8, 0, 8, 16);
                    outline_lookup[BOT].textures[x] = base_texture.GetSubtexture(x*8+8, 0, 8, 16);
                }

                //vertical lines
                base_texture = GFX.Game[path_join(texture_name, "vedges")] ;
                Ny = (int)((base_texture.Height-16)/8);

                outline_lookup[LEFT].textures = new MTexture[Ny];
                outline_lookup[RIGHT].textures = new MTexture[Ny];
 
                outline_lookup[LEFT].x_off = -8;
                outline_lookup[LEFT].y_off = 0;
                outline_lookup[RIGHT].x_off = 0;
                outline_lookup[RIGHT].y_off = 0;

                for(int y = 0; y < Ny; ++y)
                {
                    outline_lookup[LEFT].textures[y] = base_texture.GetSubtexture(0, y*8+8, 16, 8);
                    outline_lookup[RIGHT].textures[y] = base_texture.GetSubtexture(0, y*8+8, 16, 8);
                }

            }

        }
       
        public EntityID id;

        public Vector2[] targets;
        public bool activated = false;

        public float fall_threshold = 200f;

        public string texture_name;

        public Color[] fill_colors;

        public VergeBlockTexture textures; 

        public Rectangle tilebounds;
        public Rectangle real_tilebounds;
        
        public VergeBlock(EntityData data, Vector2 offset, EntityID id) : base(data, offset)
        {
            this.id = id;

            fall_threshold = data.Float("fall_threshold");

            texture_name = data.Attr("texture", "objects/eow/VergeBlock");
            textures = new(texture_name);

            //TODO jumpthrough enable
            //TODO trigger mode (on dash, on jumpthrough, on exit)
            //TODO original render mode
            //TODO Color options (flags?)
            //TODO layer enable (flags?)
            //TODO animation enable
            //TODO extend dream dash on transition

            fill_colors = new Color[N_layers];
            fill_colors[0] = Calc.HexToColor("00ff00");
            fill_colors[2] = Calc.HexToColor("ff8800");
            fill_colors[1] = Calc.HexToColor("0000ff");

            //TODO node modes
            targets = new Vector2[data.Nodes.Length];
            for(int i = 0; i < targets.Length; ++i)
            {
                targets[i] = data.Nodes[i]+offset;
            }

            base.node = null;

            int x_start = (int)Position.X/8;
            int y_start = (int)Position.Y/8;
            int twidth = (int)Width/8;
            int theight = (int)Height/8;

            tilebounds = new(x_start-1, y_start-1, twidth+2, theight+2);
            real_tilebounds = new(x_start, y_start, twidth, theight);

        }

        public static HashSet<EntityID> role_call = new();
        //TODO double buffer for smooth room transitions
        public static Dictionary<int[],OutlineChunk> outline_chunks = new(); 

        public int get_case_idx(int [,] tiles, int x, int y)
        {
            int case_index = 0;
            case_index += tiles[x, y+1];
            case_index += tiles[x+1, y] << 1;
            case_index += tiles[x, y-1] << 2;
            case_index += tiles[x-1, y] << 3;
            return case_index;

        }

        public void process_outline_tile(int[,] tiles, int x, int y, int x_off, int y_off)
        {
            int case_index = 0;
            case_index += tiles[x, y+1];
            case_index += tiles[x+1, y] << 1;
            case_index += tiles[x, y-1] << 2;
            case_index += tiles[x-1, y] << 3;

            if(textures.outline_lookup[case_index].textures != null)
            {
                outline_chunks.Add(new int[] {x+x_off,y+y_off}, textures.outline_lookup[case_index]);
            }
        }

        public static void clear_state()
        {
            role_call.Clear();
            outline_chunks.Clear();
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);

            role_call.Remove(id);
            if(role_call.Count == 0)
            {
                clear_state();
            }

        }


        public override void Added(Scene scene)
        {
            Load();
            base.Added(scene);
        }
 
        public void resume_dream_dash(Player player)
        {
            player.StateMachine.State=Player.StDreamDash;
            player.dreamBlock = this;
            player.dashAttackTimer = 0f;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);

            Player player = CollideFirst<Player>(Position);
            if(player != null)
            {
                resume_dream_dash(player);
            }

            role_call.Add(id);

            int x,y;

            int[,] tiles = new int[tilebounds.Width, tilebounds.Height];

            //There's got to be a better way to do this
            //TODO Possibly I can just fill the outline with 1's
            for(x = 0; x < tilebounds.Width; ++x)
            {
                for(y=0; y < tilebounds.Height; ++y)
                {
                    tiles[x, y] = 1;
                }
            }

            for(x = 0; x < tilebounds.Width-2; ++x)
            {
                for(y=0; y < tilebounds.Height-2; ++y)
                {
                    tiles[x+1, y+1] = 0;
                }
            }

            foreach(VergeBlock block in scene.Tracker.GetEntities<VergeBlock>())
            {
                if(block == this)
                {
                    continue;
                }

                Rectangle intersect = Rectangle.Intersect(tilebounds, block.real_tilebounds);
                for(x = intersect.X-tilebounds.X; x < intersect.X-tilebounds.X+intersect.Width; ++x)
                {
                    for(y = intersect.Y-tilebounds.Y; y < intersect.Y-tilebounds.Y+intersect.Height; ++y)
                    {
                        tiles[x, y] = 0;
                    }
                }

// Logger.Log(LogLevel.Info, "eow", $"{isect.X} {isect.Y} {isect.Width} {isect.Height}");

            }

            //TODO don't generate outline outside of room
            for(x = 1; x < tilebounds.Width-1; ++x)
            {
                process_outline_tile(tiles, x, 1, tilebounds.X, tilebounds.Y);
                process_outline_tile(tiles, x, tilebounds.Height-2, tilebounds.X, tilebounds.Y);
            }
            for(y = 1; y < tilebounds.Height-1; ++y)
            {
                process_outline_tile(tiles, 1, y, tilebounds.X, tilebounds.Y);
                process_outline_tile(tiles, tilebounds.Width-2, y, tilebounds.X, tilebounds.Y);
            }


            Logger.Log(LogLevel.Info, "eow", "------");

                for(y=0; y < tilebounds.Height; ++y)
            {
                string line = "";
            for(x = 0; x < tilebounds.Width; ++x)
                {
                    if(((x > 0 && x < tilebounds.Width-1)||x == 1 || x == tilebounds.Width-2) && (y == 1 || y == tilebounds.Height-2))
                    {
                        line+= String.Format("{0:X}", get_case_idx(tiles, x, y));
                    }
                    else
                    {
                        if(tiles[x,y] == 0)
                        {
                            line += ".";
                        }
                        else
                        {
                            line += "#";
                        }
                    }
                }
                Logger.Log(LogLevel.Info, "eow", line);
            }
            Logger.Log(LogLevel.Info, "eow", "------");



        } 

        public override void Render()
        {

            Camera camera = SceneAs<Level>().Camera;
            if (!(base.Right < camera.Left || base.Left > camera.Right || base.Bottom < camera.Top || base.Top > camera.Bottom))
            {

                Vector2 block_offset = base.Position + shake;

                //background
                Draw.Rect(block_offset.X, block_offset.Y, base.Width, base.Height, playerHasDreamDash ? activeBackColor : disabledBackColor);
                Vector2 camera_position = SceneAs<Level>().Camera.Position;            
                int scale;
                int aidx;
     
                for(int layer = 0; layer < N_layers; ++layer)
                {
    //                scale = 4*(2-layer);
                    scale = 8;
                    aidx = animation_index[layer];
                    Color color = fill_colors[layer];

                    Vector2 offset = camera_position * (0.3f+0.25f*layer);
                    //The upper left corner of the upper left tile that is complete inside this block
                    Vector2 idx_offset = (block_offset-offset)/scale;
                    idx_offset.X = (float)Math.Ceiling(idx_offset.X)*scale;
                    idx_offset.Y = (float)Math.Ceiling(idx_offset.Y)*scale;
                    int x_idx_base = (int)(idx_offset.X/scale);
                    int y_idx_base = (int)(idx_offset.Y/scale);

                    idx_offset += offset;

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
                    idx = textures.get_fill_index(x_idx_base-1,y_idx_base-1,aidx);
                    texture = textures.fill[idx];

                    clip_mask = texture.GetRelativeRect(dx, dy_top, texture.ClipRect.Width-dx, texture.ClipRect.Height-dy_top);
                    Draw.SpriteBatch.Draw(texture.Texture.Texture_Safe, 
                            new Vector2(block_offset.X, block_offset.Y), clip_mask, 
                            color, 0, new Vector2(0,0), scale/8, 0, 0
                            );

                    //Inner edge
                    y = y_idx_base;
                    for(ypos = idx_offset.Y; ypos+scale < block_offset.Y+base.Height; ypos += scale)
                    {
                        idx = textures.get_fill_index(x_idx_base-1,y,aidx);
                        texture = textures.fill[idx];
                        clip_mask = texture.GetRelativeRect(dx, 0, texture.ClipRect.Width-dx, texture.ClipRect.Height);
                        Draw.SpriteBatch.Draw(texture.Texture.Texture_Safe, 
                                new Vector2(block_offset.X, ypos), clip_mask, 
                                color, 0, new Vector2(0,0), scale/8, 0, 0
                                );
                        ++y;
                    }

                    //LL corner
                    dy = (int)Math.Round((block_offset.Y+base.Height-ypos)*8/scale, MidpointRounding.AwayFromZero);
                    if(dy > 0)
                    {
                        idx = textures.get_fill_index(x_idx_base-1,y,aidx);
                        texture = textures.fill[idx];

                        clip_mask = texture.GetRelativeRect(dx, 0, texture.ClipRect.Width-dx, dy);
                        Draw.SpriteBatch.Draw(texture.Texture.Texture_Safe, 
                                new Vector2(block_offset.X, ypos), clip_mask, 
                                color, 0, new Vector2(0,0), scale/8, 0, 0
                                );
                    }

                    //Lengthwise interior
                    for(xpos = idx_offset.X; xpos+scale < block_offset.X+base.Width; xpos += scale)
                    {
                        idx = textures.get_fill_index(x,y_idx_base-1,aidx);
                        texture = textures.fill[idx];

                        clip_mask = texture.GetRelativeRect(0, dy_top, texture.ClipRect.Width, texture.ClipRect.Height-dy_top);
                        Draw.SpriteBatch.Draw(texture.Texture.Texture_Safe, 
                                new Vector2(xpos, block_offset.Y), clip_mask, 
                                color, 0, new Vector2(0,0), scale/8, 0, 0
                                );


                        //Strict interior
                        y = y_idx_base;
                        for(ypos = idx_offset.Y; ypos+scale < block_offset.Y+base.Height; ypos += scale)
                        {


                            idx = textures.get_fill_index(x,y,aidx);
                            texture = textures.fill[idx];
                            Draw.SpriteBatch.Draw(texture.Texture.Texture_Safe, 
                                    new Vector2(xpos, ypos), texture.ClipRect, 
                                    color, 0, new Vector2(0,0), scale/8, 0, 0
                                    );
                            ++y;
                        }
                        dy = (int)Math.Round((block_offset.Y+base.Height-ypos)*8/scale, MidpointRounding.AwayFromZero);
                        if(dy > 0)
                        {
                            idx = textures.get_fill_index(x,y,aidx);
                            texture = textures.fill[idx];

                            clip_mask = texture.GetRelativeRect(0, 0, texture.ClipRect.Width, dy);
                            Draw.SpriteBatch.Draw(texture.Texture.Texture_Safe, 
                                    new Vector2(xpos, ypos), clip_mask, 
                                    color, 0, new Vector2(0,0), scale/8, 0, 0
                                    );
                        }


                        ++x;
                    }

                    //Rightmost edge

                    dx = (int)Math.Round((block_offset.X+base.Width-xpos)*8/scale, MidpointRounding.AwayFromZero);
                    if(dx > 0)
                    {

                        //UR corner
                        idx = textures.get_fill_index(x,y_idx_base-1,aidx);
                        texture = textures.fill[idx];

                        clip_mask = texture.GetRelativeRect(0, dy_top, dx, texture.ClipRect.Height-dy_top);
                        Draw.SpriteBatch.Draw(texture.Texture.Texture_Safe, 
                                new Vector2(xpos, block_offset.Y), clip_mask, 
                                color, 0, new Vector2(0,0), scale/8, 0, 0
                                );

                        //Inner edge
                        y = y_idx_base;
                        for(ypos = idx_offset.Y; ypos+scale < block_offset.Y+base.Height; ypos += scale)
                        {


                            idx = textures.get_fill_index(x,y,aidx);
                            texture = textures.fill[idx];
                            clip_mask = texture.GetRelativeRect(0, 0, dx, texture.ClipRect.Height);
                            Draw.SpriteBatch.Draw(texture.Texture.Texture_Safe, 
                                    new Vector2(xpos, ypos), clip_mask, 
                                    color, 0, new Vector2(0,0), scale/8, 0, 0
                                    );
                            ++y;
                        }

                        //LR corner
                        dy = (int)Math.Round((block_offset.Y+base.Height-ypos)*8/scale, MidpointRounding.AwayFromZero);
                        if(dy > 0)
                        {
                            idx = textures.get_fill_index(x,y,aidx);
                            texture = textures.fill[idx];

                            clip_mask = texture.GetRelativeRect(0, 0, dx, dy);
                            Draw.SpriteBatch.Draw(texture.Texture.Texture_Safe, 
                                    new Vector2(xpos, ypos), clip_mask, 
                                    color, 0, new Vector2(0,0), scale/8, 0, 0
                                    );
                        }

                    }

                }
            }
        }

        public static void render_outline()
        {
            foreach(KeyValuePair<int [], OutlineChunk> entry in outline_chunks)
            {
                int x = entry.Key[0];
                int y = entry.Key[1];
                //This is kind of stupid, because x and y both tell you this
                //but i am paranoid that they might not
                float xpos = x*8;
                float ypos = y*8;
                int aidx = Math.Abs(5*x+7*y)%3;
                entry.Value.render(xpos, ypos, x, y, outline_animation_index[aidx]);
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
        }
    }
}

