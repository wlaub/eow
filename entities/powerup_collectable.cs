using System;
using System.Collections;

using Microsoft.Xna.Framework;

using Monocle;

using MonoMod.Utils;

using Celeste;
using Celeste.Mod.Entities;

namespace Celeste.Mod.ErrandOfWednesday
{

    [Tracked]
    [CustomEntity("eow/PowerupCollectable")]
    public class PowerupCollectable : Entity
    {

        public Sprite sprite;

        public Vector2[] targets; 

        public bool do_pulse;
        public bool must_dash_toward;
        public bool center_player;
        public bool show_animation;
        public bool show_poem;
        public bool do_wiggle;
        public bool do_refill = true;
        public bool do_unfill = false;

        public int heart_index = 3;
   
        public string poem_dialog;
        public string flag;
        public string sound_name;

        public Color shatter_color;

        public float strength;

        public bool collected = false;
        public bool activated = false;

        public Wiggler move_wiggler;
        public Vector2 move_wiggler_dir;
        public float bounce_sound_delay = 0f;

        public SoundEmitter sfx;

        public Poem poem;
        public BloomPoint bloom;

        public EntityID eid;

        public PowerupCollectable(EntityData data, Vector2 offset, EntityID eid) : base(data.Position + offset)
        {
            this.eid = eid;

            do_pulse = data.Bool("do_pulse");
            must_dash_toward = data.Bool("must_dash_toward");
            center_player = data.Bool("center_player");
            show_animation = data.Bool("show_animation");
            show_poem = data.Bool("show_poem");

            do_wiggle = data.Bool("do_wiggle");
            do_refill = data.Bool("do_refill");
            do_unfill = data.Bool("do_unfill");

            heart_index = data.Int("heart_index");

            strength = data.Float("strength");

            flag = data.Attr("flag");
            poem_dialog = data.Attr("poem_dialog");
            sound_name = data.Attr("collect_sound");
 
            shatter_color = Calc.HexToColor(data.Attr("shatter_color"));

            targets = new Vector2[data.Nodes.Length];
            for(int i = 0; i < targets.Length; ++i)
            {
                targets[i] = data.Nodes[i]+offset;
            }

            Add(sprite = new Sprite(GFX.Game, ""));
            sprite.AddLoop("spin", data.Attr("sprite"), 0.08f);
            sprite.CenterOrigin();
            sprite.Play("spin");
 

            move_wiggler = Wiggler.Create(0.8f, 2f);
            move_wiggler.StartZero = true;
            Add(move_wiggler);

        } 

        public override void Awake(Scene scene)
        {
            base.Awake(scene);


            sprite.OnLoop = delegate(string anim)
            {
                if (Visible && anim == "spin" && do_pulse)
                {
                    Audio.Play("event:/game/general/crystalheart_pulse", Position);
                    
//                    ScaleWiggler.Start();
                    (base.Scene as Level).Displacement.AddBurst(Position, 0.35f, 8f, 48f, 0.25f);
                }
            };

	    	base.Collider = new Hitbox(12f, 12f, -6f, -6f);
    		Add(new PlayerCollider(OnPlayer));            

        }

        public override void Update()
        {
            base.Update();
            sprite.Position = move_wiggler_dir * move_wiggler.Value * -8f;
            bounce_sound_delay -= Engine.DeltaTime;

        }

        public void activate_nodes(Player player)
        {
            if(player is null || player.Scene is null || activated)
            {
                return;
            }
            
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
            activated = true;

        }

        public void OnPlayer(Player player)
        {
            if(collected)
            {
                return;
            }
            if(player.DashAttacking)
            {
                if(!must_dash_toward || _dashing_toward(player))
                {
                    collect(player);
                }
                return;
            }
            player.PointBounce(Center);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            if(bounce_sound_delay <= 0f)
            {
                Audio.Play("event:/game/general/crystalheart_bounce", Position);
                bounce_sound_delay = 0.1f;
            }

            if(do_wiggle)
            {
                move_wiggler.Start();
                move_wiggler_dir = (Center-player.Center).SafeNormalize(Vector2.UnitY);
            }
        }

        public bool _dashing_toward(Player player)
        {
            Vector2 toward = Center - player.Center;
            float angle = toward.X*player.Speed.X + toward.Y*player.Speed.Y;
            return angle > 0;

        }

        public void collect(Player player)
        {
            collected = true;
            Add(new Coroutine(collect_coroutine(player)));

        }

        public void do_center_player(Player player)
        {
            if(center_player)
            {
                Vector2 offset = Center - player.Center;
                if(offset.LengthSquared() >= 1)
                {
                    player.Speed = player.Speed*0.85f + offset * 2.5f;
                }
                else
                {
                    player.Speed = Vector2.Zero;

                }
            }
            else
            {
                player.Speed *= 0.85f;
            }
        }

        public IEnumerator collect_coroutine(Player player)
        {   
            Visible = false;
            Collidable = false;

            Level level = SceneAs<Level>();

            if (sound_name != "")
            {
                sfx = SoundEmitter.Play(sound_name, this);
            }

            if(show_animation || show_poem)
            {
                Audio.PauseMusic = true;
            }

            if(show_animation)
            {
                for(int i = 0; i < 4; ++ i)
                {
                    Scene.Add(new AbsorbOrb(Position, player));
                }
            }
            else
            {
                for(int i = 0; i < 10; ++ i)
                {
                    Scene.Add(new AbsorbOrb(Position, player));
                }
            }



            //Start animation
            if(show_animation)
            {
                level.CanRetry = false;
                player.StateMachine.State = Player.StDummy;
                player.DummyGravity = false;
                player.DummyMoving = true;
                player.DummyAutoAnimate = false;

                player.Sprite.Play(PlayerSprite.StartStarFly);
                player.Hair.Visible = false;

                player.Add(bloom = new BloomPoint(1f, 0f));

                float next = 0f;
                for(float t = 0f; t < 2f; t+= Engine.RawDeltaTime)
                {
                    if (t>= next)
                    {
                        Scene.Add(new AbsorbOrb(Position, player));

                        bloom.Radius = (t-0.2f)*16;

                        next += 0.2f;
                    }

                    do_center_player(player);
     
                    yield return null;
                } 

                for(float t = 0f; t < 1f; t+= Engine.RawDeltaTime)
                {
                    do_center_player(player);

                    yield return null;
                }

            }

            activate_nodes(player);
            if(flag != "")
            {
                level.Session.SetFlag(flag, true);
            }

            if(do_refill)
            {
                player.UseRefill(false);
            }
            if(do_unfill)
            {
                player.Dashes = 0;
            }

            //Do Poem
            if(show_poem)
            {

                level.FormationBackdrop.Display = true;
                level.FormationBackdrop.Alpha = 1f;

                string poem_text = Dialog.Clean(poem_dialog);

                poem = new Poem(poem_text, heart_index, 0.0f);
                poem.Heart.Visible = false;
                poem.Alpha = 0f;
                Scene.Add(poem);
                for (float t3 = 0f; t3 < 1f; t3 += Engine.RawDeltaTime)
                {
                    poem.Alpha = Ease.CubeOut(t3);
                    yield return null;
                }            
                while (!Input.MenuConfirm.Pressed && !Input.MenuCancel.Pressed)
                {
                    yield return null;
                }
               
                for (float t3 = 0f; t3 < 1f; t3 += Engine.RawDeltaTime * 2f)
                {
                    poem.Alpha = Ease.CubeIn(1f - t3);
                    yield return null;
                }
                player.Depth = 0;

        		level.FormationBackdrop.Display = false;

            } 

            //Finish
            if(show_animation)
            {
                for(float t = 0f; t < 0.1f; t+= Engine.RawDeltaTime)
                {
                    yield return null;
                }

                for(int i = 0 ; i < 7; ++i)
                {
                    SummitGem.P_Shatter.Color = shatter_color;
                    level.ParticlesFG.Emit(SummitGem.P_Shatter, 5, player.Center, Vector2.One * 4f, (float)Math.PI*2f*i/7); 
                }

                bloom.Visible = false;
                bloom.RemoveSelf();

                player.Hair.Visible = true;
                player.StateMachine.State = Player.StNormal;
            }

            if (sound_name != "")
            {
                sfx.Source.Param("end", 1f);
            }
            Audio.PauseMusic = false;

            level.CanRetry = true;
            level.Session.DoNotLoad.Add(eid);

            RemoveSelf();

        }

    }
}
