#region Usings

using System;
using AiCup2019.Model;

#endregion

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

        public static void TakeBestWeapon(Game game, MyUnit me, World around)
        {
            Set(game, me, around);

            switch (Around.BestWeapon)
            {
                case WeaponType.Pistol:
                    SetTarget(Around.NearestPistol.Position);
                    break;
                case WeaponType.AssaultRifle:
                    SetTarget(Around.NearestRifle.Position);
                    break;
                case WeaponType.RocketLauncher:
                    SetTarget(Around.NearestRLauncher.Position);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var action = new UnitAction
                         {
                             Velocity = VelocityLR(Constants.MaxVelocity),
                             Jump = SetJump(),
                             JumpDown = SetJumpDown(),
                             Aim = SetAim(Around.NearestEnemy.Position),
                             Shoot = SetShootMode(),
                             SwapWeapon = SetSwapWeapon(true),
                             PlantMine = SetPlantMine(false),
                             Reload = SetReload()
                         };
            me.NextAction = new CustomAction(nameof(TakeBestWeapon), action);
        }

        public static void GoHeel(Game game, MyUnit me, World around)
        {
            Set(game, me, around);

            SetTarget(Around.NearestHealth.Position);
            var action = new UnitAction
                         {
                             Velocity = VelocityLR(Constants.MaxVelocity),
                             Jump = SetJump(),
                             JumpDown = SetJumpDown(),
                             Aim = SetAim(Around.NearestEnemy.Position),
                             Shoot = SetShootMode(),
                             SwapWeapon = SetSwapWeapon(false),
                             PlantMine = SetPlantMine(false),
                             Reload = SetReload()
                         };
            me.NextAction = new CustomAction(nameof(GoHeel), action);
        }

        public static void ShootEm(Game game, MyUnit me, World around)
        {
            Set(game, me, around);

            SetTarget(Around.NearestEnemy.Position);

            if (Me.RLEquiped)
            {
                Me.Target = Me.Position.X > Me.Target.X
                                ? new Vec2Double(Me.Target.X + Constants.SafeArea > Constants.MaxXArrayTile
                                                     ? Constants.MaxXArrayTile
                                                     : Me.Target.X + Constants.SafeArea,
                                                 Me.Target.Y)
                                : new Vec2Double(Me.Target.X - Constants.SafeArea < 0
                                                     ? 0
                                                     : Me.Target.X - Constants.SafeArea,
                                                 Me.Target.Y);
                SetTarget(Me.Target);
            }


            var action = new UnitAction
                         {
                             Velocity = VelocityLR(Constants.MaxVelocity),
                             Jump = SetJump(),
                             JumpDown = SetJumpDown(),
                             Aim = SetAim(Around.NearestEnemy.Position),
                             Shoot = SetShootMode(),
                             SwapWeapon = SetSwapWeapon(false),
                             PlantMine = SetPlantMine(false),
                             Reload = SetReload()
                         };
            me.NextAction = new CustomAction(nameof(ShootEm), action);
        }

        private static bool SetReload()
        {
            var reload = Me.Weapon.HasValue && Me.Weapon.Value.Magazine == 0;
            Me.Reload = reload;
            return Me.Reload;
        }

        private static bool SetPlantMine(bool plant)
        {
            Me.PlantMine = plant;
            return Me.PlantMine;
        }

        private static bool SetSwapWeapon(bool swap)
        {
            Me.SwapWeapon = swap;
            return Me.SwapWeapon;
        }

        private static bool SetShootMode(bool? shoot = null)
        {
            if (shoot != null)
            {
                Me.Shoot = shoot.Value;
            }
            else
            {
                return Measure.IsStraightVisible(Me, Around.NearestEnemy, Game);
            }

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
            return Me.Aim;
        }

        private static bool SetJump()
        {
            if ((int) Me.Target.Y >= (int) Me.Position.Y && 
                ((int)Me.Target.X != (int)Me.Position.X 
                && Me.OnLadder ||
                Around.NextTileR == Tile.Wall || Around.NextTileL == Tile.Wall ||
                Me.UnderPlatform && Me.OnGround))
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
            if ((int) Me.Target.Y < (int) Me.Position.Y && !Me.Jump)
            {
                Me.JumpDown = true;
            }
            else
            {
                Me.JumpDown = false;
            }

            return Me.JumpDown;
        }
    }
}