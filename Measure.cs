#region Usings

using System;
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
    }
}