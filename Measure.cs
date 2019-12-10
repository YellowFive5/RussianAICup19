﻿#region Usings

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

        public static bool IsStraightVisible(CustomUnit me, CustomUnit target, Game game, Debug debug = null)
        {
            var visibleLine = new List<Tile>();

            var diffX = Math.Abs(me.Position.X - target.Position.X);
            var diffY = Math.Abs(me.Position.Y - target.Position.Y);
            var pointsNumber = (int) GetDistance(me.Position, target.Position);
            var intervalX = diffX / (pointsNumber + 1);
            var intervalY = diffY / (pointsNumber + 1);

            for (var i = 1; i <= pointsNumber; i++)
            {
                double x = 0;
                double y = 0;
                if (me.Position.Y < target.Position.Y && me.Position.X > target.Position.X)
                {
                    x = me.Position.X - intervalX * i;
                    y = me.Position.Y + intervalY * i;
                }
                else if (me.Position.Y > target.Position.Y && me.Position.X < target.Position.X)
                {
                    x = target.Position.X - intervalX * i;
                    y = target.Position.Y + intervalY * i;
                }
                else if (me.Position.Y < target.Position.Y && me.Position.X < target.Position.X)
                {
                    x = target.Position.X - intervalX * i;
                    y = target.Position.Y - intervalY * i;
                }
                else if (me.Position.Y > target.Position.Y && me.Position.X > target.Position.X)
                {
                    x = me.Position.X - intervalX * i;
                    y = me.Position.Y - intervalY * i;
                }

                // System.Numerics.Vector2.
                // //
                // var ang = Math.Pow(Math.Tan((Y - me.Position.Y) / (X - me.Position.X)), -1) - 
                //           Math.Pow(Math.Tan((target.Position.Y - me.Position.Y) / (target.Position.X - me.Position.X)), -1);
                // //
                debug?.Draw(new CustomData.PlacedText("+",
                                                      new Vec2Float((float) x, (float) (y + me.Size.Y / 2)),
                                                      TextAlignment.Center,
                                                      15,
                                                      Constants.BlueColor));

                visibleLine.Add(game.Level.Tiles[(int) x][(int) y]);
            }

            var visible = !visibleLine.Exists(x => x == Tile.Wall);

            return visible;
        }

        public static bool RLAimed(MyUnit me, EnemyUnit aroundNearestEnemy, Game game)
        {
            if (me.WeaponSpread > Constants.RLFireSpread)
            {
                return false;
            }

            return true;
        }

        public static double FindYOnGround(double targetX, Game game)
        {
            for (var i = Constants.MaxYArrayTile-1; i >= 0; i--)
            {
                var tile = game.Level.Tiles[(int) targetX][i];
                if (tile != Tile.Empty)
                {
                    return i+1;
                }
            }

            return 0;
        }
    }
}