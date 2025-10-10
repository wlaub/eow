
using Microsoft.Xna.Framework;

using Monocle;

using Celeste;
using Celeste.Mod.Entities;

namespace Celeste.Mod.ErrandOfWednesday
{

    [Tracked]
    [CustomEntity("eow/RoomDashCountController")]
    public class RoomDashCountController : Entity
    {
        /* situation: there are 15 competing standards */

        public EntityID eid;
        public int count;
        public bool no_refills;

        public static int old_dashes = 1;
        public static bool old_no_refills;
        
        public RoomDashCountController(EntityData data, Vector2 offset, EntityID eid) : base(data.Position+ offset)
        {
            count = data.Int("count", 1);
            no_refills = data.Bool("no_refills", true);
        }

        
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Player player = (scene as Level).Tracker.GetEntity<Player>();

            PlayerInventory inventory = (scene as Level).Session.Inventory;
            if(player != null)
            {
                if (player.MaxDashes != count)
                {
                    old_dashes = player.MaxDashes;
                }
                if (inventory.NoRefills != no_refills)
                {
                    old_no_refills = inventory.NoRefills;
                }
     
                player.Dashes = count;
                SceneAs<Level>().Session.Inventory.Dashes = count;
                SceneAs<Level>().Session.Inventory.NoRefills = no_refills;
            }
        }

        public override void Removed(Scene scene)
        {

            Player player = (scene as Level).Tracker.GetEntity<Player>();

            if(player != null)
            { 
                player.Dashes = old_dashes;
                SceneAs<Level>().Session.Inventory.Dashes = old_dashes;
                SceneAs<Level>().Session.Inventory.NoRefills = old_no_refills;
            }
            base.Removed(scene);
        }


    }

}

