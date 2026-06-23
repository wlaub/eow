using System.Collections.Generic;

namespace Celeste.Mod.ErrandOfWednesday {
    public class ErrandOfWednesdayModuleSession : EverestModuleSession {

        public bool sd_active = false;
        public float sd_checkpoint_time = 0f;
        public string sd_countdown_sound;
        public string sd_death_sound;
        public string sd_timer_color;

        public EstateState estate_state;

    }

    public class EstateState {
//        public List<EstateRoomState> drafted_rooms = new();
        public Dictionary<string,EstateRoomState> drafted_rooms = new();
        }

    public class EstateRoomState {
        public string key;
        public int xstart;
        public int ystart;
        public int xpos;
        public int ypos;
        }


}
