
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


        public static bool guitar_hands_enabled = false;
        public static float guitar_hands_duration = 0.08f;
        public static string guitar_hands_flag = "guitar_hands";
        public static bool guitar_hands_flag_inverted = false;
        //TODO: clear this on room load or something
        public static Dictionary<int, float> guitar_hands_timers = new();

        public static ILHook toe_shoes_hook;
        public static bool toe_shoes_enabled = false;
        public static string toe_shoes_flag;
        public static bool toe_shoes_flag_inverted;

        public static ILHook forehead_hook;
        public static bool forehead_enabled = false;
        public static string forehead_flag;
        public static bool forehead_flag_inverted;
        public static int forehead_distance;

        public static bool is_riding_hook(On.Celeste.Player.orig_IsRiding_Solid orig, Player self, Solid solid)
        {
            if(orig(self, solid)) 
            {

        		if (self.StateMachine.State == 1 || self.StateMachine.State == 6)
        		{
                    if(!Flagic.test_flag(self.SceneAs<Level>().Session, guitar_hands_flag, guitar_hands_flag_inverted))
                    {
                        return true;
                    }
                    int key = solid.GetHashCode();
                    float timer;
                    if(guitar_hands_timers.ContainsKey(key))
                    {
                        timer = guitar_hands_timers[key];
                    }
                    else
                    {
                        timer = guitar_hands_duration;
                    }
                    if(timer > 0)
                    {
                        timer -= Engine.DeltaTime;
                        guitar_hands_timers[key] = timer;
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

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
                On.Celeste.CS00_Ending.ctor -= bird_once;
            }
            if(guitar_hands_enabled)
            {
                On.Celeste.Player.IsRiding_Solid -= is_riding_hook;
                guitar_hands_enabled = false;
            }
            if(toe_shoes_hook != null)
            {
                toe_shoes_hook.Dispose();
                toe_shoes_hook = null;
                toe_shoes_enabled = false;
            }
            if(forehead_enabled)
            {
//                forehead_hook.Dispose();
//                forehead_hook = null;
                IL.Celeste.Player.WallJumpCheck -= forehead;
 
                forehead_enabled = false;
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
                MusicLayerSource.light_control_flag_inverted = Flagic.process_flag(
                    data.Attr("music_source_light_control_flag", ""),
                    out MusicLayerSource.light_control_flag);
 
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
            if(data.Bool("bistable_decal_enable", false))
            {
                BistableDecal.try_load();
            }
            if(data.Bool("guitar_hands_enable", false))
            {
                if(!guitar_hands_enabled)
                {
                    On.Celeste.Player.IsRiding_Solid += is_riding_hook;
                    guitar_hands_enabled = true;
                }
                guitar_hands_duration = data.Float("guitar_hands_duration", 0.08f);
                guitar_hands_flag_inverted = Flagic.process_flag(data.Attr("guitar_hands_flag", ""), out guitar_hands_flag);
 
            }
            if(data.Bool("toe_shoes_enable", false))
            {
                if(!toe_shoes_enabled)
                {
                    toe_shoes_flag_inverted = Flagic.process_flag(data.Attr("toe_shoes_flag", ""), out toe_shoes_flag);
                    toe_shoes_hook = new ILHook(typeof(Player).GetMethod("orig_Update"), toe_shoes);
                    toe_shoes_enabled = true;
                }
            }
            if(data.Bool("forehead_enable", false))
            {
                if(!forehead_enabled)
                {
                    forehead_flag_inverted = Flagic.process_flag(data.Attr("forehead_flag", ""), out forehead_flag);
                    forehead_distance = data.Int("forehead_distance", 13);
                    IL.Celeste.Player.WallJumpCheck += forehead;
                    forehead_enabled = true;
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


        public static void bird_once(On.Celeste.CS00_Ending.orig_ctor orig, CS00_Ending self, Player player, BirdNPC bird, Bridge bridge)
        {
            orig(self, player, bird, bridge);
            if(bird.onlyOnce)
            {
                Level level = Engine.Scene as Level;
                if(level != null)
                {
                    level.Session.DoNotLoad.Add(bird.EntityID);
                }
            }
        }

        public static void enable_bird()
        {
            if(bird_hook == null)
            {
                bird_hook = new ILHook(
                    typeof(CS00_Ending).GetMethod("Cutscene", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(),
                    bird_down
                    );
                On.Celeste.CS00_Ending.ctor += bird_once;
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

        public static void toe_shoes(ILContext il)
        {
             ILCursor cursor = new ILCursor(il);

            //MoveVExact((int)vector.Y); #move the player vertically with the moving solid
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall<Actor>("MoveVExact")) )
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Action<Player>>((self) => {
                    if(Flagic.test_flag(self.SceneAs<Level>().Session, toe_shoes_flag, toe_shoes_flag_inverted))
                    {
     
                        float delta = self.Position.Y-self.climbHopSolid.Position.Y ;
                        if(delta < 0)
                        {
                            self.Position.Y = self.climbHopSolid.Position.Y;
                        }
                    }
                    });
                Logger.Log(LogLevel.Warn, "eow", $"toe shoes enabled");
            }
             else
            {
                Logger.Log(LogLevel.Warn, "eow", $"Couldn't find opcode to toe shoes.");
                return;
            }
        } 

        public static void forehead(ILContext il)
        {
             ILCursor cursor = new ILCursor(il);

            //MoveVExact((int)vector.Y); #move the player vertically with the moving solid
            if (cursor.TryGotoNext(MoveType.After, 
                            instr => instr.MatchLdcI4(5)
//                            instr =>  instr.MatchStloc(0)
                            ))
            {
                //extended variants inserts stuff here. skip past it to override.
                //extended variants sets both the wall check distance and the 
                //spike check distance, and this sets only the wall check distance
                //to facilitate jank
            }
            else {
                Logger.Log(LogLevel.Warn, "eow", $"forehead failed to find first opcode");
                return;
            }
            if (cursor.TryGotoNext(MoveType.After, 
//                            instr => instr.MatchLdcI4(5)
                            instr =>  instr.MatchStloc(0)
                            ))
            {
 
                cursor.Index--;
                cursor.EmitDelegate<Func<int, int>>((orig) => {
                    if(Flagic.test_flag((Engine.Scene as Level).Session, forehead_flag, forehead_flag_inverted))
                    {
                        return 13; 
                    }
                    return orig;
                    });
                Logger.Log(LogLevel.Warn, "eow", $"forehead enabled");
            }
             else
            {
                Logger.Log(LogLevel.Warn, "eow", $"Couldn't find opcode to forehead.");
                return;
            }
        } 
 
    }
}
