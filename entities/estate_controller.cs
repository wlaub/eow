
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

            regenerate_tiles(level);

        }

        public static void regenerate_tilebounds(MapData map_data)
        {
            int left = map_data.Bounds.Left;
            int right = map_data.Bounds.Right;
            int top = map_data.Bounds.Top;
            int bot = map_data.Bounds.Bottom;

            foreach (LevelData level in map_data.Levels)
            {
                left = Math.Min(left, level.Bounds.Left);
                right = Math.Max(right, level.Bounds.Right);
                top = Math.Min(top, level.Bounds.Top);
                bot = Math.Max(bot, level.Bounds.Top);
            }


            int m = 64;
            map_data.Bounds = new Rectangle(left-m, top-m, right-left+2*m, bot-top + 2*m);

        }

        public static void regenerate_tiles(Level Level)
        {

        MapData mapData = Level.Session.MapData;


        regenerate_tilebounds(mapData);

        Level.BgTiles.RemoveSelf();
        Level.SolidTiles.RemoveSelf();

/* well that didn't work
        LevelLoader loader = new(Level.Session);
        loader.Level = Level;
        loader.LoadingThread();
*/
        //And then this is where you paste the whole tile generation section of LevelLoader.LoadThread
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
