
using System;
using System.Collections.Generic;
using System.Reflection;

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
    [CustomEntity("eow/EyeOfTheWednesday")]
    public class EyeOfTheWednesday : Entity
    {

        public EyeOfTheWednesday(EntityData data, Vector2 offset) : base(data.Position + offset)
        {


        }

        /* Actual implementation */

        public static bool loaded = false;
        public static ILHook bird_hook;


        public static void unload()
        {
            if(!loaded)
            {
                return;
            }

            if(bird_hook != null)
            {
                bird_hook.Dispose();
                bird_hook = null;
            }


            loaded = false;
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
Logger.Log(LogLevel.Debug, "eow", "Eye of the Wednesday activated."); 

            //Find the controller
            EntityData data = null;
            foreach(EntityData entity_data in level_data.Entities)
            {
                if(entity_data.Name == "eow/EyeOfTheWednesday")
                {
                    data = entity_data;
                }
                else if(entity_data.Name == "eow/FlagInitializer")
                {
                    if(session.JustStarted)
                    {
                        for(int i = 1; i < 7; ++i)
                        {
                            string flag_name = entity_data.Attr($"flag{i}", "");
                            if(!string.IsNullOrWhiteSpace(flag_name))
                            {
                                session.SetFlag(flag_name, true);
                            }
                        }
                    }
                }
            }

            if (data == null)
            {
                return;
            }

            if(data.Bool("verge_block_enable", false))
            {
               VergeBlock.try_load(session);
            }
            if(data.Bool("music_layer_source_enable", false))
            {
                MusicLayerSource.try_load(session);
            }
            if(data.Bool("global_decal_enable", false))
            {
                GlobalDecal.try_load();
            }
            if(data.Bool("cannot_transition_to_enable", false))
            {
                CannotTransitionTo.try_load();
            }
            if(data.Bool("refill_bubbler_enable", false))
            {
                RefillBubbler.try_load();
            }
            if(data.Bool("popping_mirror_enable", false))
            {
                PoppingMirror.try_load();
            }
            if(data.Bool("bird_down", false))
            {
                enable_bird();
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


        public static void enable_bird()
        {
            if(bird_hook == null)
            {
            bird_hook = new ILHook(
                typeof(CS00_Ending).GetMethod("Cutscene", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(),
                bird_down
                );

            }
        }

        public static float get_gravity_multiplier()
        {
            int gravity = GravityHelperImports.GetPlayerGravity?.Invoke() ?? 0;
            if(gravity != 0)
            {
                return -1;
            }
            return 1;
        }

        public static void bird_down(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            //The value of the Y component of the tutorial arrow vector
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(-1)) )
            {
                cursor.EmitDelegate<Func<float>>(get_gravity_multiplier);
                cursor.Emit(OpCodes.Mul);
            }
            else
            {
                Logger.Log(LogLevel.Warn, "eow", $"Couldn't find opcode to fix bird.");
                return;
            }

            //The value of the Y component of the aim vector, use to test for a dash in the tutorial direction
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<Vector2>(nameof(Vector2.Y))) )
            {
                cursor.EmitDelegate<Func<float>>(get_gravity_multiplier);
                cursor.Emit(OpCodes.Mul);
 
            }
        }

 
    }
}
