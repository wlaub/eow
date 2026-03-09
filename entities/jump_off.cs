
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


        public bool player_moving_up = false;

        public EntityID eid;
        
        public Vector2 left;
        public Vector2 right;

        public bool player_was_above = false;
        public bool active = false;

        public JumpOff(EntityData data, Vector2 offset, EntityID eid) : base(data.Position+ offset)
        {
            Collider = new Hitbox(data.Width, 5f, 0, -1);

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
                if(!player_above && player_was_above && player.Right >= Position.X && player.Left <= Position.X+Width) 
                {
                    //after falling through, enable jump while in hitbox
                    player.jumpGraceTimer=0.02f;
                    active = true;
                }
                if(player_above && !player_was_above && player.Right >= Position.X && player.Left <= Position.X+Width) 
                {
                    //after jumping through, enable buffered jump
                    player.jumpGraceTimer=0.04f;

                }

                if(CollideCheck(player) && player.Bottom <= Bottom)
                {
                    if((player.StateMachine.State == Player.StDash || player.StateMachine.State == Player.StRedDash) && player.Speed.Y ==0)
                    {
                        //if dashing horizontally inside, give full coyote time and bump to dop
                        player.Position.Y = Position.Y;
                        player.jumpGraceTimer = 0.1f;
                    }
                    else if(active)
                    {
                        //if the player fell through, allow jump as long as player remains inside
                        player.jumpGraceTimer = 0.02f;
                    }

                }
                else
                {
                    active = false;
                }


                player_was_above = player_above;
            }
        }

    }

}

