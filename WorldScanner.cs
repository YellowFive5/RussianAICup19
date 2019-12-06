#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AiCup2019.Model;

#endregion

namespace AiCup2019
{
    public static class WorldScanner
    {
        private static Game Game;
        private static MyUnit Me;
        private static World Around;

        public static void Scan(Game game, MyUnit me, World around)
        {
            Game = game;
            Me = me;
            Around = around;

            ScanLoot();
            ScanNearestEnemy();
            ScanNearestMine();
            ScanBullets();
            ScanTiles();
        }

        private static void ScanTiles()
        {
            Around.NextTileR = Game.Level.Tiles[(int) (Me.Position.X + 1)][(int) Me.Position.Y];
            Around.NextTileL = Game.Level.Tiles[(int) (Me.Position.X - 1)][(int) Me.Position.Y];
            Around.NextTileT = Game.Level.Tiles[(int) Me.Position.X][(int) Me.Position.Y + 1];
            Around.NextTileB = Game.Level.Tiles[(int) Me.Position.X][(int) Me.Position.Y - 1];

            Around.NearestEnemy.NextTileR = Game.Level.Tiles[(int) (Around.NearestEnemy.Position.X + 1)][(int) Around.NearestEnemy.Position.Y];
            Around.NearestEnemy.NextTileL = Game.Level.Tiles[(int) (Around.NearestEnemy.Position.X - 1)][(int) Around.NearestEnemy.Position.Y];
            Around.NearestEnemy.NextTileT = Game.Level.Tiles[(int) Around.NearestEnemy.Position.X][(int) Around.NearestEnemy.Position.Y + 1];
            Around.NearestEnemy.NextTileB = Game.Level.Tiles[(int) Around.NearestEnemy.Position.X][(int) Around.NearestEnemy.Position.Y - 1];
        }

        private static void ScanLoot()
        {
            Around.AllLoot = new List<LootItem>();
            Parallel.ForEach(Game.LootBoxes,
                             (lootBox) =>
                             {
                                 if (lootBox.Item is Item.Weapon weapon)
                                 {
                                     switch (weapon.WeaponType)
                                     {
                                         case WeaponType.Pistol:
                                             Around.AllLoot.Add(new LootItem(lootBox, Me, WeaponType.Pistol));
                                             break;
                                         case WeaponType.AssaultRifle:
                                             Around.AllLoot.Add(new LootItem(lootBox, Me, WeaponType.AssaultRifle));
                                             break;
                                         case WeaponType.RocketLauncher:
                                             Around.AllLoot.Add(new LootItem(lootBox, Me, WeaponType.RocketLauncher));
                                             break;
                                         default:
                                             throw new ArgumentOutOfRangeException();
                                     }
                                 }
                                 else if (lootBox.Item is Item.HealthPack)
                                 {
                                     Around.AllLoot.Add(new LootItem(lootBox, Me));
                                 }
                                 else if (lootBox.Item is Item.Mine)
                                 {
                                     Around.AllLoot.Add(new LootItem(lootBox, Me));
                                 }
                             });

            Around.LootItems = Game.LootBoxes.Length;

            Around.NearestWeapon = Around.AllLoot.Where(l => l.WeaponType != null).OrderByDescending(x => x.Distance).LastOrDefault();
            Around.NearestPistol = Around.AllLoot.Where(l => l.WeaponType == WeaponType.Pistol).OrderByDescending(x => x.Distance).LastOrDefault();
            Around.NearestRifle = Around.AllLoot.Where(l => l.WeaponType == WeaponType.AssaultRifle).OrderByDescending(x => x.Distance).LastOrDefault();
            Around.NearestRLauncher = Around.AllLoot.Where(l => l.WeaponType == WeaponType.RocketLauncher).OrderByDescending(x => x.Distance).LastOrDefault();
            Around.NearestHealth = Around.AllLoot.Where(l => l.Item.Item is Item.HealthPack).OrderByDescending(x => x.Distance).LastOrDefault();
            Around.NearestMineL = Around.AllLoot.Where(l => l.Item.Item is Item.Mine).OrderByDescending(x => x.Distance).LastOrDefault();
        }

        private static void ScanNearestEnemy()
        {
            Around.Enemies = new List<EnemyUnit>();
            Parallel.ForEach(Game.Units,
                             (unit) =>
                             {
                                 if (unit.PlayerId != Me.Man.PlayerId)
                                 {
                                     Around.Enemies.Add(new EnemyUnit(unit, Me));
                                 }
                             });
            Around.NearestEnemy = Around.Enemies.OrderByDescending(u => u.Distance).LastOrDefault();
        }

        private static void ScanNearestMine()
        {
            Around.PlantedMines = new List<EnemyMine>();
            Parallel.ForEach(Game.Mines,
                             (mine) => { Around.PlantedMines.Add(new EnemyMine(mine, Me)); });
            Around.NearestMine = Around.PlantedMines.OrderByDescending(m => m.Distance).LastOrDefault();
        }

        private static void ScanBullets()
        {
            Around.Bullets = new List<EnemyBullet>();
            Parallel.ForEach(Game.Bullets,
                             (bullet) =>
                             {
                                 if (bullet.PlayerId != Me.Man.PlayerId)
                                 {
                                     Around.Bullets.Add(new EnemyBullet(bullet, Me));
                                 }
                             });
            Around.NearestBullet = Around.Bullets.OrderByDescending(b => b.Distance).LastOrDefault();
        }
    }
}