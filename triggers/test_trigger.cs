using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Monocle;

using Celeste;
using Celeste.Mod.Entities;

namespace Celeste.Mod.ErrandOfWednesday
{

    [Tracked]
    [CustomEntity("eow/TestTrigger")]
    public class TestTrigger : Trigger 
    {

        public readonly string flag;
        public readonly bool invert;
        public readonly bool on_load;
        public bool remove_player = false;
        public Vector2[] nodes;
        public List<Entity> targets;

        public string type_filter;

        public bool triggered = false;

        public TestTrigger (EntityData data, Vector2 offset) : base(data, offset)
        {
            nodes = data.NodesOffset(offset);
            invert = data.Bool("invert");
            on_load = data.Bool("on_load");
            flag = data.Attr("flag");
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

        public bool check()
        {
            return true;
            if(flag == "") return true;
            return SceneAs<Level>().Session.GetFlag(flag) != invert;
        }

        public void activate()
        {
           int target_x = 46*8;
           int target_y = -18*8;

Logger.Log(LogLevel.Info, "eow", $"{nodes[0]}");
 
           EstateController.move_room(SceneAs<Level>(), flag, (int)(nodes[0].X), (int)(nodes[0].Y));
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);

            if(check())
            {
                activate();
            }

        }

        public override void Awake(Scene scene)
        {
            find_entities(scene);
            if(on_load && check())
            {
                activate();
            }
        }

    }
    

}
