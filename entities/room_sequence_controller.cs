using System.Linq;

using Microsoft.Xna.Framework;

using Monocle;

using Celeste;
using Celeste.Mod.Entities;

namespace Celeste.Mod.ErrandOfWednesday
{

    [Tracked]
    [CustomEntity("eow/RoomSequenceController")]
    public class RoomSequenceController : Entity
    {
        /* situation: there are 15 competing standards */

        public string sequence_name;
        public int sequence_number;

        public string prefix;
        public string this_flag;
        public string prev_flag;
        public string next_flag;

        public string flag_on_transition;
        public bool flag_on_transition_inverted;

        public RoomSequenceController(EntityData data, Vector2 offset, EntityID eid) : base(data.Position+ offset)
        {
            sequence_number = data.Int("sequence_number", 0);
            sequence_name = data.Attr("sequence_name", "");

            flag_on_transition_inverted = Flagic.process_flag(
                    data.Attr("flag_on_transition", ""),
                    out flag_on_transition);
 
            prefix = $"eow_room_sequence_{sequence_name}";
            this_flag = flag_name(sequence_name, sequence_number);
            prev_flag = flag_name(sequence_name, sequence_number-1);
            next_flag = flag_name(sequence_name, sequence_number+1);
            

        }

        public string flag_name(string sequence_name, int sequence_number)
        {
            return $"{prefix}_{sequence_number}";
        }
        
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Level level = SceneAs<Level>();
            Session session = level.Session;

            if(session.GetFlag(this_flag) && sequence_number != 0)
            {
                return;
            }

            if((!session.GetFlag(prev_flag) && !session.GetFlag(next_flag)) || sequence_number == 0)
            {
                foreach (string flag in session.Flags.Where(s => s.StartsWith(prefix)) )
                {
                    session.SetFlag(flag, false);
                } 
                if(sequence_number == 0)
                {
                    transition_to(session);
                }
            }
            else
            {
                transition_to(session);
            } 

        }

        public void transition_to(Session session)
        {
            session.SetFlag(prev_flag, false);
            session.SetFlag(next_flag, false);
            session.SetFlag(this_flag);

            if(!string.IsNullOrWhiteSpace(flag_on_transition))
            {
                session.SetFlag(flag_on_transition, !flag_on_transition_inverted);
            }
 
        }

    }

}

