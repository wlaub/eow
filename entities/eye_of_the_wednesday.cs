
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

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

        public static bool lore_enabled = false;

        public static bool loop_invariance_enabled = false;
        public static string[] invariance_targets;

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

            unload_lore();
            unload_loop_invariance();

            loaded = false;
        }

        public static void on_load_level(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            ErrandOfWednesdayModuleSession mod_session = ErrandOfWednesdayModule.Session;
            if(loop_invariance_enabled)
            {
                if(mod_session.invariance_state is not null)
                {
                    if(isFromLoader)
                    {
                        mod_session.invariance_state.restore_state(level.Session, level);
                    }
                }
                else
                {
                    mod_session.invariance_state = new();
                }
            }

            
        }

        public static void try_load(Session session, LevelLoader level_loader)
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
                enable_stage_mirror(session);
            }
            if(data.Bool("lore_enable", true)) //TODO: update lonn
            {
                enable_lore();
            }
            if(data.Bool("loop_invariance", true)) //TODO update lonn
            {
                invariance_targets = data.Attr("invariance_targets","Celeste.Mod.ErrandOfWednesday.LoreOre,Celeste.Key,Celeste.StrawberrySeed").Split(',');
                enable_loop_invariance();
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


        public static void enable_stage_mirror(Session session)
        {
            if(stage_mirror_hook == null)
            {
                stage_mirror_hook = new ILHook(
                    typeof(Level).GetMethod("Render", BindingFlags.Public | BindingFlags.Instance),
                    stage_mirror_render_bullshit
                    );

                if(session.GetFlag(StageMirror.MIRROR_FLAG))
                {
                     SaveData.Instance.Assists.MirrorMode = true;
                }
               
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
                cursor.EmitDelegate<Action<Level>>(
                    stage_mirror_draw
                    );
 
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

            //TODO you have to sort by distant to player if you want more than 1 to work right
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
//                    if(player.Center.X < world_x) // wrong because it can fall out of sync with mirror.was_left and flip sides before the mirror event happens
                    if(mirror.was_left)
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

        public static void enable_lore()
        {
            if(lore_enabled) return;
            On.Celeste.TheoCrystal.Die+=lore_die_hook;
            lore_enabled = true;
        }

        public static void unload_lore()
        {
            if(!lore_enabled) return;
            On.Celeste.TheoCrystal.Die-=lore_die_hook;
            lore_enabled = false;
        }

        public static void lore_die_hook(On.Celeste.TheoCrystal.orig_Die orig, TheoCrystal self)
        {
            if(!(self is LoreOre))
            {
                orig(self);
            }
            else if (!self.dead)
            {
                (self as LoreOre).die();
/*                self.dead = true;
                //TODO don't do this on transition
                Audio.Play("event:/game/05_mirror_temple/crystaltheo_break_free", self.Position);
                self.Add(new DeathEffect(Color.Orange, self.Center - self.Position));
                self.sprite.Visible = false;
                self.Depth = -1000000;
                self.AllowPushing = false;

                //TODO after animation
                self.RemoveSelf();*/
            }            
        }

        public static void enable_loop_invariance()
        {
            //on transition, make held object global
            //or on pickup make held object global/if it has an eid
            //on death, detach followers in-place, make global
            //save object positions and metadata
            //reinstantiate saved objects on load
            //don't add new copies of objects that are instantiated already

            if(loop_invariance_enabled) return;
            Logger.Log(LogLevel.Info, "eow", $"loading loop invariance");
            Everest.Events.Level.OnTransitionTo += li_transition_hook;
            On.Celeste.Player.Die += li_on_die;
            On.Celeste.Actor.Update += li_actor_update;
            On.Celeste.Leader.GainFollower += li_gain_follower;
            On.Celeste.Leader.LoseFollower += li_lose_follower;
            On.Monocle.Entity.Removed += li_remove;
            On.Celeste.Key.RegisterUsed += li_register_used;
            On.Celeste.Player.Pickup += li_pickup;

            invariant_entities.Clear();
            invariance_states.Clear();

            loop_invariance_enabled = true;
        }
        public static void unload_loop_invariance()
        {
            if(!loop_invariance_enabled) return;
            Logger.Log(LogLevel.Info, "eow", $"unloading loop invariance");
            Everest.Events.Level.OnTransitionTo -= li_transition_hook;
            On.Celeste.Player.Die -= li_on_die;
            On.Celeste.Actor.Update -= li_actor_update;
            On.Celeste.Leader.GainFollower -= li_gain_follower;
            On.Celeste.Leader.LoseFollower -= li_lose_follower;
            On.Celeste.Player.Pickup -= li_pickup;
            On.Monocle.Entity.Removed -= li_remove;
            On.Celeste.Key.RegisterUsed -= li_register_used;
            invariant_entities.Clear();
            invariance_states.Clear();
            loop_invariance_enabled = false;
        }
        public static Dictionary<Entity, string> invariant_entities = new();

        //TODO update all entity states on save and quit?
        //TODO update lore SourceData on damage, rotation, etc
        //TODO holdable active state based on room bounds?
        //TODO i'm gonna need to figure out how to hold the game open after player death until things settle anyway so i might as well do that sooner than later

        public static bool li_pickup(On.Celeste.Player.orig_Pickup orig, Player self, Holdable hold)
        {
            bool result = orig(self, hold);
            if(result)
            {
                Level level = self.Scene as Level;
                Entity entity = hold.Entity;
        Logger.Log(LogLevel.Info, "eow", $"holdable on pickup ->{entity.SourceId}, {entity.GetType().FullName} {string.Join(",", invariance_targets)}");
                if(li_allowed(entity))
                {
                    make_invariant(level, entity, level.Session.LevelData.Name, false, false);
                }
            }
            return result;
        }

        public static void li_register_used(On.Celeste.Key.orig_RegisterUsed orig, Key self)
        { //the lock block dnl's itself before the key is removed
            orig(self); // this calls Leader.LoseFollower, which makes the key tracked

            li_untrack(self);
        }

        public static void li_remove(On.Monocle.Entity.orig_Removed orig, Entity self, Scene scene)
        {
            li_untrack(self);

            orig(self, scene);

        }

        public static void li_untrack(Entity self)
        {
            if(invariance_states.ContainsKey(self))
            {
                invariant_entities.Remove(self);
Logger.Log(LogLevel.Info, "eow", $"remove ->{self.SourceId}");
 
                ErrandOfWednesdayModuleSession mod_session = ErrandOfWednesdayModule.Session;
                if(mod_session.invariance_state is not null)
                {
                    mod_session.invariance_state.remove_entity(self);
Logger.Log(LogLevel.Info, "eow", $"  and from the save ->{self.SourceId}");
                }
            }
 
        }

        public static void li_gain_follower(On.Celeste.Leader.orig_GainFollower orig, Leader self, Follower follower)
        {
            orig(self, follower);
            Entity entity = follower.Entity;
            if(li_allowed(entity) && entity.Scene is not null)
            {
                Level level = entity.Scene as Level;
                make_invariant(level, entity, level.Session.LevelData.Name, true, true);
            }

        }

        public static void li_lose_follower(On.Celeste.Leader.orig_LoseFollower orig, Leader self, Follower follower)
        {
            orig(self, follower);
            Entity entity = follower.Entity;
            if(li_allowed(entity) && entity.Scene is not null)
            {
                Level level = entity.Scene as Level;
                make_invariant(level, entity, level.Session.LevelData.Name, true, false);
            }


        }

        public static void li_actor_update(On.Celeste.Actor.orig_Update orig, Actor entity)
        {
            orig(entity);
            if(invariance_states.ContainsKey(entity))
            {
                InvariantEntityState entry;
//                Level level = entity.Scene as Level;
//                string room_name = level.Session.LevelData.Name;
                entry = invariance_states[entity];
                entry.update_position(entity);
            }
 
        }

        public static bool li_allowed(Entity entity)
        {
            return (invariance_targets.Contains(entity.GetType().FullName) || invariance_targets.Length == 0);
        }

        public static PlayerDeadBody li_on_die(On.Celeste.Player.orig_Die orig, Player player, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
        {
//TODO ideally if a key is in the middle of being used, we would wait for it to finish before respawning
//otherwise it gets trapped inside the lock block, and a loop-invariant key would not stop being used on death
            Level level = player.Scene as Level;
            List<Follower> to_lose = new();
            foreach(Follower follower in player.Leader.Followers)
            {
                Entity entity = follower.Entity;
Logger.Log(LogLevel.Info, "eow", $"hello ->{entity.SourceId}, {entity.GetType().FullName} {string.Join(",", invariance_targets)}");
                if(li_allowed(entity))
                {
//                    make_invariant(level, entity, level.Session.LevelData.Name, true, false);
                    entity.Collidable = true;
                    to_lose.Add(follower);
                    level.Session.Keys.Remove(entity.SourceId);
                }
                
            }

            foreach(Follower follower in to_lose)
            {
                player.Leader.LoseFollower(follower); //this triggers the entity to become invariant
            }

            return orig(player, direction, evenIfInvincible, registerDeathInStats);
        }

        public static Dictionary<Entity, InvariantEntityState> invariance_states = new();

        public static void make_invariant(Level level, Entity entity, string room_name, bool is_follower, bool is_held)
        {
Logger.Log(LogLevel.Info, "eow", $"make_invariant: ->{entity.SourceId}, {entity.GetType().FullName} {string.Join(",", invariance_targets)}");
            entity.Tag |= Tags.Global;
            invariant_entities[entity] = room_name;
            if(entity.SourceId.ID != default(EntityID).ID)
            {
                level.Session.DoNotLoad.Add(entity.SourceId);   
            }

            ErrandOfWednesdayModuleSession mod_session = ErrandOfWednesdayModule.Session;
            if(mod_session.invariance_state is not null)
            {
                mod_session.invariance_state.save_entity(entity, room_name, is_follower, is_held);
            }
            else
            {
                Logger.Log(LogLevel.Error, "eow", $"loop invariance session data missing");
            }
        }

        public static void _save_invariant_entity(InvarianceState self, Entity entity, string room_name, bool is_follower, bool is_held)
        {//this is stupid
            if(entity.SourceData is null)
            {
Logger.Log(LogLevel.Error, "eow", $"can't save entity with null source data");
                return;
            }
            InvariantEntityState entry;

            if(invariance_states.ContainsKey(entity))
            {
                entry = invariance_states[entity];
                entry.update_from(entity, room_name, is_follower, is_held);
Logger.Log(LogLevel.Info, "eow", $"updating saved entity: ->{entity.SourceId}, {entity.GetType().FullName}");
            }
            else
            {
Logger.Log(LogLevel.Info, "eow", $"saving new entity: ->{entity.SourceId}, {entity.GetType().FullName}");
                entry = new();
                entry.update_from(entity, room_name, is_follower, is_held);
                self.entities.Add(entry);
                invariance_states[entity] = entry;
            }
Logger.Log(LogLevel.Info, "eow", $"there are {self.entities.Count} entities saved");
        }

        public static void li_transition_hook(Level level, LevelData next, Vector2 direction)
        {
            Player player = level.Tracker.GetEntity<Player>();
            if(player != null)
            {
                foreach(Entity e in invariant_entities.Keys)
                {
                    string room_name = invariant_entities[e];
                    if(player.Holding == null || player.Holding.Entity != e)
                    {
                        e.Active = next.Name == room_name;
                    }
                }
            foreach(Follower follower in player.Leader.Followers)
            {
                Entity entity = follower.Entity;
                if(invariant_entities.ContainsKey(entity))
                {
                    entity.Active = true;
                    invariant_entities[entity] = next.Name;
                }
            }
 
                if(player.Holding != null)
                {
                    Entity entity = player.Holding.Entity;
            Logger.Log(LogLevel.Info, "eow", $"hello ->{entity.SourceId}, {entity.GetType().FullName} {string.Join(",", invariance_targets)}");
                    if(li_allowed(entity))
                    {
                        make_invariant(level, entity, next.Name, false, false);
                    }
                }
            }
        }


 
    }
}
