
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

using Microsoft.Xna.Framework;

using Monocle;

using MonoMod.Utils;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Mono.Cecil.Cil;

using Celeste;
using Celeste.Mod.Entities;

namespace Celeste.Mod.ErrandOfWednesday
{

    public class EstateRoomInfo
    {
        public LevelData level_data;
        public EntityData room_data;

        public int start_x;
        public int start_y;

        public bool[] entries; //lrtb

        public string key;

        public int room_number;
        public string display_name;

        public int selection_count;

        public Sprite sprite;

        public EstateRoomInfo(LevelData level_data, EntityData room_data)
        {
            this.level_data = level_data;
            this.room_data = room_data;

            key = level_data.Name;

            selection_count = room_data.Int("selection_count",1);
            
            room_number = room_data.Int("number", -1);
            display_name = Dialog.Get(room_data.Attr("name", $"eow_estate_room_{key}"));

            start_x = level_data.Bounds.X;
            start_y = level_data.Bounds.Y;

            this.entries = new bool[4];

            string entries_string = room_data.Attr("entries");
            if(string.IsNullOrWhiteSpace(entries_string))
            {
                scan_tiles();
            }
            else
            {
                entries[0] = entries_string.Contains("l");
                entries[1] = entries_string.Contains("r");
                entries[2] = entries_string.Contains("t");
                entries[3] = entries_string.Contains("b");
            }
            //create sprite

            string sprite_name = room_data.Attr("sprite", "");
            if(string.IsNullOrWhiteSpace(sprite_name))
            {
                sprite_name = "eow/estate/";
                if( entries[0]) {sprite_name+="l";}
                if( entries[1]) {sprite_name+="r";}
                if (entries[2]) {sprite_name+="t";}
                if (entries[3]) {sprite_name+="d";}
            }
            if(GFX.SpriteBank.Has(sprite_name))
            {
                sprite = GFX.SpriteBank.Create(sprite_name);
            }
            else
            {
                sprite = new Sprite(GFX.Gui, "");
                sprite.AddLoop("idle", sprite_name, 0.08f);
                sprite.CenterOrigin();
            }
            sprite.Play("idle");


            //tags

            //selection expressions

        }

        public void scan_tiles()
        {
            int w = level_data.Bounds.Width/8;
            int h = level_data.Bounds.Height/8;
            int w2 = w/2;
            int h2 = h/2;

            Grid grid = new(1,1,level_data.Solids);

            entries[0] = !grid[w2,0];
            entries[2] = !grid[0,h2];
            entries[1] = !grid[w-1,h2];
            entries[3] = !grid[w2,h-1];

        }

    }

    [Tracked]
    [CustomEntity("eow/EstateRoom")]
    public class EstateRoom : Entity
    {

        public EstateRoomInfo room_info;    
        public EntityData my_data;

        public EstateRoom(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            my_data = data;
        }


        public override void Added(Scene scene)
        {
            base.Added(scene);
            LevelData this_level = (Scene as Level).Session.LevelData;
            room_info = EstateController.rooms[this_level.Name];
            EstateController.drafted_rooms.Add(this_level.Name);
        }

        public override void Update() 
        {
            base.Update();

        }


 
 
    }
}
