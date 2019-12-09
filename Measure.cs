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

        public static bool IsStraightVisible(CustomUnit unit1, CustomUnit unit2, Game game, Debug debug = null)
        {
            var visibleLine = new List<Tile>();

            var diffX = Math.Abs(unit1.Position.X - unit2.Position.X);
            var diffY = Math.Abs(unit1.Position.Y - unit2.Position.Y);
            var pointsNumber = (int) GetDistance(unit1.Position, unit2.Position);
            var intervalX = diffX / (pointsNumber + 1);
            var intervalY = diffY / (pointsNumber + 1);

            for (var i = 1; i <= pointsNumber; i++)
            {
                double x = 0;
                double y = 0;
                if (unit1.Position.Y < unit2.Position.Y && unit1.Position.X > unit2.Position.X)
                {
                    x = unit1.Position.X - intervalX * i;
                    y = unit1.Position.Y + intervalY * i;
                }
                else if (unit1.Position.Y > unit2.Position.Y && unit1.Position.X < unit2.Position.X)
                {
                    x = unit2.Position.X - intervalX * i;
                    y = unit2.Position.Y + intervalY * i;
                }
                else if (unit1.Position.Y < unit2.Position.Y && unit1.Position.X < unit2.Position.X)
                {
                    x = unit2.Position.X - intervalX * i;
                    y = unit2.Position.Y - intervalY * i;
                }
                else if (unit1.Position.Y > unit2.Position.Y && unit1.Position.X > unit2.Position.X)
                {
                    x = unit1.Position.X - intervalX * i;
                    y = unit1.Position.Y - intervalY * i;
                }

                debug?.Draw(new CustomData.PlacedText("+",
                                                      new Vec2Float((float) x, (float) (y + unit1.Size.Y / 2)),
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
    }
}