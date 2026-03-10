
using Microsoft.Xna.Framework;

using Monocle;

using Celeste;
using Celeste.Mod.Entities;

namespace Celeste.Mod.ErrandOfWednesday
{

    [Tracked]
    [CustomEntity("eow/ThruWall")]
    public class ThruWall : Entity
    {


        public EntityID eid;
        
        public Vector2 left;
        public Vector2 right;

        public bool dash_bump;
        public bool pass_through;
        public bool clump;

        public bool active_right = false;
        public bool active_left = false;
        public bool player_was_left = false;
        public bool player_was_right = false;
        public bool inside_left = false;
        public bool inside_right = false;
        public bool inside_vert = false;

        public float jump_grace_timer = 0f;

        public static bool loaded = false;

        public static ThruWall last_used = null;
        public static ThruWall last_usable = null;

        public int _count = 0;

        public static void load()
        {
            if(loaded) return;
            On.Celeste.Player.WallJumpCheck += wjc_hook;
            On.Celeste.Player.WallJump += wj_hook;
            On.Celeste.Player.Jump += j_hook;
            loaded = true;
        }
        public static void unload()
        {
            if(!loaded) return;
            On.Celeste.Player.WallJumpCheck -= wjc_hook;
            On.Celeste.Player.WallJump -= wj_hook;
            On.Celeste.Player.Jump -= j_hook;
            loaded = false;
        }

        public static bool wjc_hook(On.Celeste.Player.orig_WallJumpCheck orig, Player self, int dir)
        {
            last_usable = null;
            if( orig(self, dir) )
            {
                return true;
            }

            if(self.Stamina <= 0f || self.jumpGraceTimer > 0f)
            {
                return false;
            }

            foreach (ThruWall entity in self.level.Tracker.GetEntities<ThruWall>())
            {
                if(entity.wall_jump_check(self, dir))
                {
                    last_usable = entity;
                    return true;
                }
            }
            return false;
        }

        public static void wj_hook(On.Celeste.Player.orig_WallJump orig, Player self, int dir)
        {
            if(last_usable != null)
            {
                if(last_usable == last_used)
                {
                    self.Stamina -= 27.5f;
                }
                last_used = last_usable;
            }
            else
            {
                last_used = null;
            }
            orig(self, dir);
        }

        public static void j_hook(On.Celeste.Player.orig_Jump orig, Player self, bool particles, bool playSfx)
        {
            last_used = null;
            orig(self, particles, playSfx);
        }

        public ThruWall(EntityData data, Vector2 offset, EntityID eid) : base(data.Position+ offset)
        {
            //TODO: line sprite
            //TODO: might be better to spawn a climb blocker to prevent clumps
            //TODO: need some kind of double jump grace timer that refunds a double jump if you buffer a jump?

            Position.X += 1;

            dash_bump = data.Bool("dash_bump", true);
            pass_through = data.Bool("pass_through", true);
            clump = data.Bool("clump_enable", false);

            Collider = new Hitbox(6f, data.Height, -3f, 0);

            left = new Vector2(Position.X, Position.Y);
            right = new Vector2(Position.X, Position.Y+data.Height);
        }

       
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            last_used = null;
            last_usable = null;
        }

        public override void Render()
        {
            if(last_used == this)
            {
                Draw.Line(left, right, Color.Black, 2f);
            }
            else
            {
                Draw.Line(left, right, Color.Black, 2f);
            }
        }

        public bool wall_jump_check(Player player, int dir)
        {
            if(!inside_vert)
            {
                return false;
            }
            bool dashing = (player.StateMachine.State == Player.StDash || player.StateMachine.State == Player.StRedDash) && player.Speed.X ==0;

            if(dir < 0) //wall to left
            {
                return (active_right || (dashing && inside_left)) && (clump || !(player.Facing == Facings.Left && Input.GrabCheck));
            }
            else //wall ro right
            {
                return (active_left || (dashing && inside_right)) && (clump || !(player.Facing == Facings.Right && Input.GrabCheck));;
            }

        }

        public override void Update()
        {
            base.Update();
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();

            if(player != null)
            {
                inside_left = player.Left >= Position.X -3f && player.Left <=Position.X +3f;
                inside_right = player.Right >= Position.X -3f && player.Right <=Position.X +3f;
                inside_vert = player.Bottom >= Position.Y && player.Top <=Position.Y+Height;


                if(pass_through)
                {
                    bool player_left = player.Left <= Position.X+2f;
                    bool player_right = player.Right >= Position.X-2f;

                    if(!player_left && player_was_left && inside_vert)
                    { //moving through right
                        active_right = true;
                        active_left = false;
                        jump_grace_timer = 0.06f;
                    }
                    else if(!player_right && player_was_right && inside_vert)
                    { //moving through left
                        active_right = false;
                        active_left = true;
                        jump_grace_timer = 0.06f;
                    }
                    else
                    {
                        if(active_right && (!inside_left /* || player.Speed.X < 0*/) && jump_grace_timer <= 0f)
                        {
                            active_right = false;
                        }
                        if(active_left && (!inside_right/* || player.Speed.X > 0*/) && jump_grace_timer <= 0f)
                        {
                            active_left = false;
                        }
                    }

                    player_was_left = player_left;
                    player_was_right = player_right;
                }

                jump_grace_timer -= Engine.DeltaTime;

                if(dash_bump && (player.StateMachine.State == Player.StDash || player.StateMachine.State == Player.StRedDash) && player.Speed.X ==0 && inside_vert)
                {
                    if(player.Left < Position.X && player.Left >= Position.X - 3f)
                    {
                        player.Left = Position.X;
                        active_right = true;
                        active_left = false;
                    }
                    else if(player.Right > Position.X && player.Right <= Position.X + 3f)
                    {
                        player.Right = Position.X;
                        active_left = true;
                        active_right = false;
                    }
    
                }
/*
_count += 1;
if(wall_jump_check(player, -1))
{
Logger.Log(LogLevel.Debug, "eow", $"l {eid} {_count}");
}
else if(wall_jump_check(player, 1))
{
Logger.Log(LogLevel.Debug, "eow", $"r {eid} {_count}");
}
else
{
_count = 0;
}
*/
//Logger.Log(LogLevel.Debug, "eow", $"l:{inside_left}/{active_right} r:{inside_right}/{active_left} v: {inside_vert} | {wall_jump_check(player, -1)} {wall_jump_check(player, 1)}");
 
            }
        }

    }

}

