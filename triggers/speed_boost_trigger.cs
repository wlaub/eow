using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Monocle;

using Celeste;
using Celeste.Mod.Entities;

namespace Celeste.Mod.ErrandOfWednesday
{

    [Tracked]
    [CustomEntity("eow/SpeedBoostFieldTrigger")]
    public class SpeedBoostFieldTrigger : Trigger 
    {

        public float speed;
        public Vector2 velocity;

        public bool set_speed = true;
        public bool triggered = false;
        public bool while_inside = true;
        public bool on_enter = false;
        public bool while_dashing = false;

        public SpeedBoostFieldTrigger (EntityData data, Vector2 offset) : base(data, offset)
        {
            on_enter = data.Bool("on_enter", false);
            set_speed = data.Bool("set_speed", false);
            while_inside = data.Bool("while_inside", true);
            while_dashing = data.Bool("while_dashing", false);


            speed = data.Float("speed");
            velocity = data.NodesOffset(offset)[0]-Position-new Vector2(Width/2,Height/2);
            velocity.Normalize();
            velocity*=speed;

        }

        public override void Update()
        {
            base.Update();

            if(while_inside && PlayerIsInside)
            {
                Player player = (Scene as Level).Tracker.GetEntity<Player>();
                if(player != null && (player.StateMachine.state != 2 || while_dashing))
                {
                    if(set_speed)
                    {
                        player.Speed = velocity;
                    }
                    else
                    {
                        player.Speed += velocity;
                    }
                }

            }
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);

            if (!on_enter) {return;}

            triggered = true;
            if(set_speed)
            {
                player.Speed = velocity;
            }
            else
            {
                player.Speed += velocity;
            }

        }

    }
    

}
