using System;
using System.Collections;

using Microsoft.Xna.Framework;

using Monocle;

using Celeste;
using Celeste.Mod.Entities;

namespace Celeste.Mod.ErrandOfWednesday
{

    [CustomEntity("eow/DiamondRiderScoreDisplay")]
    public class DiamondRiderScoreDisplay : Entity
    {
    } 

    [Tracked]
    [CustomEntity("eow/MonocleCreature")]
    public class MonocleCreature : Entity
    {
    } 

    [Tracked]
    [CustomEntity("eow/RideableDiamond")]
    public class RideableDiamond : Entity
    {
        public static int score = 0;
        public static int high_score = 0;

        public Sprite sprite;
        public Vector2 center;
        public float phase = 0;

        //attrs
        public float radius;
        public float speed;
        public string sprite_path;

        public RideableDiamond(EntityData data, Vector2 offset, EntityID id) : base(data.Position+ offset)
        {
            //TODO reset score if player on ground
            //TODO make it so you can jump off diamond but don't restore dash

            radius = data.Float("radius");
            speed = data.Float("speed");
            sprite_path = data.Attr("sprite");

            center = Position - new Vector2(radius, 0);

            sprite = new Sprite(GFX.Game, "");
            sprite.AddLoop("idle", sprite_path, 0.08f);
            sprite.CenterOrigin();
            sprite.Play("idle");
            Add(sprite);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            score = 0;
        }

        public override void Update()
        {
            base.Update();
            phase += speed * Engine.DeltaTime;
            if(phase > 2*Math.PI)
            {
                phase -= 2*(float)Math.PI;
            }
            Position.X = center.X + radius*(float)Math.Cos(phase);
            Position.Y = center.Y + radius*(float)Math.Sin(phase);

            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if(player != null)
            {
                Collider c = player.Collider;

                Vector2 base_offset = Position - player.Position;

                do_collide(player, base_offset - c.BottomLeft);
                do_collide(player, base_offset - c.BottomRight);
                do_collide(player, base_offset - c.TopLeft, c.Height);
                do_collide(player, base_offset - c.TopRight, c.Height);

            }

        }

        public void do_collide(Player player, Vector2 offset, float extra = 0)
        {
            if(Math.Abs(offset.X) + Math.Abs(offset.Y) < 12f)
            {
                float dy = offset.Y+1 - Math.Abs(offset.X);
                player.Position.Y -= dy+8f+extra;
                score += 1;
                if(score > high_score)
                {
                    high_score += 1;
                }
                player.RefillDash();
            }

        }

    }



}

