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

        public static bool IsStraightVisible(CustomUnit me, Vec2Double target, Game game, Debug debug = null)
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
                var meY = me.Position.Y + 0.9;
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

                // System.Numerics.Vector2.
                // //
                // var ang = Math.Pow(Math.Tan((Y - me.Position.Y) / (X - me.Position.X)), -1) - 
                //           Math.Pow(Math.Tan((target.Position.Y - me.Position.Y) / (target.Position.X - me.Position.X)), -1);
                // //

                debug?.Draw(new CustomData.PlacedText("+",
                                                      new Vec2Float((float) x, (float) y),
                                                      TextAlignment.Center,
                                                      15,
                                                      Constants.BlueColor));

                visibleLine.Add(game.Level.Tiles[(int) x][(int) y]);
            }

            var visible = !visibleLine.Exists(x => x == Tile.Wall);

            return visible;
        }

        public static bool RLAimed(MyUnit me)
        {
            if (me.WeaponSpread > Constants.RLFireSpread)
            {
                return false;
            }

            return true;
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

        public static Vec2Double GetTargetWithSafeArea(Vec2Double mePosition, Vec2Double meTarget, Game game)
        {
            double x;
            if (mePosition.X > meTarget.X)
            {
                x = meTarget.X + Constants.SafeArea > Constants.MaxXArrayTile
                        ? meTarget.X - Constants.SafeArea
                        : meTarget.X + Constants.SafeArea;
            }
            else
            {
                x = meTarget.X - Constants.SafeArea < 0
                        ? meTarget.X + Constants.SafeArea
                        : meTarget.X - Constants.SafeArea;
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