using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Monocle;

using Celeste;
using Celeste.Mod.Entities;

namespace Celeste.Mod.ErrandOfWednesday
{

    [Tracked]
    [CustomEntity("eow/EntityGlobalizer")]
    public class EntityGlobalizer : Trigger 
    {

        public string enable_flag;
        public bool enable_flag_inverted;
        public bool on_load;
        public bool remove_player = false;
        public Vector2[] nodes;
        public List<Entity> targets;

        public string type_filter;

        public bool triggered = false;

        public EntityGlobalizer (EntityData data, Vector2 offset) : base(data, offset)
        {
            nodes = data.NodesOffset(offset);

            enable_flag_inverted = Flagic.process_flag(data.Attr("flag"), out enable_flag);
 
            on_load = data.Bool("on_load");
            remove_player = data.Bool("remove_player", false);

            type_filter = data.Attr("type_filter", "");

            targets = new();

        }

        public void find_entities(Scene scene)
        {
            foreach(Entity e in scene.Entities)
            {
                foreach(Vector2 n in nodes)
                {
                    try
                    {
                        if(e.CollidePoint(n))
                        {
                            add_entity(e);
                            break;
                        }
                    }
                    catch (NotImplementedException)
                    {
                        try
                        {
                            if(e.CollideRect(new Rectangle((int)n.X-4, (int)n.Y-4, 8,8)))
                            {
                                add_entity(e);
                                break;
                            }
                        }
                        catch (NotImplementedException)
                        {
                            break;
                        }
                    }
                }
            }
        }
        public void add_entity(Entity e)
        {
// Logger.Log(LogLevel.Debug, "eow", $"{e.GetType().FullName} {type_filter}");
            if(String.IsNullOrWhiteSpace(type_filter) || e.GetType().FullName == type_filter)
            {
// Logger.Log(LogLevel.Debug, "eow", $"^matched");
                targets.Add(e);
            }
        }

        public void remove_entities()
        {
            if(triggered) return;

            Level level = SceneAs<Level>();

            foreach(Entity e in targets)
            {
                if(remove_player || !(e is Player)){
                e.AddTag(Tags.Global);}
            }

            triggered = true;

        }

        public bool check()
        {
            return Flagic.test_flag(SceneAs<Level>().Session, enable_flag, enable_flag_inverted);
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);

            if(check())
            {
                remove_entities();
            }

        }

        public override void Awake(Scene scene)
        {
            find_entities(scene);
            if(on_load && check())
            {
                remove_entities();
            }
        }

    }
    

}
