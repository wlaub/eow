
using System;

using Microsoft.Xna.Framework;

using Monocle;

using Celeste;
using Celeste.Mod.Entities;

using System.Text.RegularExpressions;
using System.IO;

namespace Celeste.Mod.ErrandOfWednesday
{

    [Tracked]
    [CustomEntity("eow/GlobalDecal")]
    public class GlobalDecal : Decal
    {
        public EntityID eid;

        public string flag;
        public bool flag_inverted;

        public bool actual = false;

        public GlobalDecal(EntityData data, Vector2 offset, EntityID eid) : base(
                data.Attr("sprite")+".png", 
                data.Position + offset, 
                new Vector2(data.Float("scaleX", 1f), data.Float("scaleY", 1f)),
                data.Int("depth", 9000),
                data.Float("rotation", 0),
                data.Attr("color", "ffffffff")
                )
        {

            this.eid = eid;

            flag = data.Attr("flag");
            if(!string.IsNullOrWhiteSpace(flag) && flag[0] == '!')
            {
                flag_inverted = true;
                flag = flag.Substring(1);
            }
            base.Tag = Tags.Global;

/*
        string texture = data.Attr("sprite");
Logger.Log(LogLevel.Debug, "eow", $"start: {texture}");
		if (string.IsNullOrEmpty(Path.GetExtension(texture)))
		{
			texture += ".png";
Logger.Log(LogLevel.Debug, "eow", $"add ext: {texture}");
		}
		string extension = Path.GetExtension(texture);
Logger.Log(LogLevel.Debug, "eow", $"ext: {extension}");
		string input = Path.Combine("decals", texture.Replace(extension, "")).Replace('\\', '/');
Logger.Log(LogLevel.Debug, "eow", $"input: {input}");
		Name = Regex.Replace(input, "\\d+$", string.Empty);
Logger.Log(LogLevel.Debug, "eow", $"name: {Name}");
*/



        }

        public static void try_load(Level level)
        {
Logger.Log(LogLevel.Debug, "eow", "Loading global decals.");
            foreach(Entity entity in MyLevelInspect.create_all_entity(level, "eow/GlobalDecal"))
            {
                ((GlobalDecal)entity).actual = true;
                level.Add(entity);
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if(!actual)
            {
                RemoveSelf();
            }
            (scene as Level).Session.DoNotLoad.Add(eid);
        }

        public override void Update()
        {
            base.Update();

            Visible = (string.IsNullOrWhiteSpace(flag) || SceneAs<Level>().Session.GetFlag(flag) != flag_inverted);
//  Logger.Log(LogLevel.Info, "eow", $"Decal {eid} is {Visible}"); 
        }

        public override void Render()
        {
            base.Render();
//Logger.Log(LogLevel.Info, "eow", $"Decal {eid} rendered"); 
        }

    }
}
