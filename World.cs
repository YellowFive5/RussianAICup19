#region Usings

using System.Collections.Generic;
using AiCup2019.Model;

#endregion

namespace AiCup2019
{
    public class World
    {
        public List<EnemyUnit> Enemies { get; set; }
        public EnemyUnit NearestEnemy { get; set; }
        public List<EnemyMine> PlantedMines { get; set; }
        public EnemyMine NearestMine { get; set; }
        public List<EnemyBullet> Bullets { get; set; }
        public EnemyBullet NearestBullet { get; set; }
        public int LootItems { get; set; }
        public List<LootItem> AllLoot { get; set; }
        public LootItem NearestWeapon { get; set; }
        public LootItem NearestPistol { get; set; }
        public LootItem NearestRifle { get; set; }
        public LootItem NearestRLauncher { get; set; }
        public LootItem NearestHealth { get; set; }
        public LootItem NearestMineL { get; set; }
        public Tile NextTileT { get; set; }
        public Tile NextTileB { get; set; }
        public Tile NextTileR { get; set; }
        public Tile NextTileL { get; set; }
    }
}