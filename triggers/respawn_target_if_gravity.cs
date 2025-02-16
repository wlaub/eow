using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Monocle;

using Celeste;
using Celeste.Mod.Entities;

namespace Celeste.Mod.ErrandOfWednesday
{

    [Tracked]
    [TrackedAs(typeof(RespawnTargetTrigger))]
    [CustomEntity("eow/RespawnTargetIfGravity")]
    public class RespawnTargetIfGravity : RespawnTargetTrigger
    {

        public RespawnTargetIfGravity (EntityData data, Vector2 offset) : base(data, offset)
        {
            bool gravity_inverted = (GravityHelperImports.GetPlayerGravity?.Invoke() ?? 0) != 0;


            bool on_gravity_inverted = data.Bool("gravity_inverted", false);

            Level level = Engine.Scene as Level;

            bool flag_condition = false;
            string flag = data.Attr("flag");
            if(string.IsNullOrWhiteSpace(flag))
            {
                flag_condition = true;
            }
            else if(level != null)
            {
                bool flag_inverted = Flagic.process_flag(flag, out flag);
                flag_condition = Flagic.test_flag(level.Session, flag, flag_inverted);
            }
            if(gravity_inverted == on_gravity_inverted && flag_condition)
            {
                base.Collider = new Hitbox(data.Width, data.Height);
                Target = data.Nodes[0] + offset;
            }
            else
            {
                base.Collider = null;
            }
            Visible = (Active = false);
        }

    }
    

}
