
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

        public UniqueGlider(EntityData e, Vector2 offset) : base(e, offset)
        {
            confiscate = e.Bool("confiscate");
        }


    }

}

