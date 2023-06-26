
using Microsoft.Xna.Framework;

using Monocle;

using Celeste;
using Celeste.Mod.Entities;

namespace Celeste.Mod.ErrandOfWednesday
{

    [Tracked]
    [CustomEntity("eow/UniqueJellyfish")]
    public class UniqueGlider : Glider 
    {

        public bool confiscate;

        public bool awakened = false;

        public UniqueGlider(EntityData e, Vector2 offset) : base(e, offset)
        {
            confiscate = e.Bool("confiscate");
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);

            if(awakened) return;

            //TODO: currently only confiscates other unique gliders
            //Might be neat to confiscate any glider for more consistent
            //behavior, but i don't currently plan to use this with gliders
            //that aren't unique.
            if(scene is Level level)
            {
                Player player = level.Tracker.GetEntity<Player>();
                UniqueGlider nearest = null;
                Glider held = null;
                float best_dist = -1;
                if (player != null)
                {
                    //Find the nearest non-held unique jellyfish
                    foreach(UniqueGlider e in level.Tracker.GetEntities<UniqueGlider>())
                    {
                        if(player.Holding != null && player.Holding == e.Hold)
                        {
                            held = e;
                        }
                        else if(level.IsInBounds(e))
                        {
                        
                            float dist = Vector2.DistanceSquared(player.Position, e.Position);
                            if(dist < best_dist || best_dist < 0)
                            {
                                best_dist = dist;
                                nearest = e;
                            }
                        }
                    }

                    //determine which one to keep
                    if(nearest != null && held != null)
                    {
                        if(nearest.confiscate)
                        {
                            player.Drop();
                            level.Remove(held);
                            held = null;
                        }
                        else
                        {
                            nearest = null;
                        }
                    }

                    //remove all the rest
                    foreach(UniqueGlider e in level.Tracker.GetEntities<UniqueGlider>())
                    {
                        e.awakened = true;
                        if(e != nearest && e != held)
                        {
                            level.Remove(e);
                        }
                    } 
                   
                }
            }       
        }


    }

}

