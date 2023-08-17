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
   
        public string poem_dialog;
        public string flag;

        public Color shatter_color;

        public float strength;

        public bool collected = false;

        public Wiggler move_wiggler;
        public Vector2 move_wiggler_dir;

        public PowerupCollectable(EntityData data, Vector2 offset) : base(data.Position + offset)
        {

            do_pulse = data.Bool("do_pulse");
            must_dash_toward = data.Bool("must_dash_toward");
            center_player = data.Bool("center_player");
            show_animation = data.Bool("show_animation");
            show_poem = data.Bool("show_poem");

            strength = data.Float("strength");

            flag = data.Attr("flag");
            poem_dialog = data.Attr("poem_dialog");
            
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

	    	base.Collider = new Hitbox(16f, 16f, -8f, -8f);
    		Add(new PlayerCollider(OnPlayer));            

        }

        public override void Update()
        {
            base.Update();
            sprite.Position = move_wiggler_dir * move_wiggler.Value * -8f;

        }

        public void activate_nodes(Player player)
        {
            if(player is null || player.Scene is null)
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
            move_wiggler.Start();
            move_wiggler_dir = (Center-player.Center).SafeNormalize(Vector2.UnitY);

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

        public IEnumerator collect_coroutine(Player player)
        {   
            Visible = false;
            Collidable = false;

            Level level = SceneAs<Level>();
            
            player.StateMachine.State = Player.StDummy;
            player.DummyGravity = false;
            player.DummyMoving = true;
            player.DummyAutoAnimate = false;
            //player.Visible = false;

            player.Sprite.Play(PlayerSprite.StartStarFly);
            player.Hair.Visible = false;

            //Somehow make starfly happen without hair

            SoundEmitter.Play("event:/game/07_summit/gem_get", this);

 
            for(int i = 0; i < 10; ++ i)
            {
                Scene.Add(new AbsorbOrb(Position, player));
            }

            for(float t = 0f; t < 2f; t+= Engine.RawDeltaTime)
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

                yield return null;
            }

            activate_nodes(player);

            for(float t = 0f; t < 0.1f; t+= Engine.RawDeltaTime)
            {
                yield return null;
            }

//            player.DummyAutoAnimate = true;
//            player.DummyGravity = true;

            DynamicData pdata = new DynamicData(player);
            player.Speed.Y = strength;
            for(float t = 0f; t< 4f; t+= Engine.RawDeltaTime)
            {
/*                if(pdata.Get<bool>("OnGround"))
                {
                    break;
                }*/
                yield return null;
            }

            for(float t = 0f; t < 0.5f; t+= Engine.RawDeltaTime)
            {
                yield return null;
            }

            for(int i = 0 ; i < 16; ++i)
            {
                SummitGem.P_Shatter.Color = shatter_color;
                level.ParticlesFG.Emit(SummitGem.P_Shatter, 5, player.Center, Vector2.One * 4f, (float)Math.PI*2f*i/16); 
            }

            player.Hair.Visible = true;
            player.StateMachine.State = Player.StNormal;
            RemoveSelf();

        }

    }
}
