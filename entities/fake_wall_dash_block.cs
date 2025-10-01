using System.Collections;

using Microsoft.Xna.Framework;

using Monocle;

using Celeste;
using Celeste.Mod.Entities;

namespace Celeste.Mod.ErrandOfWednesday
{

    /*
    one way to do this is to just make a dashblock that tracks as a fake wall
    but apparently the facists say you can't do that because it could crash the
    game if something finds a dash block when looking for fake walls and tries to
    do something you can only do with a real fake wall so that's why i'm doing it
    this way even though it's way more complicated and probably also not the best
    way to do it whatever i don't c# and i don't know what i'm doing
    */

    [TrackedAs(typeof(FakeWall))]
    public class MyFirstInvisibleFakeWall : FakeWall
    {

        public MyFirstInvisibleFakeWall(EntityID eid, Vector2 position, char tile, float width, float height, Modes mode) : base(eid, position, tile, width, height, mode)
        {
            Visible = false;
        }
    }
   

    [Tracked]
    [TrackedAs(typeof(DashBlock))]
    [CustomEntity("eow/FakeWallDashBlock")]
    public class FakeWallDashBlock : DashBlock
    {
        public MyFirstInvisibleFakeWall fake_wall;        

        public FakeWallDashBlock(EntityData data, Vector2 offset, EntityID id) : base(data, offset, id)
        {
            fake_wall = new MyFirstInvisibleFakeWall(id, Position, tileType, width, height, FakeWall.Modes.Block);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(fake_wall);
        }
       
        public override void Removed(Scene scene) 
        {
            base.Removed(scene);
            fake_wall.RemoveSelf();
        }

    }
}

