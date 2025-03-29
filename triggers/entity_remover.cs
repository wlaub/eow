using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Monocle;

using Celeste;
using Celeste.Mod.Entities;

namespace Celeste.Mod.ErrandOfWednesday
{

    [Tracked]
    [CustomEntity("eow/EntityRemover")]
    public class EntityRemover : Trigger 
    {

        public readonly string flag;
        public readonly bool invert;
        public readonly bool on_load;
        public bool remove_player = false;
        public Vector2[] nodes;
        public List<Entity> targets;

        public bool triggered = false;

        public EntityRemover (EntityData data, Vector2 offset) : base(data, offset)
        {
            nodes = data.NodesOffset(offset);
            invert = data.Bool("invert");
            on_load = data.Bool("on_load");
            flag = data.Attr("flag");
            remove_player = data.Bool("remove_player", false);

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
                            targets.Add(e);
                            break;
                        }
                    }
                    catch (NotImplementedException)
                    {
                        try
                        {
                            if(e.CollideRect(new Rectangle((int)n.X-4, (int)n.Y-4, 8,8)))
                            {
                                targets.Add(e);
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

        public void remove_entities()
        {
            if(triggered) return;

            Level level = SceneAs<Level>();

            foreach(Entity e in targets)
            {
                if(remove_player || !(e is Player)){
                level.Remove(e);}
            }

            triggered = true;

        }

        public bool check()
        {
            if(flag == "") return true;
            return SceneAs<Level>().Session.GetFlag(flag) != invert;
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
