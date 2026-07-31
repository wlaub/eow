using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Monocle;

using Celeste;
using Celeste.Mod.Entities;

namespace Celeste.Mod.ErrandOfWednesday
{

    [Tracked]
    [CustomEntity("eow/LoreViewer")]
    public class LoreViewer : Entity
    {

        public string enable_flag;
        public bool enable_flag_inverted;

        public LoreViewer(EntityData data, Vector2 offset) : 
            base(data.Position+offset)
        {
        }
    }



    [Tracked]
    [CustomEntity("eow/LoreOreSpawner")]
    public class LoreOreSpawner : Entity
    {

        public string enable_flag;
        public bool enable_flag_inverted;

        public float rate;
        public float timer;

        public int max_lores;

        public int health;
        public string[] options;

        public LoreOreSpawner(EntityData data, Vector2 offset) : 
            base(data.Position+offset)
        {
            rate = data.Float("interval", 7f);
            timer = data.Float("start_offset", 0f);
            max_lores = data.Int("max_lores", 7);

            health = data.Int("lore_health", 0);
            options = data.Attr("lore_options", "").Split(',');

            //TODO enable flag

        }

        public override void Update()
        {
            Level level = SceneAs<Level>();

            //TODO enable flag

            timer += Engine.DeltaTime;
            if(timer >= rate)
            {
                timer -= rate;
                if(max_lores < 0 || level.Tracker.CountEntities<LoreOre>() < max_lores)
                {
                    //TODO select random option
                    level.Add(new LoreOre(Position, health, health, options[0]));
                }

            }

        }

    }


    [Tracked]
    [CustomEntity("eow/LoreOre")]
    public class LoreOre : TheoCrystal
    {

        public int max_health;
        public int health;
        public string contents;

        public Vector2 lorientation; //front side, left side
        /* front: left, bottom, right, top
        F:aF[]
        1:2,3,5,4
        2:6,3,1,4
        3:5,1,2,6
        4:1,5,6,2
        5:1,3,6,4
        6:5,3,2,4

        1,2 -> 4,2 -> 6,2 -> 3,2        
        is 180 rotation of
        1,5 -> 1,3 -> 1,6 -> 1,4
       
        F=1, L=2, a2=4,6,3,1
        iL(F) = index of F in aL i.e. inverse of aL[F]
        F=F, L=L
        ->
        F=aL[iL(F)+1]=aF[iF(L)-1]
        ->
        F=aL[iL(F)+2] = F-1
        ->
        F=aL[iL(F)+3]=aF[iF(L)+1]
        ->
        F=aL[iL(F)+4] = aL[iL(F)] = F
        */

        public LoreOre(EntityData data, Vector2 offset) : 
            this(data.Position+offset, 
                data.Int("health", 7),
                data.Int("max_health", 7),
                data.Attr("contents", "")
                )
        {}

        public LoreOre(Vector2 position, int health, int max_health, string contents) : base(position)
        {
            this.health=health;
            this.max_health = max_health;
            this.contents=contents;

            Hold.SlowFall = false;
            Hold.SlowRun = true;
            onCollideH = OnCollideH;
            onCollideV = OnCollideV;

            base.Collider = new Hitbox(7f, 7f, -3.5f, -3.5f);
            Hold.PickupCollider = new Hitbox(14f, 14f, -7f, -7f);

            Tag -= Tags.TransitionUpdate;

//            Remove(sprite);
//            Add(sprite = GFX.SpriteBank.Create("lore_ore"));
        }

        public override void Update()
        {
            base.Update();
            if(!Hold.IsHeld && Bottom <= Level.Bounds.Top+8)
            {
                RemoveSelf();
            }
        }

        new public void OnCollideH(CollisionData data)
        {
            //140-182
            if(Math.Abs(Speed.X) > 140f +42*health/max_health)
            {
                health-= 1;
                if(health<0)
                {
                    //break
                    base.Die();
                }
                else
                {
                    //crack
                    crack();
                }
                Speed.X *= -0.7f;
            }
            else
            {
                Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_side", Position);
                Speed.X *= -0.4f;
            }
        }

        public void crack()
        {
            Audio.Play("event:/game/09_core/iceball_break", Position);
        }

        new public void OnCollideV(CollisionData data)
        {
            //141 should be safe up to health = 0
            //200 should be unsafe at max health
            if(Speed.Y >= 133+66*health/max_health)
            {
                health-= 1;
                if(health<0)
                {
                    //break
                    base.Die();
                }
                else
                {
                    //crack
                    crack();
                }

                Speed.Y *= -0.7f;
            }
            else
            {
                Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_side", Position);
                if(Speed.Y > 100f)
                {
                    Speed.Y *= -0.4f;
                }
                else
                {
                    Speed.Y = 0;
                }
            }
        }

    }
}
