
using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Monocle;

using Celeste;
using Celeste.Mod.Entities;

namespace Celeste.Mod.ErrandOfWednesday
{

    [Tracked]
    [CustomEntity("eow/FlagInitializer")]
    public class FlagInitializer : Entity
    {

        public FlagInitializer(EntityData data, Vector2 offset) : base(data.Position + offset)
        {


        }

    }
}
