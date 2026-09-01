using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Monocle;

using Celeste;
using Celeste.Mod.Entities;

namespace Celeste.Mod.ErrandOfWednesday
{

    [Tracked]
    [CustomEntity("eow/EstateMap")]
    public class EstateMap : Entity
    {
        public EntityID eid;

        public TalkComponent door_handle;


        public int[,,] counts;
        public int[,,] depths;


        float w,h, offx, offy;


        public EstateMap(EntityData data, Vector2 offset, EntityID eid) : base(data.Position+offset)
        {
            this.eid = eid;

            base.Tag = Tags.HUD | Tags.FrozenUpdate;

            Add(door_handle = new TalkComponent(
                new Rectangle((int)Position.X,(int)Position.Y, 16, 8),
                Position, 
                open_door
                ));

            Visible = false; 


            EstateGrid grid = EstateController.grid;

            counts = new int[grid.grid_width, grid.grid_height,4];
            depths = new int[grid.grid_width, grid.grid_height,4];


//            float w,h,offx,offy;

            foreach (string room_key in grid.room_position.Keys)
            {
                Vector2 pos = grid.room_position[room_key];
                EstateRoomInfo room = EstateController.rooms[room_key];





                Position = Vector2.Zero;

                w = room.sprite.Width/2;
                h = room.sprite.Height/2;
                offx = 1920f-(w*grid.grid_width)-(1920-w*grid.grid_width)/2;
                offy = 49f;

                room.sprite.Scale = new Vector2(0.5f,0.5f);
                room.sprite.Position.X = offx+(pos.X+0.5f)*w;
                room.sprite.Position.Y = offy+(pos.Y+0.5f)*h;
                Add(room.sprite);
            }



        }

        public void open_door(Player player)
        {
            Visible = !Visible;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Level level = scene as Level;

            LevelData this_level = level.Session.LevelData;            

            EstateGrid grid = EstateController.grid;


            EstateController.drafting_context = new();
            var drafting_context = EstateController.drafting_context;


            List<EstateRoomInfo> pool;

            for(int gx = 0; gx < grid.grid_width; ++gx)
            {
            for(int gy=0;gy<grid.grid_height;++gy)
            {




    //Logger.Log(LogLevel.Info, "eow", $"drafting into {gx},{gy}");



                int tx, ty, side;
                drafting_context.from_level = this_level;
                if(gx > 0)
                {//test draft left
                    tx = gx-1;
                    ty=gy;
                    side = 1;
                    drafting_context.pool_depth = 0;
                    drafting_context.into_x = tx;
                    drafting_context.into_y = ty;
                    drafting_context.into_top = ty==0;
                    drafting_context.into_bot = ty==grid.grid_height-1;
                    drafting_context.into_left = tx==0;
                    drafting_context.into_right = tx==grid.grid_width-1;
                    drafting_context.side = side;
                    pool = EstateController.make_pool(side, level.Session, tx,ty);
                    counts[gx,gy, side] = pool.Count;
                    depths[gx,gy,side]=drafting_context.pool_depth;
//    if(pool.Count == 0)
//        Logger.Log(LogLevel.Info, "eow", $"from {gx},{gy} left yields nothing");


                }
                if(gx < grid.grid_width-1)
                {//test draft right
                    tx = gx+1;
                    ty=gy;
                    side = 0;
                    drafting_context.pool_depth = 0;
                    drafting_context.into_x = tx;
                    drafting_context.into_y = ty;
                    drafting_context.into_top = ty==0;
                    drafting_context.into_bot = ty==grid.grid_height-1;
                    drafting_context.into_left = tx==0;
                    drafting_context.into_right = tx==grid.grid_width-1;
                    drafting_context.side = side;
                    pool = EstateController.make_pool(side, level.Session, tx,ty);
                    counts[gx,gy, side] = pool.Count;
                    depths[gx,gy,side]=drafting_context.pool_depth;
//    if(pool.Count > 0)
//        Logger.Log(LogLevel.Info, "eow", $"from {gx},{gy} right");


                }
                if(gy > 0)
                {//test draft up
                    tx = gx;
                    ty=gy-1;
                    side = 3;
                    drafting_context.pool_depth = 0;
                    drafting_context.into_x = tx;
                    drafting_context.into_y = ty;
                    drafting_context.into_top = ty==0;
                    drafting_context.into_bot = ty==grid.grid_height-1;
                    drafting_context.into_left = tx==0;
                    drafting_context.into_right = tx==grid.grid_width-1;
                    drafting_context.side = side;
                    pool = EstateController.make_pool(side, level.Session, tx,ty);
                    counts[gx,gy, side] = pool.Count;
                    depths[gx,gy,side]=drafting_context.pool_depth;
                }
                if(gy < grid.grid_height-1)
                {//test draft down
                    tx = gx;
                    ty=gy+1;
                    side = 2;
                    drafting_context.pool_depth = 0;
                    drafting_context.into_x = tx;
                    drafting_context.into_y = ty;
                    drafting_context.into_top = ty==0;
                    drafting_context.into_bot = ty==grid.grid_height-1;
                    drafting_context.into_left = tx==0;
                    drafting_context.into_right = tx==grid.grid_width-1;
                    drafting_context.side = side;
                    pool = EstateController.make_pool(side, level.Session, tx,ty);
                    counts[gx,gy, side] = pool.Count;
                    depths[gx,gy,side]=drafting_context.pool_depth;
                }

            }
            }



        }

        public override void Update() 
        {
            base.Update();

        }

        public override void Render()
        {
            base.Render();
            EstateGrid grid = EstateController.grid;


            for(int gx = 0; gx < grid.grid_width; ++gx)
            {
            for(int gy=0;gy<grid.grid_height;++gy)
            {

                float xc = offx+(gx+0.5f)*w;
                float yc = offy+(gy+0.5f)*h;

                string room_name = grid.room_at_grid(gx,gy);
                if(room_name != null)
                {
                ActiveFont.Draw(room_name, 
                    new Vector2(xc, yc),
                    new Vector2(0.5f, 0.5f), Vector2.One*0.25f,
                    Color.Blue
                    );
 
                }


                Color[] lookup = {Color.Magenta, Color.Black,Color.Lime,Color.Red,Color.Magenta};

                if(gx > 0)
                {
                ActiveFont.Draw($"{counts[gx,gy,1]}", 
                    new Vector2(xc-w*.4f, yc),
                    new Vector2(0f, 0.5f), Vector2.One*0.5f,
                    lookup[Math.Min(depths[gx,gy,1], lookup.Length-1)]
//                    Color.Black
                    );
                }
                if(gx < grid.grid_width-1)
                {
                 ActiveFont.Draw($"{counts[gx,gy,0]}", 
                    new Vector2(xc+w*.4f, yc),
                    new Vector2(1f, 0.5f), Vector2.One*0.5f,
//                    Color.Black
                    lookup[Math.Min(depths[gx,gy,0], lookup.Length-1)]
                    );
                }
                if(gy > 0)
                {
                  ActiveFont.Draw($"{counts[gx,gy,3]}", 
                    new Vector2(xc, yc-0.4f*h),
                    new Vector2(0.5f, 0f), Vector2.One*0.5f,
                    lookup[Math.Min(depths[gx,gy,3], lookup.Length-1)]
//                    Color.Black
                    );
                 }
                if(gy < grid.grid_height-1)
                {
                   ActiveFont.Draw($"{counts[gx,gy,2]}", 
                    new Vector2(xc, yc+0.4f*h),
                    new Vector2(0.5f, 1f), Vector2.One*0.5f,
                    lookup[Math.Min(depths[gx,gy,2], lookup.Length-1)]
//                    Color.Black
                    );
                }
            }
            }

        }

    }
}
