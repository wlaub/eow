using System;

using Microsoft.Xna.Framework;

using Monocle;

using Celeste;
using Celeste.Mod.Entities;

namespace Celeste.Mod.ErrandOfWednesday
{

    [Tracked]
    [CustomEntity("eow/LiftboostBlock")]
    public class LiftboostBlock : Solid
    {
        public Vector2 target;
        public Vector2 boost;      
        public MTexture[,] nineSlice;
        public Sprite arrow;

        public float reset_timer = 0;
        public float boost_start_timer = 0;
        public float boost_stop_timer = 0;
 
        public LiftboostBlock(EntityData data, Vector2 offset) : base(data.Position+offset, data.Width, data.Height, safe:false)
        {
            target = data.Nodes[0]+offset;
            boost = (target-Position)*10f;

            string sprite_dir = data.Attr("spriteDirectory");

            float angle = (float)Math.Atan2(boost.Y, boost.X);
            
            Add(arrow = new Sprite(GFX.Game, sprite_dir));
            arrow.Add("idle", "arrow_idle", 0.1f, "idle");
            arrow.Add("active", "arrow_active", 0.1f, "active");
            arrow.Add("cooldown", "arrow_cooldown", 0.1f, "cooldown");
            arrow.Position = (new Vector2(Width / 2f, Height / 2f));
            arrow.Rotation = angle;
    		arrow.CenterOrigin();

            arrow.Play("idle");

            MTexture mTexture = GFX.Game[sprite_dir + "block"];
            nineSlice = new MTexture[3, 3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    nineSlice[i, j] = mTexture.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                }
            }

            Add(new DashListener
            {
                OnDash = on_dash
            });
                    
        }

        public void on_dash(Vector2 direction)
        {
            if(reset_timer > 0f) return;
            reset_timer = 0.8f;
            boost_start_timer = 0f;
            boost_stop_timer = 0.05f;
        }

        public override void Update() 
        {
            base.Update();

            if(reset_timer > 0f)
            {
                reset_timer -= Engine.DeltaTime;
                boost_start_timer -= Engine.DeltaTime;
                boost_stop_timer -= Engine.DeltaTime;
                if (reset_timer <= 0f)
                {
                    arrow.Play("idle");
                    return;
                }

            }


            if(boost_start_timer > 0)
            {
                arrow.Play("idle");
            }
            else if(boost_stop_timer >0)           
            {
                arrow.Play("active");
                Player player = GetPlayerRider();
                if(player != null)
                {
Logger.Log(LogLevel.Info, "eow", "liftboost: "+boost);
                    player.LiftSpeed = boost;
                }
                else
                {
                    player = GetPlayerOnSide();
                    if(player != null)
                    {
Logger.Log(LogLevel.Info, "eow", "liftboost: "+boost);
                    player.LiftSpeed = boost;
 
                    }
                }
            }
            else if(reset_timer > 0)
            {
                arrow.Play("cooldown");
            }
        }

	public Player GetPlayerOnSide()
	{
		foreach (Player entity in base.Scene.Tracker.GetEntities<Player>())
		{
            if (entity.Facing == Facings.Left && CollideCheck(entity, Position + Vector2.UnitX))
            {
                return entity;
            }
            if (entity.Facing == Facings.Right && CollideCheck(entity, Position - Vector2.UnitX))
            {
                return entity;
            }
		}
		return null;
	}

	public override void Render()
	{
		float num = base.Collider.Width / 8f - 1f;
		float num2 = base.Collider.Height / 8f - 1f;
		for (int i = 0; (float)i <= num; i++)
		{
			for (int j = 0; (float)j <= num2; j++)
			{
				int num3 = (((float)i < num) ? Math.Min(i, 1) : 2);
				int num4 = (((float)j < num2) ? Math.Min(j, 1) : 2);
				nineSlice[num3, num4].Draw(Position + base.Shake + new Vector2(i * 8, j * 8));
			}
		}
		base.Render();
	}

    }
}
