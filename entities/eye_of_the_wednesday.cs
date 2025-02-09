
using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Monocle;

using Celeste;
using Celeste.Mod.Entities;

namespace Celeste.Mod.ErrandOfWednesday
{

    [Tracked]
    [CustomEntity("eow/EyeOfTheWednesday")]
    public class EyeOfTheWednesday : Entity
    {

        public EyeOfTheWednesday(EntityData data, Vector2 offset) : base(data.Position + offset)
        {


        }

        /* Actual implementation */

        public static bool loaded = false;

        public static void try_load(Level level)
        {
            LevelData level_data = level.Session.MapData.Get("!eow");
            if(level_data == null)
            {
                level_data = level.Session.MapData.Get("~eow");
            }
            if(level_data == null)
            {
Logger.Log(LogLevel.Debug, "eow", "Didn't find they eye.");
                return;
            }
Logger.Log(LogLevel.Debug, "eow", "Deep space anomaly matching all known criteria for the Eye of the Wednesday found by probe 9,318,054."); 

            //Find the controller
            EntityData data = null;
            foreach(EntityData entity_data in level_data.Entities)
            {
                if(entity_data.Name == "eow/EyeOfTheWednesday")
                {
                    data = entity_data;
                }
            }
            if (data == null)
            {
                return;
            }

            if(data.Bool("verge_block_enable", false))
            {
               VergeBlock.try_load(level.Session);
            }
            if(data.Bool("music_layer_source_enable", false))
            {
                MusicLayerSource.try_load(level);
            }
            if(data.Bool("global_decal_enable", false))
            {
                GlobalDecal.try_load(level);
            }
            if(data.Bool("cannot_transition_to_enable", false))
            {
                CannotTransitionTo.try_load();
            }
            if(data.Bool("refill_bubbler_enable", false))
            {
                RefillBubbler.try_load();
            }





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
