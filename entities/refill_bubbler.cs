using System.Collections;

using Microsoft.Xna.Framework;

using Monocle;

using Celeste;
using Celeste.Mod.Entities;

namespace Celeste.Mod.ErrandOfWednesday
{

    [Tracked]
    [CustomEntity("eow/RefillBubbler")]
    public class RefillBubbler : Trigger
    {

        public EntityID eid;

        public string enable_flag;
        public bool enable_flag_inverted;
        public string use_flag;
        public bool use_flag_inverted;

        public bool only_once;

        public Vector2[] nodes;

        public RefillBubbler(EntityData data, Vector2 offset, EntityID id) : base(data, offset)
        {
            eid = id;
            enable_flag = data.Attr("enable_flag", "");
            if(!string.IsNullOrWhiteSpace(enable_flag) && enable_flag[0] == '!')
            {
                enable_flag_inverted = true;
                enable_flag = enable_flag.Substring(1);
            }
            use_flag = data.Attr("use_flag", "");
            if(!string.IsNullOrWhiteSpace(use_flag) && use_flag[0] == '!')
            {
                use_flag_inverted = true;
                use_flag = use_flag.Substring(1);
            }

            only_once = data.Bool("only_once", false);

            nodes = data.NodesOffset(offset+new Vector2(Width/2, Height/2));

        }

        public void activate(Player player)
        {
            Level level = SceneAs<Level>();
            if(string.IsNullOrWhiteSpace(enable_flag) || level.Session.GetFlag(enable_flag) != enable_flag_inverted)
            {
                if(!string.IsNullOrWhiteSpace(use_flag))
                {
                    level.Session.SetFlag(use_flag, !use_flag_inverted);
                }
                if(only_once)
                {
                    level.Session.DoNotLoad.Add(eid);
                }
                Add(new Coroutine(node_poutine(player)));
            }
        }

        public IEnumerator node_poutine(Player player) {
            if (!player.Dead) {
                Audio.Play("event:/game/general/cassette_bubblereturn", SceneAs<Level>().Camera.Position + new Vector2(160f, 90f));
                player.StartCassetteFly(nodes[1], nodes[0]);
            }

            yield break;
        }

        public static bool loaded = false;
        public static void try_load()
        {
            if(loaded){return;}
            On.Celeste.Player.UseRefill += use_refill;
            loaded = true;
        }
        public static void unload()
        {
            if(!loaded){return;}
            On.Celeste.Player.UseRefill -= use_refill;
            loaded = false;
        }


        public static bool use_refill(On.Celeste.Player.orig_UseRefill orig, Player self, bool two_dashes)
        {
            bool result = orig(self, two_dashes);
            if(result)
            {
                Level level = self.SceneAs<Level>();
                foreach(RefillBubbler trigger in level.Tracker.GetEntities<RefillBubbler>())
                {
                    if( trigger.PlayerIsInside )
                    {
                        trigger.activate(self);
                    }
                }
            }
            return result;
        }

    }
}

