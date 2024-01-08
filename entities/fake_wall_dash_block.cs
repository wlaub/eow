using System.Collections;

using Microsoft.Xna.Framework;

using Monocle;

using Celeste;
using Celeste.Mod.Entities;

namespace Celeste.Mod.ErrandOfWednesday
{

    [Tracked]
    [TrackedAs(typeof(FakeWall))]
    [CustomEntity("eow/FakeWallDashBlock")]
    public class FakeWallDashBlock : DashBlock
    {

        public FakeWallDashBlock(EntityData data, Vector2 offset, EntityID id) : base(data, offset, id)
        {}
    }
}

