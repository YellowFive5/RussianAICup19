#region Usings

using AiCup2019.Model;

#endregion

namespace AiCup2019
{
    public class EnemyUnit : CustomUnit
    {
        public Tile NextTileT { get; set; }
        public Tile NextTileB { get; set; }
        public Tile NextTileR { get; set; }
        public Tile NextTileL { get; set; }

        public EnemyUnit(Unit unit, MyUnit myUnit)
        {
            Man = unit;
            Distance = Measure.GetDistance(myUnit.Position, unit.Position);
        }
    }
}