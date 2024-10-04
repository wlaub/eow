
using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Monocle;

using Celeste;
using Celeste.Mod.Entities;

namespace Celeste.Mod.ErrandOfWednesday
{

    [Tracked]
    [CustomEntity("eow/MusicLayerSource")]
    public class MusicLayerSource : Entity
    {

        public List<string> layers = new();
        public bool track_player;
        public float z_distance_sq;
        public float min_distance;
        public float max_distance; 
        public bool convert_distance;

        public float max_dist_sq;
        public float range;

        public float unflatten_distance(float pixel_distance)
        {
            return (float)Math.Sqrt(pixel_distance*pixel_distance+z_distance_sq);
        }

        public MusicLayerSource(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
//Logger.Log(LogLevel.Info, "eow", "");
            track_player = data.Bool("track_player", true);
            z_distance_sq = data.Float("z_distance", 345f);
            z_distance_sq*=z_distance_sq;
            min_distance = data.Float("min_distance", 16);
            max_distance = data.Float("max_distance", 120);
            convert_distance = data.Bool("convert_distance", true);

            if(convert_distance)
            {
                min_distance = unflatten_distance(min_distance);
                max_distance = unflatten_distance(max_distance);
            }

            range = max_distance-min_distance;
            max_dist_sq = max_distance*max_distance;

            string layer_list = data.Attr("layers");
            string[] parts = layer_list.Split(',');
            foreach(string layer in parts)
            {
                string _layer = layer.Trim();
                //0 -> layer0 etc
                if(_layer != "")
                {
                    int idx;
                    if(Int32.TryParse(_layer, out idx))
                    {
                        _layer = $"layer{idx}";
                    }
                    layers.Add(_layer);
                }
            }



        }

        public float get_distance(Vector2 target)
        {

            float dist = Vector2.DistanceSquared(Position, target) + z_distance_sq;

            if(dist > max_dist_sq)
            {
                return 0f;
            }
            dist = (float)Math.Sqrt(dist);

            if(dist < min_distance)
            {
                return 1f;
            }
            else if(dist > max_distance)
            {
                return 0f;
            }
            else
            {
                return (max_distance - dist)/range;
            }
        }

        /* Actual implementation */

        public static bool loaded = false;

        public static void Load()
        {
            if(loaded)
            {
                return;
            }
            On.Monocle.Engine.Update += update_hook;
            loaded = true;
        }

        public static void Unload()
        {
            if(!loaded)
            {
                return;
            }
            On.Monocle.Engine.Update -= update_hook;

            clear_state();
            loaded = false;
        }

        public static void try_load(Level level)
        {
            if(!MyLevelInspect.entity_in_map(level.Session, "eow/MusicLayerSource"))
            {
                return;
            }

            clear_state();
            gather_sources(level);
            Load();
        }

        public static void update_hook(On.Monocle.Engine.orig_Update orig, Engine self, GameTime gameTime)
        {
            orig(self, gameTime);

            Level level = self.scene as Level;
            if(level ==  null)
            {
                return;
            }

            do_update(level);
        }


        public static List<MusicLayerSource> camera_sources = new();
        public static List<MusicLayerSource> player_sources = new();
        public static Dictionary<string, float> distances = new();

        public static void gather_sources(Level level)
        {


            foreach(EntityData entity_data in MyLevelInspect.get_all_entity_data(level.Session, "eow/MusicLayerSource") )
            {
                LevelData level_data = entity_data.Level;
                Vector2 offset = new Vector2(level_data.Bounds.Left, level_data.Bounds.Top);

                level.Session.DoNotLoad.Add(new EntityID(level_data.Name, entity_data.ID));

                if (Level.EntityLoaders.TryGetValue(entity_data.Name, out var value))
                {
                    MusicLayerSource source = value(level, level.Session.LevelData, offset, entity_data) as MusicLayerSource;
                    foreach(string layer in source.layers)
                    {
                        distances[layer] = 0;
                    }

                    if(source.track_player)
                    {
                        player_sources.Add(source);
                    }
                    else
                    {
                        camera_sources.Add(source);
                    }
                }                       
                    
            }
        }

        public static void clear_state()
        {
            player_sources.Clear();
            camera_sources.Clear();
            distances.Clear();
        }

        public static void do_update(Level level)
        {
            Camera camera = level.Camera;           
            Player player = level.Tracker.GetEntity<Player>();           

            foreach(string layer in distances.Keys)
            {
                distances[layer] = 0;
            }

            foreach(MusicLayerSource source in camera_sources)
            {
                float distance = source.get_distance(camera.Position);
                foreach(string layer in source.layers)
                {
                    distances[layer] += distance;
                }
            }
            
            if(player != null)
            {
                foreach(MusicLayerSource source in player_sources)
                {
                    float distance = source.get_distance(player.Position);
                    foreach(string layer in source.layers)
                    {
                        distances[layer] += distance;
                    }
                }
            }

            AudioState audio = level.Session.Audio;

            foreach(KeyValuePair<string, float> entry in distances)
            {
                float value = entry.Value;
                if(value > 1)
                {
                    value = 1;
                }
                if(value < 0)
                {
                    value = 0;
                }
                audio.Music.Param(entry.Key, value);
            }

            audio.Apply(forceSixteenthNoteHack: false);
        }

        //List of MusicLayerSource entities
        //Function to calculate layer distances and update music on update
        /*
        AudioState audio = SceneAs<Level>().Session.Audio;
        audio.Music.Layer(layer, effective_volume);
        audio.Apply(forceSixteenthNoteHack: false);
        */

        //On load level, search for MusicLayerSource instances
        //clear state
        //Populate entity list
        //DNL all entities
        //Load hooks
        //Unload hooks

    }
}
