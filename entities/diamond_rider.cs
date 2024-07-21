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
        //1 green
        //3 red
        //5 yellow
        //7 blue
        //9 cyan
        //others black
        //this is gonna be a huge pain in the ass isn't it?

        public static Color[] color_lookup = {
            Color.Black,
            Color.Green, Color.Black,
            Color.Red, Color.Black,
            Color.Yellow, Color.Black,
            Color.Blue, Color.Black,
            Color.Cyan
        };

        public int show_score;
        public float size;
        public Func<int> get_score;
        public Action render_func;

        public DiamondRiderScoreDisplay(EntityData data, Vector2 offset, EntityID id) : base(data.Position+ offset)
        {
            size = data.Float("size", 16)/32f;
            show_score = data.Int("score_to_show", 0);

            switch(show_score)
            { 
                case 0: render_func = _render_score; break;
                case 1: render_func = _render_hscore; break;
                case 2: render_func = _render_dscore; break;
                default: render_func = _render_score; break;
            }

        }

        public void _render_score()
        {
            int score = RideableDiamond.score;
            string text = $"{score}";
            Vector2 pos = Position;
            for(int i = 0; i < text.Length; ++i)
            {
                ActiveFont.Draw(text[i], 
                    pos,
                    new Vector2(0.5f, 0.5f), Vector2.One*size,
                    color_lookup[(char)(text[i])-0x30]
                    );
                Vector2 dims = ActiveFont.Measure(text[i]+"");
                pos.X += dims.X*size;
            }

           
        }

        public void _render_hscore()
        {
            int score =  RideableDiamond.high_score;
            string text = $"{score}";
            Vector2 pos = Position;
            for(int i = 0; i < text.Length; ++i)
            {
                ActiveFont.Draw(text[i], 
                    pos,
                    new Vector2(0.5f, 0.5f), Vector2.One*size,
                    color_lookup[(char)(text[i])-0x30]
                    );
                Vector2 dims = ActiveFont.Measure(text[i]+"");
                pos.X += dims.X*size;
            }


        }
 
        public void _render_dscore()
        {
            int score = Derek.score;
            string text = $"{score}";
            Vector2 pos = Position;
            for(int i = 0; i < text.Length; ++i)
            {
                ActiveFont.Draw(text[i], 
                    pos,
                    new Vector2(0.5f, 0.5f), Vector2.One*size,
                    Color.Black
                    );
                Vector2 dims = ActiveFont.Measure(text[i]+"");
                pos.X += dims.X*size;
            }


        }
        
        public override void Render()
        {
            render_func();

        }
 

    } 

    [Tracked]
    [CustomEntity("eow/Derek")]
    public class Derek : Entity
    {
        //They move back and forth changing speeds when they
        //contact the edge of the room
        //make it look like a kevin with a monocle
        //score resets to 1 on touching ground
        //score text not colored
        //24x24 but square
        //bumps player horizontally to nearest left/right edge

        public static Random rnd = new();

        public static int score = 1;

        public Sprite sprite;
        public float speed = -1;
        public float var_speed;

        //attrs
        public float max_speed;
        public float min_speed;
        public string sprite_path;

        public Derek(EntityData data, Vector2 offset, EntityID id) : base(data.Position+ offset)
        {
            //TODO if speed isn't set use random
            speed = data.Float("initial_speed", -1f);
            max_speed = data.Float("max_speed", 10f);
            min_speed = data.Float("min_speed", 0.1f);
            var_speed = max_speed-min_speed;

            sprite_path = data.Attr("sprite");

            sprite = new Sprite(GFX.Game, "");
            sprite.AddLoop("idle", sprite_path, 0.08f);
            sprite.CenterOrigin();
            sprite.Play("idle");
            Add(sprite);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            score = 1;
        }

        public float get_speed()
        {
            
            float result =  (float)rnd.NextDouble(); 
            result = result*(var_speed)+min_speed;
            return result;
        }

        public override void Update()
        {
            base.Update();

            Level level = SceneAs<Level>();

            Position.X += speed;
            if(Position.X <= level.Bounds.Left)
            {
                Position.X = level.Bounds.Left;
                speed = get_speed();
            }
            if(Position.X+24 >= level.Bounds.Right)
            {
                Position.X = level.Bounds.Right-24;
                speed = -get_speed();
            }



            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if(player != null)
            {
                if(player.LoseShards)
                {
                    score = 1;
                }
 
                Collider c = player.Collider;

                Vector2 base_offset = Position - player.Position;

                //This is why the diamonds launch you so high somtimes.
                if(!do_collide(player, base_offset - c.BottomCenter))
                {
                    do_collide(player, base_offset - c.TopCenter);
                }

            }

        }

        public bool do_collide(Player player, Vector2 offset, float extra = 0)
        {
            if(Math.Abs(offset.X) < 16f && Math.Abs(offset.Y) < 12f)
            {
                float dx = 16+8 - Math.Abs(offset.X);
                if(offset.X < 0) dx *= -1;
                player.Position.X += dx; 
                score += 1;
                return true;
            }
            return false;
        }
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
            //TODO implement score saving?

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
                if(player.LoseShards)
                {
                    score = 0;
                }
                Collider c = player.Collider;

                Vector2 base_offset = Position - player.Position;

                do_collide(player, base_offset - c.BottomLeft);
                do_collide(player, base_offset - c.BottomRight);
                do_collide(player, base_offset - c.TopLeft, c.Height);
                do_collide(player, base_offset - c.TopRight, c.Height);

            }
//Logger.Log(LogLevel.Info, "eow", $"{score}, {high_score}");
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
                player.StartJumpGraceTime();
            }

        }

    }



}

