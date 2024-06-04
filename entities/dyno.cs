
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

        public float yboost_threshold;
        public float base_yboost;
        public float dash_yboost;
        public float base_xboost;
        public float dash_xboost;
        public float diag_xboost;

        public EntityID eid;
        
        public Dyno(EntityData data, Vector2 offset, EntityID eid) : base(data.Position+ offset)
        {
            yboost_threshold = data.Float("yboost_threshold");
            base_yboost = data.Float("yboost");
            dash_yboost = data.Float("yboost_dash");
            base_xboost = data.Float("xboost");
            dash_xboost = data.Float("xboost_dash");
            diag_xboost = data.Float("xboost_diag");

            Add(sprite = GFX.SpriteBank.Create("booster"));
            base.Collider = new Circle(10f, 0f, 0f);
            Add(Hold = new Holdable(0.3f));
            Hold.PickupCollider = new Circle(10f, 0f, 0f);
            Hold.SlowFall = false;
            Hold.SlowRun = false;
            Hold.OnPickup = OnPickup;
            home_position = Position;
            base.Depth = 8999;
        }

        private void OnPickup()
        {
            activated = true;
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
        }

        
public override void Awake(Scene scene)
        {
            base.Awake(scene);
        }

        public override void Update()
        {
            base.Update();
            Position = home_position;
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();

            if(activated)
            {
                if(!CollideCheck(player))
                {
                    activated = false;
                    if(Hold.IsHeld)
                    {
                        Hold.Holder.Drop();
                    }
                }
            }
        }

    }

}

