#region Usings

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using AiCup2019.Model;

#endregion

namespace AiCup2019
{
    public class MyStrategy
    {
        public MyStrategy()
        {
            Me = new MyUnit();
            Around = new World();
        }

        private Game Game { get; set; }
        private Debug Debug { get; set; }
        private MyUnit Me { get; }
        private World Around { get; }

        public UnitAction GetAction(Unit unit, Game game, Debug debug)
        {
            NextTick(unit, game, debug);

            WorldScanner.Scan(Game, Me, Around);

            ChooseBehavior();

            IsStraightVisible(Me.Position, Around.NearestEnemy.Position);

            DebugWrite();
            return DoAction();
        }

        private void NextTick(Unit unit, Game game, Debug debug)
        {
            Game = game;
            Debug = debug;
            Me.Man = unit;
        }

        private void ChooseBehavior()
        {
            SetTarget(Around.NearestEnemy.Position);
            SetAim(Around.NearestEnemy.Position);
            SetJump();
        }

        private void SetTarget(Vec2Double target)
        {
            Me.Target = target;
        }

        private void SetAim(Vec2Double target)
        {
            Me.Aim = new Vec2Double(Around.NearestEnemy.Position.X - Me.Position.X, Around.NearestEnemy.Position.Y - Me.Position.Y);
            // Aim = target;
        }

        private void SetJump()
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
        }

        private double VelocityLR(double velocity)
        {
            if (Me.Position.X < Me.Target.X)
            {
                return velocity;
            }

            return velocity * -1;
        }

        private bool IsStraightVisible(Vec2Double obj1, Vec2Double obj2)
        {
            Vec2Double higherObj;
            Vec2Double lowerObj;
            Vec2Double leftObj;
            Vec2Double rightObj;
            int higherSide;
            int lowerSide;
            var windowHighTiles = 0;
            var windowWidthTiles = 0;
            var visibleLine = new List<Tile>();

            if ((int) obj1.Y > (int) obj2.Y)
            {
                higherObj = obj1;
                lowerObj = obj2;
                windowHighTiles = (int) higherObj.Y - (int) lowerObj.Y;
            }
            else if ((int) obj1.Y < (int) obj2.Y)
            {
                higherObj = obj2;
                lowerObj = obj1;
                windowHighTiles = (int) higherObj.Y - (int) lowerObj.Y;
            }
            else
            {
                higherObj = lowerObj = obj1;
            }

            windowHighTiles += 1;

            if ((int) obj1.X < (int) obj2.X)
            {
                leftObj = obj1;
                rightObj = obj2;
                windowWidthTiles = (int) rightObj.X - (int) leftObj.X;
            }
            else if ((int) obj1.X > (int) obj2.X)
            {
                leftObj = obj2;
                rightObj = obj1;
                windowWidthTiles = (int) rightObj.X - (int) leftObj.X;
            }
            else
            {
                leftObj = rightObj = obj1;
            }

            windowWidthTiles += 1;

            if (windowHighTiles > windowWidthTiles)
            {
                higherSide = windowHighTiles;
                lowerSide = windowWidthTiles;
            }
            else if (windowHighTiles < windowWidthTiles)
            {
                higherSide = windowWidthTiles;
                lowerSide = windowHighTiles;
            }
            else
            {
                higherSide = lowerSide = windowHighTiles;
            }

            var coeff = (int) Math.Ceiling(higherSide / (double) lowerSide);
            coeff += 1;
            Debug.Draw(new CustomData.Log($"{higherSide}|{lowerSide}|{coeff}"));

            var n = 0;
            if (leftObj.Equals(lowerObj) && lowerSide != 1)
            {
                for (var i = 0; i < lowerSide; i++)
                {
                    for (var j = 0; j < coeff; j++)
                    {
                        var x = (int) lowerObj.X + j + n > Constants.MaxXArrayTile
                                    ? Constants.MaxXArrayTile
                                    : (int) lowerObj.X + j + n;
                        var y = (int) lowerObj.Y + i > Constants.MaxYArrayTile
                                    ? Constants.MaxYArrayTile
                                    : (int) lowerObj.Y + i;

                        visibleLine.Add(Game.Level.Tiles[x][y]);

                        Debug.Draw(new CustomData.PlacedText("+", new Vec2Float(x, y), TextAlignment.Center, 15, Constants.BlueColor));
                    }

                    n += coeff;
                }
            }
            else if (leftObj.Equals(higherObj) && lowerSide != 1)
            {
                for (var i = 0; i < lowerSide; i++)
                {
                    for (var j = 0; j < coeff; j++)
                    {
                        var x = Constants.MaxXArrayTile - ((int)higherObj.X - j - n) > Constants.MaxXArrayTile
                                    ? Constants.MaxXArrayTile
                                    : Constants.MaxXArrayTile - ((int)higherObj.X - j - n);
                        var y = Constants.MaxYArrayTile - ((int)higherObj.Y - i) < 0
                                    ? 0
                                    : Constants.MaxYArrayTile - ((int)higherObj.Y - i);

                        visibleLine.Add(Game.Level.Tiles[x][y]);

                        Debug.Draw(new CustomData.PlacedText("+", new Vec2Float(x, y), TextAlignment.Center, 15, Constants.BlueColor));
                    }

                    n += coeff - 1;
                }
            }
            else if (higherObj.Equals(lowerObj))
            {
                for (var j = 0; j < higherSide; j++)
                {
                    var x = (int) leftObj.X + 1;
                    var y = (int) leftObj.Y + j > Constants.MaxYArrayTile
                                ? Constants.MaxYArrayTile
                                : (int) leftObj.Y + j;
                    visibleLine.Add(Game.Level.Tiles[x][y]);
                    Debug.Draw(new CustomData.PlacedText("+", new Vec2Float(x, y), TextAlignment.Center, 15, Constants.BlueColor));
                }
            }

            // Debug.Draw(new CustomData.Log($"{visibleLine.Count}"));

            var visible = !visibleLine.Exists(x => x == Tile.Wall);

            Debug.Draw(new CustomData.Line(new Vec2Float((float) Me.Position.X,
                                                         (float) Me.Position.Y + (float) Me.Size.Y / 2),
                                           new Vec2Float((float) Around.NearestEnemy.Position.X,
                                                         (float) Around.NearestEnemy.Position.Y + (float) Around.NearestEnemy.Size.Y / 2),
                                           0.1f,
                                           visible
                                               ? Constants.GreenColor
                                               : Constants.RedColor));
            return visible;
        }

        private UnitAction DoAction()
        {
            var action = new UnitAction
                         {
                             // Velocity = VelocityLR(Const.MaxVelocity),
                             Velocity = 0,
                             Jump = Me.Jump,
                             JumpDown = false,
                             Aim = Me.Aim,
                             Shoot = false,
                             SwapWeapon = true,
                             PlantMine = false,
                             Reload = true
                         };
            return action;
        }

        private void DebugWrite()
        {
            Debug.Draw(new CustomData.Log($"" +
                                          // $"Bullets count: {Around.Bullets.Count} | " +
                                          //  $"Nearest bullet: {(Around.NearestBullet != null ? $"{(int) Around.NearestBullet.Bullet.Position.X}/{(int) Around.NearestBullet.Bullet.Position.Y}/{(int) Around.NearestBullet.Distance}" : "-")} | " +
                                          //  $"Nearest bullet type: {(Around.NearestBullet != null ? $"{Around.NearestBullet.WeaponType}" : "-")} | " +
                                          //  $"Nearest bullet damage: {(Around.NearestBullet != null ? $"{Around.NearestBullet.Damage}" : "-")} | " +
                                          //  $"Nearest enemy {(Around.NearestEnemy != null ? $"{(int) Around.NearestEnemy.Unit.Position.X}/{(int) Around.NearestEnemy.Unit.Position.Y}/{(int) Around.NearestEnemy.Distance}" : "-")} | " +
                                          //  $"Nearest enemy mine {(Around.NearestMine != null ? $"{(int) Around.NearestMine.Mine.Position.X}/{(int) Around.NearestMine.Mine.Position.Y}/{(int) Around.NearestMine.Distance}" : "-")} | " +
                                          //  $"Nearest weapon {(Around.NearestWeapon != null ? Around.NearestWeapon.WeaponType.ToString() : "-")} | " +
                                          //  $"Nearest weapon position {(Around.NearestWeapon != null ? $"{(int) Around.NearestWeapon.Item.Position.X}/{(int) Around.NearestWeapon.Item.Position.Y}/{(int) Around.NearestWeapon.Distance}" : "-")} | " +
                                          //  $"Nearest pistol {(Around.NearestPistol != null ? $"{(int) Around.NearestPistol.Item.Position.X}/{(int) Around.NearestPistol.Item.Position.Y}/{(int) Around.NearestPistol.Distance}" : "-")} | " +
                                          //  $"Nearest rifle {(Around.NearestRifle != null ? $"{(int) Around.NearestRifle.Item.Position.X}/{(int) Around.NearestRifle.Item.Position.Y}/{(int) Around.NearestRifle.Distance}" : "-")} | " +
                                          //  $"Nearest RL {(Around.NearestRLauncher != null ? $"{(int) Around.NearestRLauncher.Item.Position.X}/{(int) Around.NearestRLauncher.Item.Position.Y}/{(int) Around.NearestRLauncher.Distance}" : "-")} | " +
                                          //  $"Nearest health {(Around.NearestHealth != null ? $"{(int) Around.NearestHealth.Item.Position.X}/{(int) Around.NearestHealth.Item.Position.Y}/{(int) Around.NearestHealth.Distance}" : "-")} | " +
                                          //  $"Nearest mine loot {(Around.NearestMineL != null ? $"{(int) Around.NearestMineL.Item.Position.X}/{(int) Around.NearestMineL.Item.Position.Y}/{(int) Around.NearestMineL.Distance}" : "-")} | " +
                                          //  $"Me has weapon: {Me.HasWeapon} | " +
                                          //  $"My weapon type: {(Me.HasWeapon ? $"{Me.Weapon.Value.Typ}" : "-")} | " +
                                          //  $"My health: {Me.Health} | " +
                                          //  $"Nearest enemy health: {Around.NearestEnemy.Health} | " +
                                          //  $"Nearest enemy has weapon: {Around.NearestEnemy.HasWeapon} | " +
                                          //  $"Nearest enemy weapon type: {(Around.NearestEnemy.HasWeapon ? $"{Around.NearestEnemy.Weapon.Value.Typ}" : "-")} | " +
                                          //  $"My magazine ammo: {(Me.HasWeapon ? $"{Me.Weapon.Value.Magazine}" : "-")} | " +
                                          //  $"My tile Top: {Around.NextTileT} | " +
                                          //  $"My tile Bottom: {Around.NextTileB} | " +
                                          //  $"My tile Left: {Around.NextTileL} | " +
                                          //  $"My tile Right: {Around.NextTileR} | " +
                                          //  $"Nearest enemy tile Top: {Around.NearestEnemy.NextTileT} | " +
                                          //  $"Nearest enemy tile Bottom: {Around.NearestEnemy.NextTileB} | " +
                                          //  $"Nearest enemy tile Left: {Around.NearestEnemy.NextTileL} | " +
                                          //  $"Nearest enemy tile Right: {Around.NearestEnemy.NextTileR} | " +
                                          //  $"Me.OnGround: {Me.OnGround} | " +
                                          //  $"Me.OnLadder: {Me.OnLadder} | " +
                                          //  $"Me.Stand: {Me.Stand} | " +
                                          //  $"Me.SeeRight: {Me.SeeRight} | " +
                                          //  $"Me.SeeLeft: {Me.SeeLeft} | " +
                                          //  $"Me.Mines: {Me.Mines} | " +
                                          //  $"Me.CanPlantMine: {Me.CanPlantMine} | " +
                                          //  $"{Game.Level.Tiles[39][29]}" +
                                          ""));

            // Debug.Draw(new CustomData.Line(new Vec2Float((float) Me.Position.X,
            //                                              (float) Me.Position.Y + (float) Me.Size.Y / 2),
            //                                new Vec2Float((float) Around.NearestEnemy.Position.X,
            //                                              (float) Around.NearestEnemy.Position.Y + (float) Around.NearestEnemy.Size.Y / 2),
            //                                0.1f,
            //                                Constants.GreenColor));
        }
    }
}