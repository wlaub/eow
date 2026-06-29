
using System;
using System.Collections.Generic;
using System.Reflection;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
        public static ILHook stage_mirror_hook;



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

        public static bool hitbox_flag_loaded = false;
        public static string show_hitbox_flag;
        public static bool show_hitbox_flag_inverted;

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
            if(stage_mirror_hook != null)
            {
                stage_mirror_hook.Dispose();
                stage_mirror_hook = null;
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
            unload_hitbox_flag();

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
            if(data.Bool("stage_mirror_enable", false))
            {
                enable_stage_mirror();
            }
 

            string hitbox_flag = data.Attr("show_hitbox_flag", "");
            if(!string.IsNullOrWhiteSpace(hitbox_flag))
            {
                show_hitbox_flag_inverted = Flagic.process_flag(hitbox_flag, out show_hitbox_flag);
                try_load_hitbox_flag(session);
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


        public static void enable_stage_mirror()
        {
            if(stage_mirror_hook == null)
            {
                stage_mirror_hook = new ILHook(
                    typeof(Level).GetMethod("Render", BindingFlags.Public | BindingFlags.Instance),
                    stage_mirror_render_bullshit
                    );
            }
        }


        public static void stage_mirror_render_bullshit(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            //Intercept the draw call
            if (cursor.TryGotoNext(MoveType.Before, 
                instr => instr.MatchLdnull(),
                instr => instr.MatchCallvirt<GraphicsDevice>("SetRenderTarget")
                ))
            {

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Action<Level>>((level) => {
                    stage_mirror_draw(level);
                    });
 
                Logger.Log(LogLevel.Warn, "eow", $"stage mirror enabled");
            }
             else
            {
                Logger.Log(LogLevel.Warn, "eow", $"Couldn't find opcode to stage mirror.");
                return;
            }
 
        }

        public static void stage_mirror_draw(Level level)
        {
            /* this is the only way the author of dreamjellyfish could imagine doing this, which is better than i could manage */
            SpriteBatch sb = Draw.SpriteBatch;

            foreach (StageMirror mirror in level.Tracker.GetEntities<StageMirror>())
            {

            float world_x = 272;
            float world_top = 0;
            float world_bot = 32;

            world_x = 184;
            world_bot = 64;
            world_top = 32;

            world_x = mirror.Center.X;
            world_top = mirror.Top;
            world_bot = mirror.Bottom;

//            world_top = level.Bounds.Top;
//            world_bot = level.Bounds.Bottom;

            Rectangle bb = GameplayBuffers.Level.Bounds;
            int xoff = bb.Width-32;
            int ytop = 64;
            int ybot;
            xoff = (int)(world_x - level.Camera.Position.X);
            ytop = (int)(world_top - level.Camera.Position.Y);
            ybot = (int)(world_bot - level.Camera.Position.Y);

            int stop = (int)(level.Bounds.Top - level.Camera.Position.Y);
            int sbot = (int)(level.Bounds.Bottom - level.Camera.Position.Y);


            bool no_draw = false;
            float ytop2 = level.Bounds.Top;
            float ybot2 = level.Bounds.Bottom;
            int xedge = level.Bounds.Left;
            Player player = level.Tracker.GetEntity<Player>();
            if(player != null)
            {
                float yeye = player.Top;
                if(player.Left > world_x)
                {
                    float xeye = player.Left;
                    float dx = xeye-world_x;
                    float ddx = xeye-level.Bounds.Left;
                    ytop2 = (int)(world_top+(world_top-yeye)*ddx/dx);
                    ybot2 = (int)(world_bot+(world_bot-yeye)*ddx/dx);
                    xedge = level.Bounds.Left;
                }
                else if(player.Right < world_x)
                {
                    float xeye = player.Right;
                    float dx = world_x-xeye;
                    float ddx = level.Bounds.Right - xeye;
                    ytop2 = (int)(world_top+(world_top-yeye)*ddx/dx);
                    ybot2 = (int)(world_bot+(world_bot-yeye)*ddx/dx);
                    xedge = level.Bounds.Right;
                }
                else if (yeye >= world_top && yeye <= world_bot)
                {
                    if(player.Center.X < world_x)
                    {
                         xedge = level.Bounds.Right;
                         ytop2 = world_top = level.Bounds.Top;
                         ybot2 = world_bot = level.Bounds.Bottom;
                    }
                    else
                    {
                         xedge = level.Bounds.Left;
                         ytop2 = world_top = level.Bounds.Top;
                         ybot2 = world_bot = level.Bounds.Bottom;
                    }
                }
                else
                {
                    no_draw = true;
                } 
            }

            RenderTarget2D mask = null;
            if(!no_draw)
            {
                mask = new(sb.GraphicsDevice, bb.Width, bb.Height);
                sb.GraphicsDevice.SetRenderTarget(mask);
                sb.GraphicsDevice.Clear(Color.Transparent);



                VertexPositionColor[] verts = new VertexPositionColor[6];

                verts[0].Position = new Vector3(world_x, world_top, 0f);
                verts[1].Position = new Vector3(world_x, world_bot, 0f);
                verts[2].Position = new Vector3(xedge, ytop2, 0f);

                verts[3].Position = new Vector3(xedge, ybot2, 0f);
                verts[4].Position = new Vector3(xedge, ytop2, 0f);
                verts[5].Position = new Vector3(world_x, world_bot, 0f);
     
                verts[0].Color = Color.White;
                verts[1].Color = Color.White;
                verts[2].Color = Color.White;
                verts[3].Color = Color.White;
                verts[4].Color = Color.White;
                verts[5].Color = Color.White;


                GFX.DrawVertices(level.Camera.Matrix, verts, 6, null, null);

                BlendState mask_blend = new();
                mask_blend.ColorSourceBlend = Blend.DestinationAlpha;
                mask_blend.ColorDestinationBlend = Blend.Zero;

                mask_blend.AlphaSourceBlend = Blend.Zero;
                mask_blend.AlphaDestinationBlend = Blend.One;
                mask_blend.AlphaBlendFunction = BlendFunction.Add;

                sb.Begin(SpriteSortMode.Deferred, mask_blend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null);

                Rectangle mirror_bounds = new Rectangle(-(bb.Width-2*xoff),stop,bb.Width, sbot-stop);
                sb.Draw(GameplayBuffers.Level, new Vector2(0,stop), mirror_bounds, Color.White, 0, Vector2.Zero, 1f, SpriteEffects.FlipHorizontally, 0);
                sb.End();


            }

            sb.GraphicsDevice.SetRenderTarget(GameplayBuffers.Level);
 
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null);
            if(!no_draw)
            {
                sb.Draw(mask, Vector2.Zero, bb, Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);
            }
            Draw.Line(new Vector2(xoff, ytop), new Vector2(xoff, ybot), Color.Black, 2f);
            sb.End(); 

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
                        return forehead_distance; 
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

        //
        // show hitboxes on flag
        //
        public static void hitbox_set_flag_callback(On.Celeste.Session.orig_SetFlag orig, Session self, string flag, bool val)
        {
            orig(self, flag, val);

            if(flag == show_hitbox_flag)
            {
                GameplayRenderer.RenderDebug=(val != show_hitbox_flag_inverted);
            }

        }

        public static void try_load_hitbox_flag(Session session)
        {
            if(hitbox_flag_loaded){return;}
            On.Celeste.Session.SetFlag += hitbox_set_flag_callback;
            GameplayRenderer.RenderDebug=Flagic.test_flag(session, show_hitbox_flag, show_hitbox_flag_inverted);
 
            hitbox_flag_loaded = true;
        }

        public static void hitbox_flag_on_load(Level level)
        {
            if(!hitbox_flag_loaded){return;}
            GameplayRenderer.RenderDebug=Flagic.test_flag(level.Session, show_hitbox_flag, show_hitbox_flag_inverted);
        }



        public static void unload_hitbox_flag()
        {
            if(!hitbox_flag_loaded){return;}

            On.Celeste.Session.SetFlag -= hitbox_set_flag_callback;
            GameplayRenderer.RenderDebug = false;
            hitbox_flag_loaded = false;
        }





 
    }
}
