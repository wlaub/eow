using System;

using Microsoft.Xna.Framework;

using Monocle;

using Celeste;
using Celeste.Mod.Entities;

namespace Celeste.Mod.ErrandOfWednesday
{

    [Tracked]
    [CustomEntity("eow/EstateDoor")]
    public class EstateDoor : Solid
    {
        public Vector2 target;

        public MTexture[,] nineSlice;

        public int side;

        public EntityID eid;

        public EstateDoor(EntityData data, Vector2 offset, EntityID eid) : base(data.Position+offset, data.Width, data.Height, safe:true)
        {
            this.eid = eid;

            target = data.Nodes[0]+offset;

            //TODO create interaction thingy



            //TODO actual graphics

            string sprite_dir = data.Attr("spriteDirectory");

            MTexture mTexture = GFX.Game[sprite_dir + "block"];
            nineSlice = new MTexture[3, 3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    nineSlice[i, j] = mTexture.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                }
            }

        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Level level = scene as Level;

            LevelData this_level = level.Session.LevelData;            


            float dx = (Position.X - this_level.Bounds.Left)/(this_level.Bounds.Width-Width)-0.5f;
            float dy = (Position.Y - this_level.Bounds.Top)/(this_level.Bounds.Height-Height)-0.5f;


            if(Math.Abs(dx) > Math.Abs(dy))
            {
                if(dx > 0)
                { //on right side, enter left
                    side = 0;
                }
                else
                {
                    side = 1;
                }
            }
            else
            {
                if(dy > 0)
                { //on bottom side, enter top
                    side = 2;
                }
                else
                {
                    side = 3;
                }
 
            }
//Logger.Log(LogLevel.Info, "eow", $"  {eid}:{side}");   

        }

        public override void Update() 
        {
            base.Update();

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
