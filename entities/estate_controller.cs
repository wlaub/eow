
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

using Microsoft.Xna.Framework;

using Monocle;

using MonoMod.Utils;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Mono.Cecil.Cil;

using Celeste;
using Celeste.Mod.Entities;

namespace Celeste.Mod.ErrandOfWednesday
{

    public class DraftMenu : Entity
    {
        public int selection = 0;

        public List<EstateRoomInfo> options = new();

        public int target_x;
        public int target_y;

        public Action on_finish;

        public DraftMenu(List<EstateRoomInfo> pool, int target_x, int target_y, Action on_finish=null)
        {
            this.target_x = target_x;
            this.target_y = target_y;
            this.on_finish = on_finish;

            base.Tag = Tags.HUD | Tags.FrozenUpdate;
            while(options.Count < 3 && pool.Count > 0)
            {
                EstateRoomInfo new_option = Calc.Random.Choose(pool);
                pool.RemoveAll( x => x == new_option);
                options.Add(new_option);

                Add(new_option.sprite);

                int idx = options.Count-1;
                //w=320, gap=160
                //m=320
                new_option.sprite.Position = new Vector2(320f+160f+idx*(320f+160f), 320f);
            }

            EstateGrid grid = EstateController.grid;
            float w,h;
            w = options[0].sprite.Width/4;
            h = options[0].sprite.Height/4;
            float offx = 49f;
            float offy = 1080f-49f-h*grid.grid_height;
 
            foreach (string room_key in grid.room_position.Keys)
            {
                Vector2 pos = grid.room_position[room_key];
                EstateRoomInfo room = EstateController.rooms[room_key];
                room.sprite.Scale = new Vector2(0.25f,0.25f);
                room.sprite.Position.X = offx+(pos.X+0.5f)*w;
                room.sprite.Position.Y = offy+(pos.Y+0.5f)*h;
                Add(room.sprite);
            }

            int gx,gy;
            grid.w2g(target_x, target_y, out gx, out gy);

            

            Add(new Coroutine(routine()));
        }

        public IEnumerator routine()
        {
            Level level = SceneAs<Level>();
            level.Frozen = true;
            while(true)
            {
                if(Input.MenuConfirm.Pressed)
                {
                    //TODO this causes the player to jump immediately
                    Input.MenuConfirm.ConsumeBuffer();
                    EstateRoomInfo draft = options[selection];
                    EstateController.drafted_rooms.Add(draft.key); 
                    EstateController.grid.add_room_world(draft.key, target_x, target_y);
                    EstateController.move_room(level, draft.key, target_x, target_y);
                    break;                    
                }
                if(Input.MenuLeft.Pressed && selection > 0)
                {
                    --selection;
                }
                if(Input.MenuRight.Pressed && selection < options.Count-1)
                {
                    ++selection;
                }
                yield return null;
            }
            level.Frozen = false;


            if(on_finish is not null)
            {
                on_finish();
            }

            RemoveSelf();
        }

        public override void Render()
        {
            base.Render();
            for(int i = 0; i < options.Count; ++i)
            {
                EstateRoomInfo option = options[i];

                float cx = option.sprite.Position.X;
                float cy = option.sprite.Position.Y;

                if(i == selection)
                {
                    option.sprite.Color = Color.White; 
                }
                else
                {
                    option.sprite.Color = Color.White*0.5f; 
                }

                ActiveFont.Draw(option.display_name, 
                    new Vector2(option.sprite.Position.X, option.sprite.Position.Y+option.sprite.Height/2+7),
                    new Vector2(0.5f, 0f), Vector2.One,
                    Color.White
                    );
             }

        }


    }


    public class EstateGrid 
    {
        public int room_width; //size of room
        public int room_height;
        public int grid_width; //rows and cols
        public int grid_height;
        public int top_world; //location of room 0,0 upper left corner
        public int left_world;

        public int rw_world;
        public int rh_world;

//        public string[,] rooms;
        public Dictionary<Tuple<int,int>, string> position_room;
        public Dictionary<string, Vector2> room_position;
 
        public EstateGrid(int gw, int gh, int rw, int rh, int top, int left) 
        {
            grid_width=gw;
            grid_height=gh;
            room_width=rw;
            room_height=rh;
            top_world=top;
            left_world=left;
            rw_world = room_width*8;
            rh_world = room_height*8;

            room_position = new();
            position_room = new();
        }

        public void populate_grid(Session session)
        {
//Logger.Log(LogLevel.Info, "eow", $"populate grid {grid_width} {grid_height}!!!!!!!!!!!!!!!!!!");
            for(int x = 0; x < grid_width; ++x)
            {
                for(int y = 0; y < grid_height; ++y)
                {

                    Vector2 world_pos = new Vector2(left_world+x*rw_world, top_world+y*rh_world);
                    foreach(LevelData room in session.MapData.Levels)
                    {
                        if(room.Position == world_pos)
                        {
                            add_room(room.Name, x, y);
                        }
                    }
                   
                }
            }
           
        }

        public void w2g(int wx, int wy, out int gx, out int gy)
        {
            gx = (int)(wx-left_world)/rw_world;
            gy = (int)(wy-top_world)/rh_world;
        }
 
        public void add_room(string room_key, int x, int y)
        {
            position_room[Tuple.Create(x,y)] = room_key;
            room_position[room_key] = new Vector2(x,y);
 Logger.Log(LogLevel.Info, "eow", $"added room {room_key} at {x},{y}");           
        }
        public void add_room_world(string room_key, int wx, int wy)
        {
            int x,y;
            w2g(wx, wy, out x, out y);
            add_room(room_key, x, y);
            
        }



    }


    [Tracked]
    [CustomEntity("eow/EstateController")]
    public class EstateController : Entity
    {

        public static bool loaded = false;
        public static EstateGrid grid;
        public static int room_width;
        public static int room_height;
        public static int grid_width;
        public static int grid_height;

        public static Dictionary<string, EstateRoomInfo> rooms = new();
        public static HashSet<string> drafted_rooms = new();

        public static int camera_margin;

        public EstateController(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            //TODO i wonder if you can make a fake wall/dash block that just like batches up all the entities and runs all through the autotiler together somehow
            //like essentially just make a virtualmap<char> that is a composite of all the dash blocks in the room and then pass that whole thing to the autotiler once to generate a single overlay, and then regenerate that whenever one gets removed.
        }

        /* Actual implementation */

        public static Hook camera_target_hook;

        public static void register_commands()
        {
            FrostHelperImports.RegisterFunctionSessionExpressionCommand?.Invoke("eow", "e_from",
                (session, args) =>
                {
                    return (string)args[0] == drafting_context.from_level.Name ? 1:0;
                }
                );
            FrostHelperImports.RegisterFunctionSessionExpressionCommand?.Invoke("eow", "e_drafted",
                (session, args) =>
                {
                    return drafted_rooms.Contains((string)args[0])?1:0;
                }
                );
            FrostHelperImports.RegisterSimpleSessionExpressionCommand?.Invoke("eow", "e_into_left",
                (session) => { return drafting_context.into_left; } );
            FrostHelperImports.RegisterSimpleSessionExpressionCommand?.Invoke("eow", "e_into_right",
                (session) => { return drafting_context.into_right; } );
            FrostHelperImports.RegisterSimpleSessionExpressionCommand?.Invoke("eow", "e_into_top",
                (session) => { return drafting_context.into_top; } );
            FrostHelperImports.RegisterSimpleSessionExpressionCommand?.Invoke("eow", "e_into_bot",
                (session) => { return drafting_context.into_bot; } );
            FrostHelperImports.RegisterSimpleSessionExpressionCommand?.Invoke("eow", "e_into_x",
                (session) => { return drafting_context.into_x; } );
            FrostHelperImports.RegisterSimpleSessionExpressionCommand?.Invoke("eow", "e_into_y",
                (session) => { return drafting_context.into_y; } );
 
            //e_from_*
            //e_left_of()
            //e_right_of()
            //e_above_of()
            //e_below_of()
            FrostHelperImports.RegisterSimpleSessionExpressionCommand?.Invoke("eow", "e_pool_depth",
                (session) => { return drafting_context.pool_depth; } );

        }

        public static void unload()
        {
            if(!loaded)
            {
                return;
            }

            camera_target_hook?.Dispose();
            camera_target_hook = null;

            loaded = false;
        }
       
        public static void clear_save(Session session)
        {
            rooms.Clear();
            drafted_rooms.Clear(); 
           
            ErrandOfWednesdayModuleSession mod_session = ErrandOfWednesdayModule.Session;
            if(mod_session.estate_state is not null)
            {
                mod_session.estate_state = new();
            }
           
        }

        public static void try_load(Session session)
        {

            LevelData level_data = session.MapData.Get("!eow");
            if(level_data == null)
            {
                level_data = session.MapData.Get("~eow");
            }
            if(level_data == null)
            {
Logger.Log(LogLevel.Debug, "eow", "Didn't find the eye.");
                return;
            }
Logger.Log(LogLevel.Debug, "eow", "Estate mode activate."); 

            //Find the controller
            EntityData data = null;
            foreach(EntityData entity_data in level_data.Entities)
            {
                if(entity_data.Name == "eow/EstateController")
                {
                    data = entity_data;
                }
            }

            if (data == null)
            {
                return;
            }

            session.MapData.Reload();       
            rooms.Clear();
            drafted_rooms.Clear(); 

            ErrandOfWednesdayModuleSession mod_session = ErrandOfWednesdayModule.Session;

            if(mod_session.estate_state is null)
            {
                mod_session.estate_state = new();
            }
            {
Logger.Log(LogLevel.Debug, "eow", "found existing estate state"); 
            }

            room_width = data.Int("room_width");
            room_height = data.Int("room_height");
            grid_width = data.Int("grid_width");
            grid_height = data.Int("grid_height");

            camera_margin = data.Int("camera_margin", 16);

//            data.Nodes[0];
            Vector2 grid_ul = Vector2.Zero;
            if(data.Nodes.Length > 0)
            {
                grid_ul = data.Nodes[0]+level_data.Position;
            }
            grid = new EstateGrid(grid_width, grid_height, room_width, room_height, (int)grid_ul.Y, (int)grid_ul.X);

            grid.populate_grid(session);

            int[] e = {0,0,0,0};
            foreach(LevelData room_data in session.MapData.Levels)
            {
                foreach(EntityData entity_data in room_data.Entities)
                {
                    if(entity_data.Name == "eow/EstateRoom")
                    {
//Logger.Log(LogLevel.Info, "eow", $"loading room {room_data.Name}");
                        EstateRoomInfo info = new(room_data, entity_data);
                        rooms[info.key] = info;
                        for(int i = 0; i < 4; ++i)
                            e[i] += info.entries[i]?1:0; 
                    }
                }
               
            }

Logger.Log(LogLevel.Info, "eow", $"l,r,t,d={string.Join(",", e)}");

            foreach(string room_key in grid.room_position.Keys)
            {
                drafted_rooms.Add(room_key);
            }

            foreach(EstateRoomState room_state in mod_session.estate_state.drafted_rooms.Values)
            {
                LevelData target_data = session.MapData.Get(room_state.key);
//Logger.Log(LogLevel.Info, "eow", $"re-drafting room {room_state.key}");

                drafted_rooms.Add(room_state.key);
                grid.add_room_world(room_state.key, room_state.xpos, room_state.ypos);

                int target_x = room_state.xpos;
                int target_y = room_state.ypos;

                int start_x = target_data.Bounds.X;
                int start_y = target_data.Bounds.Y;
                int off_x = target_x-start_x;
                int off_y = target_y-start_y;


                target_data.Bounds.X = target_x;
                target_data.Bounds.Y = target_y;

                for(int i = 0; i < target_data.Spawns.Count; ++i)
                {
                    Vector2 spawn = target_data.Spawns[i];
                    spawn.X += off_x;
                    spawn.Y += off_y;
                    target_data.Spawns[i] = spawn;
                }
                for(int i = 0; i < target_data.Spawns.Count; ++i)
                {
                    Vector2 spawn = target_data.Spawns[i];
                }

            MapData map_data = session.MapData;
            int left = map_data.Bounds.Left;
            int right = map_data.Bounds.Right;
            int top = map_data.Bounds.Top;
            int bot = map_data.Bounds.Bottom;

            foreach (LevelData _level in map_data.Levels)
            {
                left = Math.Min(left, _level.Bounds.Left);
                right = Math.Max(right, _level.Bounds.Right);
                top = Math.Min(top, _level.Bounds.Top);
                bot = Math.Max(bot, _level.Bounds.Top);
            }


            Rectangle old_bounds = map_data.Bounds;
            Rectangle old_tb = map_data.TileBounds;
            int m = 64;
            map_data.Bounds = new Rectangle(left-m, top-m, right-left+2*m, bot-top + 2*m);
               
            }

            // Hooks

            camera_target_hook = new Hook(
                typeof(Player).GetMethod("get_CameraTarget"),
                typeof(EstateController).GetMethod("my_camera_target_hook", BindingFlags.NonPublic | BindingFlags.Static));

            // Done

            Logger.Log(LogLevel.Debug, "eow", $"Finished loading everything");

            loaded = true;

        }

        private static Vector2 my_camera_target_hook(Func<Player, Vector2> orig, Player self)
        {
            Vector2 result = orig(self);
 
            if(self.Scene is not null)
            {
                Level level = (self.Scene as Level);
                Rectangle bounds = level.Session.LevelData.Bounds;

                int m = EstateController.camera_margin;

                EstateGrid grid = EstateController.grid;
                int gx, gy;
                grid.w2g((int)self.Position.X, (int)self.Position.Y, out gx, out gy);
                int l = grid.left_world+gx*grid.rw_world+m;
                int r = grid.left_world+(gx+1)*grid.rw_world-m;
                int t = grid.top_world+gy*grid.rh_world+m;
                int b = grid.top_world+(gy+1)*grid.rh_world-m;

                float clamp_x = MathHelper.Clamp(result.X, bounds.Left+m, bounds.Right-320-m);
                float clamp_y = MathHelper.Clamp(result.Y, bounds.Top+m, bounds.Bottom-180-m);

                if(self.Left >= l && self.Right <= r && self.Top >= t && self.Bottom <= b)
                {
                    result.X = clamp_x;
                    result.Y = clamp_y;
                }
                /*
                if(self.Left < l)
                {
                    result.X = MathHelper.Lerp(clamp_x, result.X, (l-self.Left)/m);
                }
                else if(self.Right > r)
                {
                    result.X = MathHelper.Lerp(clamp_x, result.X, (self.Right-r)/m);
                }
                else
                {result.X=clamp_x;}
                if(self.Top < t)
                {
                    result.Y = MathHelper.Lerp(clamp_y, result.Y, (t-self.Top)/m);
                }
                else if(self.Bottom > b)
                {
                    result.Y = MathHelper.Lerp(clamp_y, result.Y, (self.Bottom-b)/m);
                }
                else
                {result.Y=clamp_y;}
                */

            }
            return result;
        }

        public static DraftingContext drafting_context = null;


        public static List<EstateRoomInfo> make_pool(int side, Session session, int target_x, int target_y)
        {
            List<EstateRoomInfo> pool = new();

            while(pool.Count == 0 && drafting_context.pool_depth < 3)
            {
                foreach(EstateRoomInfo info in rooms.Values)
                {
//    Logger.Log(LogLevel.Info, "eow", $"checking {info.key}");
                    if(info.entries[side] && !drafted_rooms.Contains(info.key))
                    {
                        if( drafting_context.into_top && info.exits[2] ||
                            drafting_context.into_bot && info.exits[3] ||
                            drafting_context.into_left && info.exits[0] ||
                            drafting_context.into_right && info.exits[1]
                            )
                        {
                            continue;
                        }
//    Logger.Log(LogLevel.Info, "eow", $"passing {info.key} with {string.Join(",",info.exits)}");
                        float selection_count = info.selection_count;
                        if(info.session_expression != null)
                        {
                            object result = FrostHelperImports.GetSessionExpressionValue?.Invoke(info.session_expression, session);
    Logger.Log(LogLevel.Info, "eow", $"{info.selection_expression}: {result}");
                            if(result is int)
                            {
                                selection_count *= (int)result;
                            }
                            else if(result is float)
                            {
                                selection_count *= (float)result;
                            }
                            else if(result is bool && !(bool)result)
                            {
                                selection_count = 0;
                            }
                        }

                        for(int i = 0; i < selection_count; ++i)
                        {
                            pool.Add(info);
                        }
                    }
                }
                ++drafting_context.pool_depth;
            }
            return pool;
        }


        public static bool get_draft_target(LevelData from_level, int side, out int target_x, out int target_y)
        {
            switch(side)
            {
                case 0:
                    target_x = from_level.Bounds.Right;
                    target_y = from_level.Bounds.Y;
                    break;
                case 1:
                    target_x = from_level.Bounds.Left-EstateController.room_width*8;
                    target_y = from_level.Bounds.Y;
                    break;
                case 3:
                    target_x = from_level.Bounds.X;
                    target_y = from_level.Bounds.Y-EstateController.room_height*8;
                    break;
                case 2:
                    target_x = from_level.Bounds.X;
                    target_y = from_level.Bounds.Bottom;
                    break; 
                default:
                    target_x=0;
                    target_y=0;
                    return false;
            }
            return true;
        }

    public class DraftingContext{
        public int pool_depth = 0;
        public LevelData from_level;
        public int side;
        public bool into_top;
        public bool into_bot;
        public bool into_left;
        public bool into_right;
        public int into_x;
        public int into_y;
        //TODO from grid pos
        //TODO left, right, above, below rooms
        };


        public static string room_at_world(int world_x, int world_y)
        {
            grid.w2g(world_x, world_y, out world_x, out world_y);
            Tuple<int, int> key= new Tuple<int, int>(world_x, world_y) ; //what is this language's fucking problem

            return grid.position_room.ContainsKey(key) ? grid.position_room[key] : null;

        }

        public static void draft_room(Level level, LevelData from_level, int side, Action on_finish = null)
        {
            int target_x;
            int target_y;
            if(!EstateController.get_draft_target(from_level, side, out target_x, out target_y))
            {
                return;
            }

            drafting_context = new();
            drafting_context.from_level = from_level;
            drafting_context.side = side;

            if(room_at_world(target_x, target_y) != null){return;}

            int gx, gy;
            grid.w2g(target_x, target_y, out gx, out gy);
            drafting_context.into_x = gx;
            drafting_context.into_y = gy;
            drafting_context.into_top = gy==0;
            drafting_context.into_bot = gy==grid.grid_height-1;
            drafting_context.into_left = gx==0;
            drafting_context.into_right = gx==grid.grid_width-1;

//Logger.Log(LogLevel.Info, "eow", $"drafting into {gx},{gy}");

            List<EstateRoomInfo> pool = EstateController.make_pool(side, level.Session, target_x, target_y);

            if(pool.Count == 0){return;}

foreach(EstateRoomInfo option in pool)
{
Logger.Log(LogLevel.Info, "eow", $"  {option.key}");
}

Logger.Log(LogLevel.Info, "eow", $"side={side}");
            EstateRoomInfo draft = Calc.Random.Choose(pool);
Logger.Log(LogLevel.Info, "eow", $"drafting {draft.key}");

            level.Add(new DraftMenu(pool, target_x, target_y, on_finish));
        }

        public static void save_room_state(string key, int xstart, int ystart, int xpos, int ypos)
        {
//Logger.Log(LogLevel.Info, "eow", $"doing the room save"); 
            ErrandOfWednesdayModuleSession mod_session = ErrandOfWednesdayModule.Session;
//Logger.Log(LogLevel.Info, "eow", $"mod session {mod_session}"); 
            if(mod_session.estate_state is null)
            {
                mod_session.estate_state = new();
//Logger.Log(LogLevel.Info, "eow", "did  not found state???"); 
            }
            {



            }



            EstateRoomState new_state = new();
            new_state.xstart=xstart;
            new_state.ystart=ystart;
            new_state.key = key;
            new_state.xpos=xpos;
            new_state.ypos=ypos;
            mod_session.estate_state.drafted_rooms[key] = new_state;
           
        }


        public static void move_room(Level level, string room_name, int target_x, int target_y) 
        {
            Session session = level.Session;


            LevelData level_data = session.MapData.Get(room_name);

            int start_x = level_data.Bounds.X;
            int start_y = level_data.Bounds.Y;
            int off_x = target_x-start_x;
            int off_y = target_y-start_y;

            level_data.Bounds.X = target_x;
            level_data.Bounds.Y = target_y;

            for(int i = 0; i < level_data.Spawns.Count; ++i)
            {
                Vector2 spawn = level_data.Spawns[i];
                spawn.X += off_x;
                spawn.Y += off_y;
                level_data.Spawns[i] = spawn;
            }
            for(int i = 0; i < level_data.Spawns.Count; ++i)
            {
                Vector2 spawn = level_data.Spawns[i];
            }

            regenerate_tilebounds(level, level_data, start_x, start_y, target_x, target_y);

            save_room_state(room_name, start_x, start_y, target_x, target_y);

//            session.MapData.Reload(); //m,aybe need to hook and call this on chaper restart?
//            AssetReloadHelper.ReloadLevel();

        }

        public static void regenerate_tilebounds(Level level, LevelData level_data, int start_x, int start_y, int target_x, int target_y)
        {
            MapData map_data = level.Session.MapData;
            int left = map_data.Bounds.Left;
            int right = map_data.Bounds.Right;
            int top = map_data.Bounds.Top;
            int bot = map_data.Bounds.Bottom;

            foreach (LevelData _level in map_data.Levels)
            {
                left = Math.Min(left, _level.Bounds.Left);
                right = Math.Max(right, _level.Bounds.Right);
                top = Math.Min(top, _level.Bounds.Top);
                bot = Math.Max(bot, _level.Bounds.Top);
            }


            Rectangle old_bounds = map_data.Bounds;
            Rectangle old_tb = map_data.TileBounds;
            int m = 64;
            map_data.Bounds = new Rectangle(left-m, top-m, right-left+2*m, bot-top + 2*m);

            int dl = -map_data.TileBounds.Left+old_tb.Left;
            int dr = map_data.TileBounds.Right-old_tb.Right;
            int dt = -map_data.TileBounds.Top+old_tb.Top;
            int db = map_data.TileBounds.Bottom-old_tb.Bottom;

            level.SolidTiles.Tiles.Extend(dl, dr, dt, db);
            level.FgTilesLightMask.Extend(dl, dr, dt, db);
            level.BgTiles.Tiles.Extend(dl, dr, dt, db);
            level.SolidTiles.Grid.Extend(dl, dr, dt, db);

            //update autotiler bounds to fix dash blocks crashing?
            for(int i = 0; i < GFX.FGAutotiler.LevelBounds.Count; ++i)
            {
                Rectangle other_bounds = GFX.FGAutotiler.LevelBounds[i];
                other_bounds.X+=dl;
                other_bounds.Y+=dt;
                GFX.FGAutotiler.LevelBounds[i] = other_bounds;
            }
            GFX.FGAutotiler.LevelBounds.Add(new Rectangle(level_data.TileBounds.X-map_data.TileBounds.X, level_data.TileBounds.Y-map_data.TileBounds.Y, level_data.TileBounds.Width, level_data.TileBounds.Height));

            //extend level.solidsdata and level.bgdata
            VirtualMap<char> new_map = new(map_data.TileBounds.Width, map_data.TileBounds.Height,'0');
            VirtualMap<char> new_bg = new(map_data.TileBounds.Width, map_data.TileBounds.Height,'0');
            for(int x = 0; x<old_tb.Width; ++x)
            {
                for(int y = 0; y <old_tb.Height; ++y)
                {
                    new_map[x+dl, y+dt]=level.SolidsData[x,y];
                    new_bg[x+dl, y+dt]=level.BgData[x,y];
                }
            }
            level.SolidsData = new_map;
            level.BgData = new_bg;

            start_x -= map_data.Bounds.Left;
            start_y -= map_data.Bounds.Top;
            target_x -= map_data.Bounds.Left;
            target_y -= map_data.Bounds.Top;
            
            start_x /= 8;
            start_y /= 8;
            target_x /= 8;
            target_y /= 8;

            for(int x = 0; x < level_data.TileBounds.Width; ++x)
            {
                for(int y=0; y < level_data.TileBounds.Height; ++y)
                {
//TODO: animated tiles
                    int tx = target_x+x;
                    int ty = target_y+y;
                    int sx = start_x+x;
                    int sy = start_y+y;
                    level.SolidTiles.Tiles.Tiles[tx,ty] = level.SolidTiles.Tiles.Tiles[sx,sy];
                    level.FgTilesLightMask.Tiles[tx,ty] = level.FgTilesLightMask.Tiles[sx,sy];
                    level.SolidTiles.Grid.Data[tx,ty] = level.SolidTiles.Grid.Data[sx,sy];
                    level.SolidsData[tx,ty] = level.SolidsData[sx,sy];

                    level.BgTiles.Tiles.Tiles[tx,ty] = level.BgTiles.Tiles.Tiles[sx,sy];
                    level.BgData[tx,ty] = level.BgData[sx,sy];


                }
            }


        }


 
    }
}
