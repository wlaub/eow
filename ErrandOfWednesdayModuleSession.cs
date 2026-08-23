using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Monocle;

using Celeste;

namespace Celeste.Mod.ErrandOfWednesday {
    public class ErrandOfWednesdayModuleSession : EverestModuleSession {

        public bool sd_active = false;
        public float sd_checkpoint_time = 0f;
        public string sd_countdown_sound;
        public string sd_death_sound;
        public string sd_timer_color;

        public EstateState estate_state;

        public InvarianceState invariance_state;

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

    public class InvarianceState {
        public List<InvariantEntityState> entities = new();

        public void restore_state(Session session, Level level){
            foreach (InvariantEntityState entry in entities){
                if(entry.is_follower && entry.is_held){
                    //the game will restore it and it will be made invariant later
                    continue;
                    }
Logger.Log(LogLevel.Info, "eow", $"restoring {entry.id} {entry.room_name} at {entry.x} {entry.y}");
                Entity e;
                EntityData entity_data = entry.data.to_entity_data(level.Session);
                if (Level.LoadCustomEntity(entity_data, level)){
                    e = level.Entities.toAdd[^1];
                    }
                else{
                    if(entity_data.Name == "key"){
                        e = new Key(entity_data, Vector2.Zero, entry.id);
                        level.Add(e);
                        }
                    else{
                        continue;
                        }
                    }
                if(e is not null){
Logger.Log(LogLevel.Info, "eow", $"the item was created");

                    EyeOfTheWednesday.invariance_states[e] = entry;//has to come before make_invariant
                    e.Active = level.Session.LevelData.Name == entry.room_name;
                    e.Position = new Vector2(entry.x, entry.y); //must come before make_invariant
                    EyeOfTheWednesday.make_invariant(level, e, entry.room_name, entry.is_follower);

                    }
                }
            }

        public void save_entity(Entity entity, string room_name, bool is_follower, bool is_held){
            EyeOfTheWednesday._save_invariant_entity(this, entity, room_name, is_follower, is_held);
            }

        }

    public class InvariantEntityState {
        public EntityID id;
        public SerializableEntityData data = new();
        public float x;
        public float y;
        public string room_name;
        public bool is_follower;
        public bool is_held;

        public void update_from(Entity e, string room_name, bool is_follower, bool is_held){
            id = e.SourceId;
            
//            data = e.SourceData;
            clone_data(e.SourceData);

            x = e.Position.X;
            y = e.Position.Y;
            this.room_name = room_name;
            this.is_follower = is_follower;
            this.is_held = is_held;
 
            }

        public void clone_data(EntityData d){
            //in python you can just import copy
            data.ID = d.ID;
            data.room_name = d.Level.Name;
            data.Name = d.Name;
            data.Position = d.Position;
            data.Origin = d.Origin;
            data.Width = d.Width;
            data.Height = d.Height;
            data.Nodes = d.Nodes;
            data.Values = d.Values;
            }

        }

    public class SerializableEntityData {
        public int ID;
        public string Name;
        public string room_name;
        public Vector2 Position;
        public Vector2 Origin;
        public int Width;
        public int Height;
        public Vector2[] Nodes;
        public Dictionary<string, object> Values;

        public EntityData to_entity_data(Session session){
            EntityData r = new();
            r.ID = ID;
            r.Name = Name;
            r.Level = session.MapData.Get(room_name);
            r.Position = Position;
            r.Origin = Origin;
            r.Width = Width;
            r.Height = Height;
            r.Nodes = Nodes;
            r.Values = Values;

            return r;
            }
        
        }

}
