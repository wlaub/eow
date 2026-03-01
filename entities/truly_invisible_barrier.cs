using System;

using Microsoft.Xna.Framework;

using Monocle;

using Celeste;
using Celeste.Mod.Entities;

namespace Celeste.Mod.ErrandOfWednesday
{

    [Tracked]
    [CustomEntity("eow/TrulyInvisibleBarrier")]
    public class TrulyInvisibleBarrier : InvisibleBarrier
    {
        public TrulyInvisibleBarrier(EntityData data, Vector2 offset) : base(data, offset)
        {
            base.Collider = new InvisibleHitbox(Width, Height);
        }
    }
}
