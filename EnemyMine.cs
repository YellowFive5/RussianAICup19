#region Usings

using AiCup2019.Model;

#endregion

namespace AiCup2019
{
    public class EnemyMine
    {
        public Mine Mine { get; }
        public Vec2Double Position => Mine.Position;
        public double Distance { get; }

        public EnemyMine(Mine mine, MyUnit myUnit)
        {
            Mine = mine;
            Distance = Measure.GetDistance(myUnit.Position, mine.Position);
        }
    }
}