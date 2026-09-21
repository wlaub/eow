using System;

using Microsoft.Xna.Framework;

using Monocle;

using Celeste;
using Celeste.Mod.Entities;

namespace Celeste.Mod.ErrandOfWednesday
{

    [Tracked]
    [CustomEntity("eow/GlobalLookoutBounds")]
    public class GlobalLookoutBounds : Entity
    {
        public Rectangle bounds;
        public GlobalLookoutBounds(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            bounds = new(data.Int("left",0), data.Int("top",0), data.Int("width_",320), data.Int("height_",190));
        }
    }
}
