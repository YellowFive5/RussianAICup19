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

        #region Actions

        public static void TakeBestWeapon(Game game, MyUnit me, World around)
        {
            Set(game, me, around);
            me.NextAction = new CustomAction(nameof(TakeBestWeapon));

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

            me.NextAction.Action = new UnitAction
                                   {
                                       Velocity = VelocityLR(Constants.MaxVelocity),
                                       Jump = SetJump(),
                                       JumpDown = SetJumpDown(),
                                       Aim = SetAim(Around.NearestEnemy.Position),
                                       Shoot = SetShootMode(Around.NearestEnemy.Position),
                                       SwapWeapon = SetSwapWeapon(true),
                                       PlantMine = SetPlantMine(false),
                                       Reload = SetReload()
                                   };
        }

        public static void GoHeel(Game game, MyUnit me, World around)
        {
            Set(game, me, around);
            me.NextAction = new CustomAction(nameof(GoHeel));

            SetTarget(Around.NearestHealth.Position);

            me.NextAction.Action = new UnitAction
                                   {
                                       Velocity = VelocityLR(Constants.MaxVelocity),
                                       Jump = SetJump(),
                                       JumpDown = SetJumpDown(),
                                       Aim = SetAim(Around.NearestEnemy.Position),
                                       Shoot = SetShootMode(Around.NearestEnemy.Position),
                                       SwapWeapon = SetSwapWeapon(false),
                                       PlantMine = SetPlantMine(false),
                                       Reload = SetReload()
                                   };
        }

        public static void ShootEm(Game game, MyUnit me, World around)
        {
            Set(game, me, around);
            me.NextAction = new CustomAction(nameof(ShootEm));

            SetTarget(Around.NearestEnemy.Position);

            me.NextAction.Action = new UnitAction
                                   {
                                       Velocity = VelocityLR(Constants.MaxVelocity),
                                       Jump = SetJump(),
                                       JumpDown = SetJumpDown(),
                                       Aim = SetAim(Around.NearestEnemy.Position),
                                       Shoot = SetShootMode(Around.NearestEnemy.Position),
                                       SwapWeapon = SetSwapWeapon(false),
                                       PlantMine = SetPlantMine(false),
                                       Reload = SetReload()
                                   };
        }

        public static void ShootEmWithRL(Game game, MyUnit me, World around)
        {
            Set(game, me, around);
            me.NextAction = new CustomAction(nameof(ShootEmWithRL));

            SetTarget(Around.NearestEnemy.Position);

            Me.Target = Measure.GetTargetWithSafeArea(Me.Position, Me.Target, game);

            me.NextAction.Action = new UnitAction
                                   {
                                       Velocity = VelocityLR(Constants.MaxVelocity),
                                       Jump = SetJump(),
                                       JumpDown = SetJumpDown(),
                                       Aim = SetAim(Around.NearestEnemy.Position),
                                       Shoot = SetShootMode(Around.NearestEnemy.Position),
                                       SwapWeapon = SetSwapWeapon(false),
                                       PlantMine = SetPlantMine(false),
                                       Reload = SetReload()
                                   };
        }

        public static void DestroyAllPlantedMines(Game game, MyUnit me, World around)
        {
            Set(game, me, around);
            me.NextAction = new CustomAction(nameof(DestroyAllPlantedMines));

            SetTarget(Around.NearestMine.Position);
            Me.Target = Measure.GetTargetWithSafeArea(Me.Position, Me.Target, game);

            me.NextAction.Action = new UnitAction
                                   {
                                       Velocity = VelocityLR(Constants.MaxVelocity),
                                       Jump = SetJump(),
                                       JumpDown = SetJumpDown(),
                                       Aim = SetAim(Around.NearestMine.Position),
                                       Shoot = SetShootMode(Around.NearestMine.Position),
                                       SwapWeapon = SetSwapWeapon(false),
                                       PlantMine = SetPlantMine(false),
                                       Reload = SetReload()
                                   };
        }

        #endregion

        #region Setters

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

        private static bool SetShootMode(Vec2Double targetPosition, bool? shoot = null)
        {
            if (shoot != null)
            {
                Me.Shoot = shoot.Value;
            }
            else
            {
                if (Me.RLEquiped)
                {
                    if (Me.Health <= Constants.OneShotRLHealth && // not kamikaze shooting
                        Around.NearestEnemy.Health > 70 &&
                        Measure.GetDistance(Me.Position, Around.NearestEnemy.Position) <= 5)
                    {
                        return false;
                    }

                    return Measure.IsStraightVisible(Me, targetPosition, Game) &&
                           Measure.RLAimed(Me);
                }

                return Measure.IsStraightVisible(Me, targetPosition, Game);
            }

            return Me.Shoot;
        }

        private static double VelocityLR(double velocity)
        {
            if ((int) Me.Position.X == (int) Me.Target.X
                && (int) Me.Position.Y == (int) Me.Target.Y)
            {
                return 0;
            }

            if (Me.Position.X < Me.Target.X)
            {
                return velocity;
            }

            if (Me.Position.X > Me.Target.X)
            {
                return velocity * -1;
            }

            return 0;
        }

        private static void SetTarget(Vec2Double target)
        {
            Me.Target = Measure.CheckSpringsNear(target, Game);
        }

        private static Vec2Double SetAim(Vec2Double target)
        {
            Me.Aim = new Vec2Double(target.X - Me.Position.X, target.Y - Me.Position.Y);
            return Me.Aim;
        }

        private static bool SetJump()
        {
            if ((int) Me.Target.Y > (int) Me.Position.Y
                || Math.Abs(Me.Target.X - Me.Position.X) > 1
                && (Me.OnLadder
                    || Around.WallNear
                    || !Me.OnGround))
            {
                Me.Jump = true;
            }
            else if (Me.NextAction.Name != nameof(TakeBestWeapon) &&
                     Me.NextAction.Name != nameof(GoHeel) &&
                     !Measure.IsStraightVisible(Me, Around.NearestEnemy.Position, Game))
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
            if ((int) Me.Target.Y < (int) Me.Position.Y
                && !Me.Jump)
            {
                Me.JumpDown = true;
            }
            else
            {
                Me.JumpDown = false;
            }

            return Me.JumpDown;
        }

        #endregion
    }
}