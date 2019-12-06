#region Usings

using AiCup2019.Model;

#endregion

namespace AiCup2019
{
    public class LootItem
    {
        public LootBox Item { get; }
        public Vec2Double Position => Item.Position;
        public double Distance { get; }
        public WeaponType? WeaponType { get; }

        public LootItem(LootBox item, MyUnit myUnit, WeaponType? type = null)
        {
            Item = item;
            Distance = Measure.GetDistance(myUnit.Position, item.Position);
            WeaponType = type;
        }
    }
}