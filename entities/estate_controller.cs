
using System;
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

    [Tracked]
    [CustomEntity("eow/EstateController")]
    public class EstateController : Entity
    {

        public static bool loaded = false;
        public static int room_width;
        public static int room_height;
        public static int grid_width;
        public static int grid_height;

        public static Dictionary<string, LevelData> rooms;
        public static HashSet<string> drafted_rooms;

        

        public EstateController(EntityData data, Vector2 offset) : base(data.Position + offset)
        {


        }

        /* Actual implementation */

        public static void unload()
        {
            if(!loaded)
            {
                return;
            }

            loaded = false;
        }
       

        public static void move_room(Level level) 
        {
            Session session = level.Session;
            int target_x = 46*8;
            int target_y = -18*8;
 

            LevelData level_data = session.MapData.Get("www");

            int start_x = level_data.Bounds.X;
            int start_y = level_data.Bounds.Y;
            int off_x = target_x-start_x;
            int off_y = target_y-start_y;

Logger.Log(LogLevel.Info, "eow", $"{level_data.Bounds.X} {level_data.Bounds.Y}");
            level_data.Bounds.X = target_x;
            level_data.Bounds.Y = target_y;

            for(int i = 0; i < level_data.Spawns.Count; ++i)
            {
                Vector2 spawn = level_data.Spawns[i];
Logger.Log(LogLevel.Info, "eow", $"  {spawn.X} {spawn.Y}");
                spawn.X += off_x;
                spawn.Y += off_y;
                level_data.Spawns[i] = spawn;

Logger.Log(LogLevel.Info, "eow", $"->{spawn.X} {spawn.Y}");
            }
            for(int i = 0; i < level_data.Spawns.Count; ++i)
            {
                Vector2 spawn = level_data.Spawns[i];
Logger.Log(LogLevel.Info, "eow", $"=={spawn.X} {spawn.Y}");
            }

            regenerate_tilebounds(level, start_x, start_y, target_x, target_y);

        }

        public static void regenerate_tilebounds(Level level, int start_x, int start_y, int target_x, int target_y)
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
            int m = 64;
            map_data.Bounds = new Rectangle(left-m, top-m, right-left+2*m, bot-top + 2*m);

            int dl = -map_data.Bounds.Left+old_bounds.Left;
            int dr = map_data.Bounds.Right-old_bounds.Right;
            int dt = -map_data.Bounds.Top+old_bounds.Top;
            int db = map_data.Bounds.Bottom-old_bounds.Bottom;

            level.SolidTiles.Tiles.Extend(dl/8, dr/8, dt/8, db/8);
            level.BgTiles.Tiles.Extend(dl/8, dr/8, dt/8, db/8);
            level.SolidTiles.Grid.Extend(dl/8, dr/8, dt/8, db/8);


            start_x -= map_data.Bounds.Left;
            start_y -= map_data.Bounds.Top;
            target_x -= map_data.Bounds.Left;
            target_y -= map_data.Bounds.Top;
            
            start_x /= 8;
            start_y /= 8;
            target_x /= 8;
            target_y /= 8;

            for(int x = 0; x < 46; ++x)
            {
                for(int y=0; y < 36; ++y)
                {
                   level.SolidTiles.Tiles.Tiles[target_x+x,target_y+y] = level.SolidTiles.Tiles.Tiles[start_x+x, start_y+y];
                   level.SolidTiles.Grid.Data[target_x+x,target_y+y] = level.SolidTiles.Grid.Data[start_x+x, start_y+y];
                   level.BgTiles.Tiles.Tiles[target_x+x,target_y+y] = level.SolidTiles.Tiles.Tiles[start_x+x, start_y+y];
                }
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

            room_width = data.Int("room_width");
            room_height = data.Int("room_height");
            grid_width = data.Int("grid_width");
            grid_height = data.Int("grid_height");

            foreach(LevelData room_data in session.MapData.Levels)
            {
                foreach(EntityData entity_data in room_data.Entities)
                {
                    if(entity_data.Name == "eow/EstateRoom")
                    {
                        //TODO load up that estate room
                    }
                }
               
            }

            Logger.Log(LogLevel.Debug, "eow", $"Finished loading everything");

            loaded = true;

/*
            //Scan for things
            foreach(LevelData level_data in level.Session.MapData.Levels)
            {
                foreach(EntityData entity_data in level_data.Entities)
                {
                    if(entity_data.Name == name)
                    {
                        return true;
                    }
                }
            }
*/


        }

 
    }
}
