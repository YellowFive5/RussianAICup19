using System.Dynamic;
using AiCup2019.Model;

namespace AiCup2019
{
    public static class Action
    {
        private static Game Game;
        private static MyUnit Me;
        private static World Around;

        private static void Set(Game game, MyUnit me, World around)
        {
            Game = game;
            Me = me;
            Around = around;
        }

        public static void TakeNearestWeapon(Game game, MyUnit me, World around)
        {
            Set(game, me, around);

            var action = new UnitAction
                         {
                             Velocity = VelocityLR(Constants.MaxVelocity),
                             Jump = SetJump(),
                             JumpDown = SetJumpDown(),
                             Aim = SetAim(Around.NearestEnemy.Position),
                             Shoot = SetShootMode(),
                             SwapWeapon = SetSwapWeapon(),
                             PlantMine = SetPlantMine(),
                             Reload = SetReload()
                         };

            me.NextAction = new CustomAction(nameof(TakeNearestWeapon), action);
        }

        private static bool SetReload()
        {
            return Me.Reload;
        }

        private static bool SetPlantMine()
        {
            return Me.PlantMine;
        }

        private static bool SetSwapWeapon()
        {
            return Me.SwapWeapon;
        }

        private static bool SetShootMode()
        {
            return Me.Shoot;
        }

        private static double VelocityLR(double velocity)
        {
            if (Me.Position.X < Me.Target.X)
            {
                return velocity;
            }

            return velocity * -1;
        }

        private static void SetTarget(Vec2Double target)
        {
            Me.Target = target;
        }

        private static Vec2Double SetAim(Vec2Double target)
        {
            Me.Aim = new Vec2Double(Around.NearestEnemy.Position.X - Me.Position.X, Around.NearestEnemy.Position.Y - Me.Position.Y);
            // Aim = target;
            return Me.Aim;
        }

        private static bool SetJump()
        {
            if (Me.OnLadder
                || Around.NextTileR == Tile.Wall
                || Around.NextTileL == Tile.Wall)
            {
                Me.Jump = true;
            }
            else
            {
                Me.Jump = false;
            }

            return Me.Jump;
        }

        private static bool SetJumpDown()
        {
            return Me.JumpDown;
        }
    }
}