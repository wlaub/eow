using System;
using System.Collections;

using Microsoft.Xna.Framework;

using Monocle;

using Celeste;
using Celeste.Mod.Entities;

namespace Celeste.Mod.ErrandOfWednesday
{

    [Tracked]
    [TrackedAs(typeof(DreamBlock))]
    [CustomEntity("eow/VergeBlock")]
    public class VergeBlock : DreamBlock
    {
        public static bool loaded = false;

        public Vector2[] targets;
        public bool activated = false;

        public float fall_threshold = 200f;


        public VergeBlock(EntityData data, Vector2 offset, EntityID id) : base(data, offset)
        {

            fall_threshold = data.Float("fall_threshold");

            targets = new Vector2[data.Nodes.Length];
            for(int i = 0; i < targets.Length; ++i)
            {
                targets[i] = data.Nodes[i]+offset;
            }

            base.node = null;

        }

        public static void Load()
        {
            if(loaded) return;

            On.Celeste.Player.OnCollideV += on_collide_v;

            loaded = true;
        } 
        public static void Unload()
        {
            if(!loaded) return;

            On.Celeste.Player.OnCollideV -= on_collide_v;

            loaded = false;
        }

        public static void on_collide_v(On.Celeste.Player.orig_OnCollideV orig, Player player, CollisionData data)
        {
            if(player.StateMachine.State == Player.StNormal)
            {
                Vector2 dir = Vector2.UnitY * Math.Sign(player.Speed.Y);
                if(data.Hit != null && data.Hit is VergeBlock)
                {
//Logger.Log(LogLevel.Info, "eow", $"hit, {data.Hit.GetType().Name}");
                    VergeBlock block = (VergeBlock)data.Hit;
//Logger.Log(LogLevel.Info, "eow", $"{player.Speed.Y} {dir}");
                    if(block != null && Math.Abs(player.Speed.Y) > block.fall_threshold)
                    {
                        player.StateMachine.State=Player.StDreamDash;
                        player.dreamBlock = block;
                        player.Speed = dir*240f;
                        player.Position += dir;
                        player.dashAttackTimer = 0f;
                        block.activate(player);
                    }
                }
            }
        
            orig(player, data);
        }

        public void activate(Player player)
        {
            if(activated) return;

            foreach(Trigger trigger in Scene.Tracker.GetEntities<Trigger>())
            {
                for(int i = 0; i < targets.Length; ++i)
                {
                    Vector2 target = targets[i];
                    if(trigger.CollidePoint(target))
                    {
                        trigger.OnEnter(player);
                        trigger.OnStay(player);
                    }
                }
            }
//            activated = true;
        }

        public override void Update()
        {
            base.Update();
        }
    }
}

