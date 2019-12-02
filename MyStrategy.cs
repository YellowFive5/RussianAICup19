#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AiCup2019.Model;

#endregion

namespace AiCup2019
{
    public class MyStrategy
    {
        private class Orientation
        {
            #region Classes

            public class LootItem
            {
                public LootItem(LootBox item, WeaponType? type = null)
                {
                    Item = item;
                    Distance = GetDistance(MyUnit.Position, item.Position);
                    WeaponType = type;
                }

                public LootBox Item { get; }
                public double Distance { get; }
                public WeaponType? WeaponType { get; }
            }

            public class EnemyUnit
            {
                public Unit Unit { get; }
                public double Distance { get; }

                public EnemyUnit(Unit unit)
                {
                    Unit = unit;
                    Distance = GetDistance(MyUnit.Position, unit.Position);
                }
            }

            public class EnemyMine
            {
                public Mine Mine { get; }
                public double Distance { get; }

                public EnemyMine(Mine mine)
                {
                    Mine = mine;
                    Distance = GetDistance(MyUnit.Position, mine.Position);
                }
            }

            #endregion

            #region Enemy

            public List<EnemyUnit> AllEnemiesUnits { get; set; }
            public EnemyUnit NearestEnemy { get; set; } = new EnemyUnit(new Unit());
            public List<EnemyMine> AllEnemiesMines { get; set; }
            public EnemyMine NearestMine { get; set; } = new EnemyMine(new Mine());

            #endregion

            #region Loot

            public int LootItems { get; set; }
            public List<LootItem> AllLoots { get; set; }
            public LootItem NearestWeapon { get; set; } = new LootItem(new LootBox());
            public LootItem NearestPistol { get; set; } = new LootItem(new LootBox());
            public LootItem NearestRifle { get; set; } = new LootItem(new LootBox());
            public LootItem NearestRLauncher { get; set; } = new LootItem(new LootBox());
            public LootItem NearestHealth { get; set; } = new LootItem(new LootBox());
            public LootItem NearestMineL { get; set; } = new LootItem(new LootBox());

            #endregion
        }

        public MyStrategy()
        {
            Around = new Orientation();
        }

        private static Game Game { get; set; }
        private static Debug Debug { get; set; }
        private static Unit MyUnit { get; set; }
        private Orientation Around { get; }
        private int MyHealth { get; set; }
        private bool MyUnitHasWeapon { get; set; }
        private Weapon? MyWeapon { get; set; }
        private Vec2Double MyUnitTarget { get; set; }
        private Vec2Double MyUnitAim { get; set; }
        private bool Jump { get; set; }

        public UnitAction GetAction(Unit unit, Game game, Debug debug)
        {
            NextTick(unit, game, debug);
            ScanLoot();
            ScanNearestEnemy();
            ScanNearestMine();

            ChooseBehavior();

            DebugWrite();
            return DoAction();
        }

        private void NextTick(Unit unit, Game game, Debug debug)
        {
            MyUnit = unit;
            Game = game;
            Debug = debug;

            MyUnitHasWeapon = MyUnit.Weapon.HasValue;
            if (MyUnitHasWeapon)
            {
                MyWeapon = MyUnit.Weapon;
            }

            MyHealth = MyUnit.Health;

            Around.LootItems = game.LootBoxes.Length;
        }

        private void ScanLoot()
        {
            Around.AllLoots = new List<Orientation.LootItem>();
            Parallel.ForEach(Game.LootBoxes,
                             (lootBox) =>
                             {
                                 if (lootBox.Item is Item.Weapon weapon)
                                 {
                                     switch (weapon.WeaponType)
                                     {
                                         case WeaponType.Pistol:
                                             Around.AllLoots.Add(new Orientation.LootItem(lootBox, WeaponType.Pistol));
                                             break;
                                         case WeaponType.AssaultRifle:
                                             Around.AllLoots.Add(new Orientation.LootItem(lootBox, WeaponType.AssaultRifle));
                                             break;
                                         case WeaponType.RocketLauncher:
                                             Around.AllLoots.Add(new Orientation.LootItem(lootBox, WeaponType.RocketLauncher));
                                             break;
                                         default:
                                             throw new ArgumentOutOfRangeException();
                                     }
                                 }
                                 else if (lootBox.Item is Item.HealthPack)
                                 {
                                     Around.AllLoots.Add(new Orientation.LootItem(lootBox));
                                 }
                                 else if (lootBox.Item is Item.Mine)
                                 {
                                     Around.AllLoots.Add(new Orientation.LootItem(lootBox));
                                 }
                             });

            Around.LootItems = Game.LootBoxes.Length;

            Around.NearestWeapon = Around.AllLoots.Where(l => l.WeaponType != null).OrderByDescending(x => x.Distance).LastOrDefault();
            Around.NearestPistol = Around.AllLoots.Where(l => l.WeaponType == WeaponType.Pistol).OrderByDescending(x => x.Distance).LastOrDefault();
            Around.NearestRifle = Around.AllLoots.Where(l => l.WeaponType == WeaponType.AssaultRifle).OrderByDescending(x => x.Distance).LastOrDefault();
            Around.NearestRLauncher = Around.AllLoots.Where(l => l.WeaponType == WeaponType.RocketLauncher).OrderByDescending(x => x.Distance).LastOrDefault();
            Around.NearestHealth = Around.AllLoots.Where(l => l.Item.Item is Item.HealthPack).OrderByDescending(x => x.Distance).LastOrDefault();
            Around.NearestMineL = Around.AllLoots.Where(l => l.Item.Item is Item.Mine).OrderByDescending(x => x.Distance).LastOrDefault();
        }

        private void ScanNearestEnemy()
        {
            Around.AllEnemiesUnits = new List<Orientation.EnemyUnit>();
            Parallel.ForEach(Game.Units,
                             (unit) =>
                             {
                                 if (unit.PlayerId != MyUnit.PlayerId)
                                 {
                                     Around.AllEnemiesUnits.Add(new Orientation.EnemyUnit(unit));
                                 }
                             });
            Around.NearestEnemy = Around.AllEnemiesUnits.OrderByDescending(u => u.Distance).LastOrDefault();
        }

        private void ScanNearestMine()
        {
            Around.AllEnemiesMines = new List<Orientation.EnemyMine>();
            Parallel.ForEach(Game.Mines,
                             (mine) => { Around.AllEnemiesMines.Add(new Orientation.EnemyMine(mine)); });
            Around.NearestMine = Around.AllEnemiesMines.OrderByDescending(m => m.Distance).LastOrDefault();
        }

        private void ChooseBehavior()
        {
            if (!MyUnitHasWeapon)
            {
                SetTarget(Around.NearestEnemy.Unit.Position);
            }
            else
            {
                SetTarget(Around.NearestEnemy.Unit.Position);
            }

            SetAim(Around.NearestEnemy.Unit.Position);
            SetJump();
        }

        private void SetTarget(Vec2Double target)
        {
            MyUnitTarget = target;
        }

        private void SetAim(Vec2Double target)
        {
            MyUnitAim = new Vec2Double(Around.NearestEnemy.Unit.Position.X - MyUnit.Position.X, Around.NearestEnemy.Unit.Position.Y - MyUnit.Position.Y);
            // Aim = target;
        }

        private void SetJump()
        {
            Jump = true;
            // Jump = Target.Y > MyUnit.Position.Y;
            // if (Target.X > MyUnit.Position.X && Game.Level.Tiles[(int) (MyUnit.Position.X + 1)][(int) MyUnit.Position.Y] == Tile.Wall)
            // {
            //     Jump = true;
            // }
            //
            // if (Target.X < MyUnit.Position.X && Game.Level.Tiles[(int) (MyUnit.Position.X - 1)][(int) MyUnit.Position.Y] == Tile.Wall)
            // {
            //     Jump = true;
            // }
        }

        private UnitAction DoAction()
        {
            var action = new UnitAction();

            var findWeaponAction = new UnitAction
                                   {
                                       Velocity = 99,
                                       Jump = Jump,
                                       JumpDown = !Jump,
                                       Aim = MyUnitAim,
                                       Shoot = false,
                                       SwapWeapon = true,
                                       PlantMine = false
                                   };


            action = findWeaponAction;
            return action;
        }

        private void DebugWrite()
        {
            // Debug.Draw(new CustomData.Log("Target pos X: " + Target.X));
            // Debug.Draw(new CustomData.Log("Target pos Y: " + Target.Y));
            // Debug.Draw(new CustomData.Log("Loot items: " + Around.LootItems));
            // Debug.Draw(new CustomData.Log($"{Around.AllLoots.Count}"));
            // var d = GetDistance(MyUnit.Position, Around.NearestEnemy.Value.Position);
            // Debug.Draw(new CustomData.Log(d.ToString()));
            Debug.Draw(new CustomData.Log($"NEAR WEAPON {(Around.NearestWeapon != null ? Around.NearestWeapon.WeaponType.ToString() : "-")} | " +
                                          $"POS {(Around.NearestWeapon != null ? $"{Around.NearestWeapon.Item.Position.X}/{Around.NearestWeapon.Item.Position.Y.ToString()}/{(int) Around.NearestWeapon.Distance}" : "-")} | " +
                                          $"PISTOL {(Around.NearestPistol != null ? $"{Around.NearestPistol.Item.Position.X.ToString()}/{Around.NearestPistol.Item.Position.Y.ToString()}/{(int) Around.NearestPistol.Distance}" : "-")} | " +
                                          $"RIFLE {(Around.NearestRifle != null ? $"{Around.NearestRifle.Item.Position.X.ToString()}/{Around.NearestRifle.Item.Position.Y.ToString()}/{(int) Around.NearestRifle.Distance}" : "-")} | " +
                                          $"RL {(Around.NearestRLauncher != null ? $"{Around.NearestRLauncher.Item.Position.X.ToString()}/{Around.NearestRLauncher.Item.Position.Y.ToString()}/{(int) Around.NearestRLauncher.Distance}" : "-")} | " +
                                          $"HEALTH {(Around.NearestHealth != null ? $"{Around.NearestHealth.Item.Position.X.ToString()}/{Around.NearestHealth.Item.Position.Y.ToString()}/{(int) Around.NearestHealth.Distance}" : "-")} | " +
                                          $"L-MINE {(Around.NearestMineL != null ? $"{Around.NearestMineL.Item.Position.X.ToString()}/{Around.NearestMineL.Item.Position.Y.ToString()}/{(int) Around.NearestMineL.Distance}" : "-")} | " +
                                          $"ENEMY {(Around.NearestEnemy != null ? $"{Around.NearestEnemy.Unit.Position.X.ToString()}/{Around.NearestEnemy.Unit.Position.Y.ToString()}/{(int) Around.NearestEnemy.Distance}" : "-")} | " +
                                          $"E-MINE {(Around.NearestMine != null ? $"{Around.NearestMine.Mine.Position.X.ToString()}/{Around.NearestMine.Mine.Position.Y.ToString()}/{(int) Around.NearestMine.Distance}" : "-")} | " +
                                          $""));
            // Debug.Draw(new CustomData.Log($"{(MyUnitHasWeapon ? MyWeapon.Value.Typ.ToString() : "NO WEAPON")}"));
        }

        private static double GetDistance(Vec2Double a, Vec2Double b)
        {
            return (a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y);
        }
    }
}