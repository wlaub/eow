
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

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

    public class AxisParallax : Component
    {

        public static void load()
        {
            DecalRegistry.AddPropertyHandler("axis_parallax", delegate(Decal decal, XmlAttributeCollection attrs) {
                float x = 0;
                float y = 0;
                if(attrs["x"] != null)
                    x = float.Parse(attrs["x"].Value);
                if(attrs["y"] != null)
                    y = float.Parse(attrs["y"].Value);


                AxisParallax comp = new(x,y);
                decal.Add(comp);
                decal.Tag |= Tags.TransitionUpdate;

                });
        }

        public float x_amount;
        public float y_amount;
        public Vector2 base_position;

        public AxisParallax(float x_amount, float y_amount): base(active:true, visible:true)
        {
            this.x_amount = x_amount;
            this.y_amount = y_amount;
        }

        public override void EntityAwake()
        {
            base_position = base.Entity.Position;
        }

        public override void Update()
        {
            base.Update();
            Decal decal = (Decal)base.Entity;
            Vector2 delta = base_position - (SceneAs<Level>().Camera.Position + new Vector2(160f, 90f));
            delta.X *= x_amount;
            delta.Y *= y_amount; 
            decal.Position = base_position + delta;
        }


    } 




}
