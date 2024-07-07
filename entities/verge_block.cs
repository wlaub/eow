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
            On.Celeste.DreamBlock.ActivateNoRoutine += activate_node_routine;
            On.Celeste.DreamBlock.DeactivateNoRoutine += deactivate_node_routine;
            On.Celeste.Level.LoadLevel += load_level_hook;
            On.Monocle.Engine.Update += static_update;
            Everest.Events.Level.OnExit += on_exit_hook;
            Everest.Events.Level.OnEnter += on_enter_hook;
            Everest.Events.Level.OnTransitionTo += transition_hook;

            loaded = true;
        } 
        public static void Unload()
        {
            if(!loaded) return;

            On.Celeste.Player.OnCollideV -= on_collide_v;
            On.Celeste.DreamBlock.ActivateNoRoutine -= activate_node_routine;
            On.Celeste.DreamBlock.DeactivateNoRoutine -= deactivate_node_routine;
            On.Celeste.Level.LoadLevel -= load_level_hook;
            On.Monocle.Engine.Update -= static_update;
            Everest.Events.Level.OnEnter -= on_enter_hook;
            Everest.Events.Level.OnExit -= on_exit_hook;
            Everest.Events.Level.OnTransitionTo -= transition_hook;

            clear_state();

            loaded = false;
        }

        public static void transition_hook(Level level, LevelData next, Vector2 direction)
        {
// Logger.Log(LogLevel.Info, "eow", $"transition hook {direction}");
            transition_dir = direction;
            disabled_position = level.Camera.Position;
        }

        public static void on_exit_hook(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow)
        {
            Unload();
        }

        public static void on_enter_hook(Session session, bool fromSaveData)
        {
            clear_state();
        }

        public static void load_level_hook(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            if(isFromLoader)
            {
                clear_state();
            }
            orig(self, playerIntro, isFromLoader) ;
        }

        public static void static_update(On.Monocle.Engine.orig_Update orig, Engine self, GameTime gameTime)
        {
            orig(self, gameTime);

            Level level = self.scene as Level;
            if(level ==  null)
            {
                return;
            }

            //everyone loves
            bool active2 = has_dream_dash && white_fill<=0f;
            
            outline_color = (active2) ? activeLineColor :  disabledLineColor;
            if (white_fill > 0f)
            {
                outline_color = Color.Lerp(disabledLineColor, activeLineColor, white_fill);
            }

            if(active2 && !level.Paused)
            {
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
                    if(block != null && block.fall_enter_enable && block.playerHasDreamDash && Math.Abs(player.Speed.Y) > block.fall_threshold)
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

        public static int N_layers = 3;

        public static float fill_animation_rate = 0.2f;
        public static float outline_animation_rate = 0.3f;
        public static int[] animation_index = {0,0,0};
        public static float[] animation_timers = {0,fill_animation_rate/3,3*fill_animation_rate/3};
        public static int[] outline_animation_index = {0,0,0};
        public static float[] outline_animation_timers = {0,outline_animation_rate/3,2*outline_animation_rate/3};

        public static Color outline_color = Color.White;
        public static Vector2 disabled_position = new Vector2(420, 69);
//        public static bool active;
        public static float white_fill;
        public static bool has_dream_dash;
        public bool prime_end_of_activate = false;
        public static Vector2 global_shake;

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
                get_texture(x,y,i).Draw(new Vector2(xpos+x_off, ypos+y_off), Vector2.Zero, outline_color);
            }
 
        }

        public class Outline
        {
            public Dictionary<Tuple<int,int>,OutlineChunk> outline_chunks = new();
            public float left;
            public float right;
            public float top;
            public float bot;

            public Outline(float l, float r, float t, float b)
            {
                left = l;
                right = r;
                top = t;
                bot = b;
            }

            public void expand(float l, float r, float t, float b)
            {
                if(l < left) left = l;
                if(r > right) right = r;
                if(t < top) top = t;
                if(b > bot) bot = b;
            }

            public void add_point(int xpos, int ypos, OutlineChunk value)
            {
                expand(xpos*8, xpos*8+8, ypos*8, ypos*8+8);
                Tuple<int,int> key = Tuple.Create(xpos, ypos);
                if(!outline_chunks.ContainsKey(key))
                {
                    outline_chunks.Add(key, value);
                }
            }

            public void render(Camera camera)
            {
                if (right < camera.Left || left > camera.Right || bot < camera.Top || top > camera.Bottom)
                {
                    return;
                }
                foreach(KeyValuePair<Tuple<int, int>, OutlineChunk> entry in outline_chunks)
                {
                    int x = entry.Key.Item1;
                    int y = entry.Key.Item2;
                    //This is kind of stupid, because x and y both tell you this
                    //but i am paranoid that they might not
                    float xpos = x*8;
                    float ypos = y*8;
                    int aidx = Math.Abs(5*x+7*y)%3;
                    entry.Value.render(xpos, ypos, x, y, outline_animation_index[aidx]);
                }
            }
        }

        public class OutlineEntity : Entity
        {
            public static Dictionary<Tuple<string, int>, OutlineEntity> entity_registry = new();
             public static Dictionary<Tuple<string, int>, Outline> outline_registry = new();
           
            public string room; 
            public Outline outline;

            public static void clear()
            {
                outline_registry.Clear();
                foreach(KeyValuePair<Tuple<string, int>, OutlineEntity> entry in entity_registry)
                {
                    entry.Value.RemoveSelf();
                }
                entity_registry.Clear();
 
            }

            public static OutlineEntity get_outline(VergeBlock block)
            {
                Level level = block.SceneAs<Level>();
                string room = level.Session.Level;
                int depth = block.Depth-1;
                Tuple<string, int> key = Tuple.Create(room, depth);

                if(entity_registry.ContainsKey(key))
                {
                    return entity_registry[key];
                }

                Outline _outline;
                if(!outline_registry.ContainsKey(key))
                {
                    _outline = new(block.X, block.Y, block.Width, block.Height);
                    outline_registry.Add(key, _outline);
                }
                else
                {
                    _outline = outline_registry[key];
                }

                OutlineEntity result = new(room, depth, _outline);

                level.Add(result);
                entity_registry.Add(key, result);
                return result;
               
            }

            public OutlineEntity(string room, int depth, Outline o)
            {
                this.room =room;
                base.Depth = depth;
                outline = o;
            }

            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                Tuple<string, int> key = Tuple.Create(room, base.Depth);

                if(entity_registry.ContainsKey(key))
                {
                    entity_registry.Remove(key);
                }
                else
                {
Logger.Log(LogLevel.Error, "eow", $"somehow already removed {room} {depth}");
                }
            }

            public override void Render()
            {
                base.Render();
                Level level = SceneAs<Level>();
                Camera camera = level.Camera;
 
                outline.render(camera);
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

        public Tween move_tween;
        public Trigger[] triggers;
        public bool activated = false;

        public bool below;
        public float depth_offset_factor;

        public float fall_threshold = 200f;
        public bool fall_enter_enable = true;

        public bool vanilla_render = false;
        public int active_layers = 3;
        public bool animate_fill = true;

        public int trigger_mode;
        public int[] trigger_ids;

        public string texture_name;

        public Color[] fill_colors;
        public Color[] flag_fill_colors;
        public string[] layer_flags;

        public VergeBlockTexture textures; 

        public Rectangle tilebounds;
        public Rectangle real_tilebounds;

        public OutlineEntity outline_entity;
        
        public VergeBlock(EntityData data, Vector2 offset, EntityID id) : base(data, offset)
        {
            this.id = id;

            depth_offset_factor = data.Float("depth_offset_factor", 1);

            base.Depth = data.Int("depth", -11000);
            below = data.Bool("below");
            if(below)
            {
                //Default = -11000
                //Below = 5000
                base.Depth += 16000;
            }

            //TODO no_outline option?
            string trigger_names = data.Attr("trigger_ids");
            if(trigger_names != "")
            {
                string[] trigger_parts = trigger_names.Split(',');
                trigger_ids = new int[trigger_parts.Length];
                triggers = new Trigger[trigger_parts.Length];
                for(int i = 0; i < trigger_parts.Length; ++i)
                {
                    if(!Int32.TryParse(trigger_parts[i], out trigger_ids[i]))
                    {
    Logger.Log(LogLevel.Error, "eow", $"failed to parse trigger id {trigger_parts[i]}");
                    }
                }
            }

            fall_threshold = data.Float("fall_threshold");
            fall_enter_enable = data.Bool("fall_enter");

            texture_name = data.Attr("texture", "objects/eow/VergeBlock");
            textures = new(texture_name);

            active_layers = data.Int("layer_count");
            if(active_layers < 0)
                active_layers = 0;
            if(active_layers > 3)
                active_layers = 3;

            vanilla_render = data.Bool("vanilla_render");

            animate_fill = data.Bool("animate_fill");

            fill_colors = new Color[N_layers];
            flag_fill_colors = new Color[N_layers];
            layer_flags = new string[N_layers];
            fill_colors[0] = Calc.HexToColor(data.Attr("layer_0_color"));
            fill_colors[1] = Calc.HexToColor(data.Attr("layer_1_color"));
            fill_colors[2] = Calc.HexToColor(data.Attr("layer_2_color"));
            flag_fill_colors[0] = Calc.HexToColor(data.Attr("layer_0_flag_color"));
            flag_fill_colors[1] = Calc.HexToColor(data.Attr("layer_1_flag_color"));
            flag_fill_colors[2] = Calc.HexToColor(data.Attr("layer_2_flag_color"));
            layer_flags[0] = data.Attr("layer_0_flag");
            layer_flags[1] = data.Attr("layer_1_flag");
            layer_flags[2] = data.Attr("layer_2_flag");
//            fill_colors[0] = Calc.HexToColor("00ff00");
//            fill_colors[2] = Calc.HexToColor("ff8800");
//            fill_colors[1] = Calc.HexToColor("0000ff");

            //TODO trigger_mode (on dash, on jumpthrough, on exit)
            // 0 - on fall enter, 1 - on dash enter, 2 - on enter
            // 3 - on fall exit, 4 - on dash exit, 5 - on exit
            trigger_mode = data.Int("trigger_mode");

            int x_start = (int)Position.X/8;
            int y_start = (int)Position.Y/8;
            int twidth = (int)Width/8;
            int theight = (int)Height/8;

            tilebounds = new(x_start-1, y_start-1, twidth+2, theight+2);
            real_tilebounds = new(x_start, y_start, twidth, theight);

        }

        public static Vector2 transition_dir = Vector2.Zero;

        public int get_case_idx(int [,] tiles, int x, int y)
        {
            int case_index = 0;
            case_index += tiles[x, y+1];
            case_index += tiles[x+1, y] << 1;
            case_index += tiles[x, y-1] << 2;
            case_index += tiles[x-1, y] << 3;
            return case_index;

        }

        public void process_outline_tile(Rectangle bounds, int[,] tiles, int x, int y, int x_off, int y_off)
        {
            int xpos = x+x_off;
            int ypos = y+y_off;

            if( xpos < bounds.Left || xpos > bounds.Right ||
                ypos < bounds.Top || ypos > bounds.Bottom)
            {
//Logger.Log(LogLevel.Info, "eow", $"exclude tile {xpos},{ypos} because {bounds.Left} {bounds.Right} {bounds.Top} {bounds.Bottom}");
                return;
            }

            int case_index = 0;
            case_index += tiles[x, y+1];
            case_index += tiles[x+1, y] << 1;
            case_index += tiles[x, y-1] << 2;
            case_index += tiles[x-1, y] << 3;

            OutlineChunk selection = textures.outline_lookup[case_index];

            if(selection.textures != null && selection.textures.Length > 0)
            {
                outline_entity.outline.add_point(xpos, ypos, selection);
            }
        }

        public static void clear_state()
        {
            OutlineEntity.clear();
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
        }

        public void start_move(Scene scene)
        {
            ///Pulled from the original source and modified to permit starting and stopping
            if(move_tween != null)
            {
                move_tween.Active = true;
                return;
            }

            if (node.HasValue)
            {
                Vector2 start = Position;
                Vector2 end = node.Value;
                float num = Vector2.Distance(start, end) / 12f;
                if (fastMoving)
                {
                    num /= 3f;
                }
                move_tween = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.SineInOut, num, start: true);

                move_tween.OnUpdate = delegate(Tween t)
                {
                    double num2 = (double)start.X + ((double)end.X - (double)start.X) * (double)t.Eased;
                    double num3 = (double)start.Y + ((double)end.Y - (double)start.Y) * (double)t.Eased;
                    float moveH = (float)(num2 - (double)Position.X - (double)_movementCounter.X);
                    float moveV = (float)(num3 - (double)Position.Y - (double)_movementCounter.Y);
                    if (Collidable)
                    {
                        MoveH(moveH);
                        MoveV(moveV);
                    }
                    else
                    {
                        MoveHNaive(moveH);
                        MoveVNaive(moveV);
                    }
                };
                Add(move_tween);
            }
 
        }

        public void stop_move()
        {
            if(move_tween == null)
            {
                return;
            }
            move_tween.Active = false;
        }

        public void orig_added(Scene scene)
        {
            playerHasDreamDash = SceneAs<Level>().Session.Inventory.DreamDash;

            if(playerHasDreamDash)
            {
                start_move(scene);
            }
            else
            {
                Add(occlude = new LightOcclude());
            }
            Setup();
        }

        public void entity_added(Scene scene)
        {
            Scene = scene;
            if (Components != null)
            {
                foreach (Component component in Components)
                {
                    component.EntityAdded(scene);
                }
            }
            Scene.SetActualDepth(this);
        }

        public override void Added(Scene scene)
        {
            Load();
            entity_added(scene);

            orig_added(scene);

            if(trigger_ids != null)
            {
                for(int i = 0; i < trigger_ids.Length; ++i)
                {
                    Trigger result = TriggerManager.make_trigger((scene as Level), trigger_ids[i]);
                    if(result == null)
                    {
Logger.Log(LogLevel.Error, "eow", $"Failed to instantiate trigger {trigger_ids[i]}");
                    }
                    else
                    {
                        result.Scene = scene;
                        triggers[i] = result;
                    }
                }
            }

            outline_entity = OutlineEntity.get_outline(this);
        }
 
        public void resume_dream_dash(Player player)
        {
            if(!playerHasDreamDash)
            {
                return;
            }
            player.dreamBlock = this;
            player.dashAttackTimer = 0f;
            player.DashDir = transition_dir;
            if(player.DashDir == Vector2.Zero)
            {
                player.DashDir = new Vector2((float)player.Facing,0);
            }
            player.StateMachine.State=Player.StDreamDash;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);

            Level level = (scene as Level);


            if(triggers != null)
            {
                foreach(Trigger trigger in triggers)
                {
                    if(trigger != null)
                    {
                        trigger.Awake(scene);

                    }
                }
            }

            Player player = CollideFirst<Player>(Position);
            if(player != null)
            {
                resume_dream_dash(player);
            }

            if(vanilla_render)
            {
                return;
            }

            if(outline_entity == null)
            {
                Logger.Log(LogLevel.Error, "eow", $"verge block has no outline entity at awake!");
            }

            Rectangle level_bounds = new(level.Bounds.X/8, level.Bounds.Y/8, level.Bounds.Width/8-1, level.Bounds.Height/8);

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
                if(block == this || block.vanilla_render || block.Depth > this.Depth)
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

/* 
//This includes adjacent tiles in the grid so that the outline
//won't go through them, but due it looks bad
//would be better to be able to render outline chunks at a specific layer instead of on top of everything
            if(this.below)
            {
                Grid grid = level.SolidTiles.Grid;
                int gx = (int)(level.SolidTiles.Position.X/8);
                int gy = (int)(level.SolidTiles.Position.Y/8);
                int dx, dy;
                for(x = 0; x < tilebounds.Width; ++x)
                {
                    dx = tilebounds.X + x - gx;
                    dy = tilebounds.Y - gy;
                    {
                        tiles[x,0] = 0;
                    }
                    dy = tilebounds.Bottom - gy;
                    if(level.SolidTiles.Grid[dx, dy])
                    {
                        tiles[x,tilebounds.Height-1] = 0;
                    }

                }
            }
*/

            for(x = 1; x < tilebounds.Width-1; ++x)
            {
                process_outline_tile(level_bounds, tiles, x, 1, tilebounds.X, tilebounds.Y);
                process_outline_tile(level_bounds, tiles, x, tilebounds.Height-2, tilebounds.X, tilebounds.Y);
            }
            for(y = 1; y < tilebounds.Height-1; ++y)
            {
                process_outline_tile(level_bounds, tiles, 1, y, tilebounds.X, tilebounds.Y);
                process_outline_tile(level_bounds, tiles, tilebounds.Width-2, y, tilebounds.X, tilebounds.Y);
            }

/*
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
*/


        } 

        public override void Render()
        {
            if(vanilla_render)
            {
                base.Render();
                return;
            }

            Level level = SceneAs<Level>();

            Camera camera = level.Camera;
            if (!(base.Right < camera.Left || base.Left > camera.Right || base.Bottom < camera.Top || base.Top > camera.Bottom))
            {

                Vector2 block_offset = base.Position + global_shake;

                //background
                Draw.Rect(block_offset.X, block_offset.Y, base.Width, base.Height, playerHasDreamDash ? activeBackColor : disabledBackColor);
                Vector2 camera_position;

                if(playerHasDreamDash)
                {
                    camera_position = camera.Position;
                }
                else
                {
                    if(whiteFill > 0f)
                    {
                        camera_position = Vector2.Lerp(disabled_position, camera.Position, whiteFill);
                    }
                    else
                    {
                        camera_position = disabled_position;
                    }
                }
                int scale;
                int aidx;
    
                //Infill 
                for(int layer = 0; layer < active_layers; ++layer)
                {
    //                scale = 4*(2-layer);
                    scale = 8;
                    if(animate_fill)
                    {
                        aidx = animation_index[layer];
                    }
                    else
                    {
                        aidx = 0;
                    }

                    Color color;
                    if(layer_flags[layer] == "" || !level.Session.GetFlag(layer_flags[layer]))
                    {
                        color = fill_colors[layer];
                    }
                    else
                    {
                        color = flag_fill_colors[layer];
                    }

                    float depth_offset = 1f-Math.Max(0,(base.Depth+11000)*0.55f/16000f)*depth_offset_factor;
                    Vector2 offset = camera_position * (0.3f+0.25f*layer)*depth_offset;
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
                if (whiteFill > 0f)
                {
                    Draw.Rect(block_offset.X, block_offset.Y, base.Width, base.Height * whiteHeight, Color.White * whiteFill);
                }

            }
        }

        public void activate(Player player)
        {
            if(activated || triggers == null) return;

            foreach(Trigger trigger in triggers)
            {
                if(trigger != null)
                {
                    trigger.OnEnter(player);
                    trigger.OnStay(player);
                }
            }
//            activated = true;
        }

        public static void activate_node_routine(On.Celeste.DreamBlock.orig_ActivateNoRoutine orig, DreamBlock self)
        {
            orig(self);
            if(self is VergeBlock)
            {
                (self as VergeBlock).prime_end_of_activate = true;
            }
        }        

        public static void deactivate_node_routine(On.Celeste.DreamBlock.orig_DeactivateNoRoutine orig, DreamBlock self)
        {
            orig(self);
            if(self is VergeBlock)
            {
                (self as VergeBlock).stop_move();
            }
        }        



        public void switch_on()
        {
            prime_end_of_activate = false;
            start_move(Scene);
        }
        
        public void switch_off()
        {
        }

        public override void Update()
        {
            base.Update();
            has_dream_dash = playerHasDreamDash;
            white_fill = whiteFill;
            global_shake = shake;
            if(prime_end_of_activate && white_fill <= 0f)
            {
                switch_on();

            }
        }
    }
}

