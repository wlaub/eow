
using Microsoft.Xna.Framework;

using Monocle;

using Celeste;
using Celeste.Mod.Entities;

namespace Celeste.Mod.ErrandOfWednesday
{

    [Tracked]
    [CustomEntity("eow/JumpOff")]
    public class JumpOff : Entity
    {

        public bool activated = false;

        public bool player_moving_up = false;

        public EntityID eid;

        public Hitbox dash_box;
        public Hitbox jump_box;
        
        public Vector2 left;
        public Vector2 right;

        public bool player_was_above = false;

        public JumpOff(EntityData data, Vector2 offset, EntityID eid) : base(data.Position+ offset)
        {
            dash_box = new Hitbox(data.Width, 5f, 0, -1);
            jump_box = new Hitbox(data.Width, 3f);
            Collider = jump_box;

            left = new Vector2(Position.X, Position.Y);
            right = new Vector2(Position.X+data.Width, Position.Y);
        }

       
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
        }

        public override void Render()
        {
            Draw.Line(left, right, Color.White);
        }

        public override void Update()
        {
            base.Update();
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();

            if(player != null)
            {
                
                bool player_above = player.Bottom <= Position.Y;
                if(player_above != player_was_above && player.Right >= Position.X && player.Left <= Position.X+Width) 
                {
                    player.jumpGraceTimer=0.1f;
                }

                player_was_above = player_above;

                Collider = dash_box;

                if((player.StateMachine.State == Player.StDash || player.StateMachine.State == Player.StRedDash) && player.Speed.Y ==0 && CollideCheck(player) && player.Bottom <= Bottom)
                {
                    player.Position.Y = Position.Y;
                    player.jumpGraceTimer = 0.1f;
                }
/*  
                Collider = jump_box;
              
                if(CollideCheck(player) && player.Bottom <= Bottom)
                {
                    if(player.Speed.Y >= 0)
                    {
                        player.jumpGraceTimer = 0.1f;
                        player_moving_up = false;
                    }
                    else
                    {
                        player_moving_up = true;
                    }
                }
                else if(player_moving_up)
                {
                        player.jumpGraceTimer = 0.05f;
                        player_moving_up = false;
                    
                }
                Collider = dash_box;
*/
            }
        }

    }

}

