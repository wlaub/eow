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
            int target_x = 46*8;
            int target_y = -18*8;

            Level level = SceneAs<Level>();

            int nx = (int)nodes[0].X;
            int ny = (int)nodes[0].Y;

            LevelData this_level = level.Session.LevelData;            

            int side=0;
            if(nx > this_level.Bounds.Right)
            {//enter left
                side=0;
                target_x = this_level.Bounds.Right;
                target_y = this_level.Bounds.Y;
            }
            else if(nx < this_level.Bounds.Left)
            {//enter right
                side=1;
                target_x = this_level.Bounds.Left-46*8;
                target_y = this_level.Bounds.Y;
            }
            else if(ny < this_level.Bounds.Top)
            {//enter bot
                side=3;
                target_x = this_level.Bounds.X;
                target_y = this_level.Bounds.Y-36*8;
            }
            else if(ny > this_level.Bounds.Bottom)
            {//enter top
                side=2;
                target_x = this_level.Bounds.X;
                target_y = this_level.Bounds.Bottom;
            }
            else {return;}

            if(level.Session.MapData.GetAt(nodes[0]) != null){return;}

            List<EstateRoomInfo> pool = EstateController.make_pool(side);


            if(pool.Count == 0){return;}


foreach(EstateRoomInfo option in pool)
{
Logger.Log(LogLevel.Info, "eow", $"  {option.key}");
}

Logger.Log(LogLevel.Info, "eow", $"side={side}");
            EstateRoomInfo draft = Calc.Random.Choose(pool);
Logger.Log(LogLevel.Info, "eow", $"drafting {draft.key}");

            level.Add(new DraftMenu(pool, target_x, target_y));
//            EstateController.drafted_rooms.Add(draft.key); 
//            EstateController.move_room(level, draft.key, target_x, target_y);
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
