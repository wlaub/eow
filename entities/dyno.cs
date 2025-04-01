
using Microsoft.Xna.Framework;

using Monocle;

using Celeste;
using Celeste.Mod.Entities;

namespace Celeste.Mod.ErrandOfWednesday
{

    [Tracked]
    [CustomEntity("eow/Dyno")]
    public class Dyno : Entity
    {

        public Vector2 home_position;
        public Sprite sprite;

        public Holdable Hold;

        public bool activated = false;

        public bool single_use;
        public float radius;

        public float yboost_threshold;
        public float base_yboost;
        public float dash_yboost;
        public float base_xboost;
        public float dash_xboost;
        public float diag_xboost;
        public string idle_sprite;
        public string active_sprite;
        public string used_sprite;
        public bool remove_sprite = false;

        public int base_depth = 8999;

        public EntityID eid;
        
        public Dyno(EntityData data, Vector2 offset, EntityID eid) : base(data.Position+ offset)
        {
            //TODO hover sprite and hitbox?
            yboost_threshold = data.Float("yboost_threshold");
            base_yboost = data.Float("yboost");
            dash_yboost = data.Float("yboost_dash");
            base_xboost = data.Float("xboost");
            dash_xboost = data.Float("xboost_dash");
            diag_xboost = data.Float("xboost_diag");
            single_use = data.Bool("single_use");
            radius = data.Float("radius");

            idle_sprite = data.Attr("idle_sprite");
            active_sprite = data.Attr("active_sprite");
            used_sprite = data.Attr("used_sprite");

            if(GFX.SpriteBank.Has(idle_sprite))
            {
                Add(sprite = GFX.SpriteBank.Create(idle_sprite));
            }
            else
            {
                Add(sprite = new Sprite(GFX.Game, ""));
                sprite.AddLoop("idle", idle_sprite, 0.08f);

                sprite.AddLoop("used", used_sprite, 0.08f);
                if(used_sprite == "")
                {
                    remove_sprite = true;
                }

                sprite.Add("active_single", active_sprite, 0.08f, "used");
                sprite.Add("active_multi", active_sprite, 0.08f, "idle");
 
                sprite.CenterOrigin();
            }
            sprite.Play("idle");

            base.Collider = new Circle(radius, 0f, 0f);
            Add(Hold = new Holdable(data.Float("holdoff_duration",0.65f)));
            Hold.PickupCollider = new Circle(radius, 0f, 0f);
            Hold.SlowFall = false;
            Hold.SlowRun = false;
            Hold.OnPickup = OnPickup;
            Hold.OnCarry = OnCarry;
            home_position = Position;
            base.Depth = base_depth;
        }

	    public void OnCarry(Vector2 position)
    	{
        }

        public void OnPickup()
        {
            activated = true;
            if(single_use)
            {
                sprite.Play("active_single");
                if(remove_sprite)
                {
                    sprite.OnLastFrame = delegate(string anim)
                    {
                        if (anim == "active_single")
                        {
                            RemoveSelf();
                            Visible = false;
                        }
                    };
                }
            }
            else
            {
                sprite.Play("active_multi");
            }


            Player player = Hold.Holder;
            float yboost = 0;
            float xboost = base_xboost;
            if(player.Speed.Y < yboost_threshold)
            {
                yboost = -base_yboost;
                if(player.StateMachine.State == Player.StDash)
                {
                    yboost = -dash_yboost;
                } 
            }
            
            if(player.StateMachine.State == Player.StDash)
            {
                //horizontal dash
                if(player.Speed.Y == 0)
                {
                    xboost = dash_xboost;
                }
                //diagonal down dash
                else if(player.Speed.Y > 0)
                {
                    xboost = diag_xboost;
                }
            }

            if(player.Speed.Y > 0)
            {
                player.Speed.Y = 0;
            }
            player.Speed.Y += yboost;
            if(player.Speed.X > 0)
            {
                player.Speed.X += xboost;
            }
            if(player.Speed.X < 0)
            {
                player.Speed.X -= xboost;
            }
            base.Depth = base_depth;
        }

        
public override void Awake(Scene scene)
        {
            base.Awake(scene);
        }

        public override void Update()
        {
            base.Update();
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();

            if(activated && player != null)
            {
                if(!CollideCheck(player))
                {
                    activated = false;
                    if(Hold.IsHeld)
                    {
                        Hold.Holder.Drop();
                        if(single_use)
                        {
                            Remove(Hold);
                        }
                    }
                }
            }
        }

    }

}

