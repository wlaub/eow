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
            if(flag == "") return true;
            return SceneAs<Level>().Session.GetFlag(flag) != invert;
        }

        public void activate()
        {
            Level level = SceneAs<Level>();


            int target_x;
            int target_y;


            int nx = (int)nodes[0].X;
            int ny = (int)nodes[0].Y;

            LevelData this_level = level.Session.LevelData;            

            int side=0;
            if(nx > this_level.Bounds.Right)
            {//enter left
                side=0;
            }
            else if(nx < this_level.Bounds.Left)
            {//enter right
                side=1;
            }
            else if(ny < this_level.Bounds.Top)
            {//enter bot
                side=3;
            }
            else if(ny > this_level.Bounds.Bottom)
            {//enter top
                side=2;
            }
            else {return;}

            EstateController.draft_room(level, this_level, side);
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
