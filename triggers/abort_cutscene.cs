using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Monocle;

using Celeste;
using Celeste.Mod.Entities;

namespace Celeste.Mod.ErrandOfWednesday
{

    [Tracked]
    [CustomEntity("eow/AbortCutscene")]
    public class AbortCutscene : Trigger 
    {

        public readonly string flag;
        public readonly bool flag_inverted;

        public AbortCutscene (EntityData data, Vector2 offset) : base(data, offset)
        {
            flag = data.Attr("flag");
            if(!string.IsNullOrWhiteSpace(flag) && flag[0] == '!')
            {
                flag_inverted = true;
                flag = flag.Substring(1);
            }
 

        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            Level level = SceneAs<Level>();
            if(level.InCutscene || player.Dead)
            {
                level.CancelCutscene();
                level.Completed = false;
                level.Session.InArea = true;
                player.StateMachine.State = 0;
            }
        }

/*
        public static bool loaded = false;

        public static void try_load()
        {
            if(loaded){return;}
            On.Celeste.MapData.CanTransitionTo += can_transition_to;
            loaded = true;
        }
        public static void unload()
        {
            if(!loaded){return;}
            On.Celeste.MapData.CanTransitionTo -= can_transition_to;
            loaded = false;
        }

        public static bool can_transition_to(On.Celeste.MapData.orig_CanTransitionTo orig, MapData self, Level level, Vector2 position)
        {
            bool result = orig(self, level, position);
            foreach(AbortCutscene trigger in level.Tracker.GetEntities<AbortCutscene>())
            {
              if( trigger.PlayerIsInside && 
                  (string.IsNullOrWhiteSpace(trigger.flag) || level.Session.GetFlag(trigger.flag) != trigger.flag_inverted)
                )
                {
                    return false;
                }
            }
            return result;
        }
*/
    }
    

}
