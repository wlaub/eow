using System.Collections;

using Microsoft.Xna.Framework;

using Monocle;

using Celeste;
using Celeste.Mod.Entities;

namespace Celeste.Mod.ErrandOfWednesday
{

    [Tracked]
    [CustomEntity("eow/InvisibleSpinner")]
    public class InvisibleSpinner : CrystalStaticSpinner
    {

        public InvisibleSpinner(EntityData data, Vector2 offset, EntityID id) : base(data, offset, CrystalColor.Purple)
        {}

        public override void Update()
        {
        }
    }
}

