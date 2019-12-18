#region Usings

using System;
using System.Collections.Generic;
using AiCup2019.Model;

#endregion

namespace AiCup2019
{
    public static class Measure
    {
        public static double GetDistance(Vec2Double a, Vec2Double b)
        {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        public static bool IsStraightVisible(CustomUnit me, Vec2Double target, Game game, World around, Debug debug = null)
        {
            var visibleLine = new List<Tile>();

            var diffX = Math.Abs(me.Position.X - target.X);
            var diffY = Math.Abs(me.Position.Y - target.Y);
            var pointsNumber = (int) GetDistance(me.Position, target);
            var intervalX = diffX / (pointsNumber + 1);
            var intervalY = diffY / (pointsNumber + 1);

            for (var i = 1; i <= pointsNumber; i++)
            {
                double x = 0;
                double y = 0;
                var meX = me.Position.X;
                var meY = me.Position.Y;
                var tX = target.X;
                var tY = target.Y + 0.9;

                if (meY < tY && meX > tX)
                {
                    x = meX - intervalX * i;
                    y = meY + intervalY * i;
                }
                else if (meY > tY && meX < tX)
                {
                    x = tX - intervalX * i;
                    y = tY + intervalY * i;
                }
                else if (meY < tY && meX < tX)
                {
                    x = tX - intervalX * i;
                    y = tY - intervalY * i;
                }
                else if (meY > tY && meX > tX)
                {
                    x = meX - intervalX * i;
                    y = meY - intervalY * i;
                }
                else if (meY == tY)
                {
                    if (meX > tX)
                    {
                        x = meX - intervalX * i;
                    }
                    else
                    {
                        x = meX + intervalX * i;
                    }

                    y = meY;
                }
                else if (meX == tX)
                {
                    if (meY > tY)
                    {
                        y = meY - intervalY * i;
                    }
                    else
                    {
                        y = meY + intervalY * i;
                    }

                    x = meX;
                }

                var tileX = (int) Math.Round(x) > Constants.MaxXArrayTile
                                ? Constants.MaxXArrayTile
                                : (int) Math.Round(x);
                var tileY = (int) Math.Round(y) > Constants.MaxYArrayTile
                                ? Constants.MaxYArrayTile
                                : (int) Math.Round(y);
                tileX = tileX < 0
                            ? 0
                            : tileX;
                tileY = tileY < 0
                            ? 0
                            : tileY;

                debug?.Draw(new CustomData.PlacedText("+",
                                                      new Vec2Float(tileX, tileY),
                                                      TextAlignment.Center,
                                                      15,
                                                      Constants.BlueColor));

                if (around.Teammate != null)
                {
                    var tile = GetDistance(new Vec2Double(tileX, tileY), around.Teammate.Position) <= 1
                                   ? Tile.Wall
                                   : game.Level.Tiles[tileX][tileY];

                    visibleLine.Add(tile);
                }
                else
                {
                    visibleLine.Add(game.Level.Tiles[tileX][tileY]);
                }
            }

            var visible = !visibleLine.Exists(x => x == Tile.Wall);

            return visible;
        }

        public static bool RLAimed(MyUnit me, Vec2Double target, Game game, Debug debug = null)
        {
            var Ax = me.Position.X;
            var Ay = me.Position.Y;
            var Bx = target.X;
            var By = target.Y + 0.9;
            var angle1 = me.WeaponSpread + Math.Atan2(By - Ay, Bx - Ax);
            var point1 = new Vec2Double(Ax + Math.Cos(angle1) * Constants.SaveAreaRays,
                                        Ay + Math.Sin(angle1) * Constants.SaveAreaRays);
            var angle2 = -me.WeaponSpread + Math.Atan2(By - Ay, Bx - Ax);
            var point2 = new Vec2Double(Ax + Math.Cos(angle2) * Constants.SaveAreaRays,
                                        Ay + Math.Sin(angle2) * Constants.SaveAreaRays);
            var list = new List<Vec2Double>
                       {
                           point1, point2
                       };

            var visibleLine = new List<Tile>();

            foreach (var point in list)
            {
                var diffX = Math.Abs(me.Position.X - point.X);
                var diffY = Math.Abs(me.Position.Y - point.Y);
                var pointsNumber = (int) GetDistance(me.Position, point);
                var intervalX = diffX / (pointsNumber + 1);
                var intervalY = diffY / (pointsNumber + 1);

                for (var i = 1; i <= pointsNumber; i++)
                {
                    double x = 0;
                    double y = 0;
                    var meX = me.Position.X;
                    var meY = me.Position.Y;
                    var tX = point.X;
                    var tY = point.Y;

                    if (meY < tY && meX > tX)
                    {
                        x = meX - intervalX * i;
                        y = meY + intervalY * i;
                    }
                    else if (meY > tY && meX < tX)
                    {
                        x = tX - intervalX * i;
                        y = tY + intervalY * i;
                    }
                    else if (meY < tY && meX < tX)
                    {
                        x = tX - intervalX * i;
                        y = tY - intervalY * i;
                    }
                    else if (meY > tY && meX > tX)
                    {
                        x = meX - intervalX * i;
                        y = meY - intervalY * i;
                    }
                    else if (meY == tY)
                    {
                        if (meX > tX)
                        {
                            x = meX - intervalX * i;
                        }
                        else
                        {
                            x = meX + intervalX * i;
                        }

                        y = meY;
                    }
                    else if (meX == tX)
                    {
                        if (meY > tY)
                        {
                            y = meY - intervalY * i;
                        }
                        else
                        {
                            y = meY + intervalY * i;
                        }

                        x = meX;
                    }

                    var tileX = (int) Math.Round(x) > Constants.MaxXArrayTile
                                    ? Constants.MaxXArrayTile
                                    : (int) Math.Round(x);
                    var tileY = (int) Math.Round(y) > Constants.MaxYArrayTile
                                    ? Constants.MaxYArrayTile
                                    : (int) Math.Round(y);
                    tileX = tileX < 0
                                ? 0
                                : tileX;
                    tileY = tileY < 0
                                ? 0
                                : tileY;

                    debug?.Draw(new CustomData.PlacedText("+",
                                                          new Vec2Float(tileX, tileY),
                                                          TextAlignment.Center,
                                                          15,
                                                          Constants.RedColor));

                    visibleLine.Add(game.Level.Tiles[tileX][tileY]);
                }
            }

            var visible = !visibleLine.Exists(x => x == Tile.Wall);

            return visible;
        }

        public static double FindYOnGround(double targetX, Game game)
        {
            for (var i = Constants.MaxYArrayTile - 1; i >= 0; i--)
            {
                var tile = game.Level.Tiles[(int) targetX][i];
                if (tile != Tile.Empty)
                {
                    return i + 1;
                }
            }

            return 0;
        }

        public static Vec2Double GetTargetWithSafeArea(int saveArea, Vec2Double mePosition, Vec2Double meTarget, Game game)
        {
            double x;
            if (mePosition.X > meTarget.X)
            {
                x = meTarget.X + saveArea > Constants.MaxXArrayTile
                        ? meTarget.X - saveArea
                        : meTarget.X + saveArea;
            }
            else
            {
                x = meTarget.X - saveArea < 0
                        ? meTarget.X + saveArea
                        : meTarget.X - saveArea;
                if (x < 1)
                {
                    x = 1;
                }
            }

            var y = FindYOnGround(x, game);

            return new Vec2Double(x, y);
        }

        public static Vec2Double CheckSpringsNear(Vec2Double target, Game game)
        {
            var tileRight = game.Level.Tiles[(int) target.X + 1][(int) target.Y];
            var tileLeft = game.Level.Tiles[(int) target.X - 1][(int) target.Y];
            if (tileRight == Tile.JumpPad || tileLeft == Tile.JumpPad)
            {
                Vec2Double fixedTarget;

                fixedTarget = tileRight == Tile.JumpPad
                                  ? new Vec2Double(target.X - 0.5, target.Y)
                                  : new Vec2Double(target.X + 0.5, target.Y);
                return fixedTarget;
            }

            return target;
        }
    }
}