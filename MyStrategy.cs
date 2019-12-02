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

            #region Enemy

            public Unit? NearestEnemy { get; set; }
            public Mine? NearestMine { get; set; }

            #endregion

            #region Loot

            public int LootItems { get; set; }
            public LootItem NearestWeapon { get; set; }
            public LootItem NearestPistol { get; set; }
            public LootItem NearestRifle { get; set; }
            public LootItem NearestRLauncher { get; set; }
            public LootItem NearestHealth { get; set; }
            public LootItem NearestMineL { get; set; }

            #endregion
        }

        public MyStrategy()
        {
            Around = new Orientation();
        }

        private static Game Game { get; set; }
        private static Debug Debug { get; set; }
        private Orientation Around { get; }
        private static Unit MyUnit { get; set; }
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
            Parallel.ForEach(Game.LootBoxes,
                             (lootBox) =>
                             {
                                 if (lootBox.Item is Item.Weapon weapon)
                                 {
                                     switch (weapon.WeaponType)
                                     {
                                         case WeaponType.Pistol:
                                             if (Around.NearestPistol == null || GetDistance(MyUnit.Position, lootBox.Position) < GetDistance(MyUnit.Position, Around.NearestPistol.Item.Position))
                                             {
                                                 Around.NearestPistol = new Orientation.LootItem(lootBox, WeaponType.Pistol);
                                             }

                                             break;
                                         case WeaponType.AssaultRifle:
                                             if (Around.NearestRifle == null || GetDistance(MyUnit.Position, lootBox.Position) < GetDistance(MyUnit.Position, Around.NearestRifle.Item.Position))
                                             {
                                                 Around.NearestRifle = new Orientation.LootItem(lootBox, WeaponType.AssaultRifle);
                                             }

                                             break;
                                         case WeaponType.RocketLauncher:
                                             if (Around.NearestRLauncher == null || GetDistance(MyUnit.Position, lootBox.Position) < GetDistance(MyUnit.Position, Around.NearestRLauncher.Item.Position))
                                             {
                                                 Around.NearestRLauncher = new Orientation.LootItem(lootBox, WeaponType.RocketLauncher);
                                             }

                                             break;
                                         default:
                                             throw new ArgumentOutOfRangeException();
                                     }
                                 }
                                 else if (lootBox.Item is Item.HealthPack)
                                 {
                                     if (Around.NearestHealth == null || GetDistance(MyUnit.Position, lootBox.Position) < GetDistance(MyUnit.Position, Around.NearestHealth.Item.Position))
                                     {
                                         Around.NearestHealth = new Orientation.LootItem(lootBox);
                                     }
                                 }
                                 else if (lootBox.Item is Item.Mine)
                                 {
                                     if (Around.NearestMineL == null || GetDistance(MyUnit.Position, lootBox.Position) < GetDistance(MyUnit.Position, Around.NearestMineL.Item.Position))
                                     {
                                         Around.NearestMineL = new Orientation.LootItem(lootBox);
                                     }
                                 }
                             });

            Around.LootItems = Game.LootBoxes.Length;

            var weapons = new List<Orientation.LootItem>();
            if (Around.NearestPistol != null)
            {
                weapons.Add(Around.NearestPistol);
            }

            if (Around.NearestRifle != null)
            {
                weapons.Add(Around.NearestRifle);
            }

            if (Around.NearestRLauncher != null)
            {
                weapons.Add(Around.NearestRLauncher);
            }

            Around.NearestWeapon = weapons.OrderByDescending(x => x.Distance).Last();
        }

        private void ScanNearestEnemy()
        {
            Parallel.ForEach(Game.Units,
                             (unit) =>
                             {
                                 if (unit.PlayerId != MyUnit.PlayerId)
                                 {
                                     if (!Around.NearestEnemy.HasValue || GetDistance(MyUnit.Position, unit.Position) < GetDistance(MyUnit.Position, Around.NearestEnemy.Value.Position))
                                     {
                                         Around.NearestEnemy = unit;
                                     }
                                 }
                             });
        }

        private void ScanNearestMine()
        {
            Parallel.ForEach(Game.Mines,
                             (mine) =>
                             {
                                 if (!Around.NearestMine.HasValue || GetDistance(MyUnit.Position, mine.Position) < GetDistance(MyUnit.Position, Around.NearestMine.Value.Position))
                                 {
                                     Around.NearestMine = mine;
                                 }
                             });
        }

        private void ChooseBehavior()
        {
            if (!MyUnitHasWeapon)
            {
                SetTarget(Around.NearestEnemy.Value.Position);
            }
            else
            {
                SetTarget(Around.NearestEnemy.Value.Position);
            }

            SetAim(Around.NearestEnemy.Value.Position);
            SetJump();
        }

        private void SetTarget(Vec2Double target)
        {
            MyUnitTarget = target;
        }

        private void SetAim(Vec2Double target)
        {
            MyUnitAim = new Vec2Double(Around.NearestEnemy.Value.Position.X - MyUnit.Position.X, Around.NearestEnemy.Value.Position.Y - MyUnit.Position.Y);
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
            // Debug.Draw(new CustomData.Log("Loot is actual: " + LootIsActual));
            var d = GetDistance(MyUnit.Position,Around.NearestEnemy.Value.Position);
            Debug.Draw(new CustomData.Log(d.ToString()));
            Debug.Draw(new CustomData.Log($"NEAR WEAPON {(Around.NearestWeapon != null ? Around.NearestWeapon.WeaponType.ToString() : "-")} | " +
                                          $"POS {(Around.NearestWeapon != null ? $"{Around.NearestWeapon.Item.Position.X.ToString()}/{Around.NearestWeapon.Item.Position.Y.ToString()}/{Around.NearestWeapon.Distance}" : "-")} | " +
                                          $"PISTOL {(Around.NearestPistol != null ? $"{Around.NearestPistol.Item.Position.X.ToString()}/{Around.NearestPistol.Item.Position.Y.ToString()}/{Around.NearestPistol.Distance}" : "-")} | " +
                                          $"RIFLE {(Around.NearestRifle != null ? $"{Around.NearestRifle.Item.Position.X.ToString()}/{Around.NearestRifle.Item.Position.Y.ToString()}/{Around.NearestRifle.Distance}" : "-")} | " +
                                          $"RL {(Around.NearestRLauncher != null ? $"{Around.NearestRLauncher.Item.Position.X.ToString()}/{Around.NearestRLauncher.Item.Position.Y.ToString()}/{Around.NearestRLauncher.Distance}" : "-")} | " +
                                          $"HEALTH {(Around.NearestHealth != null ? $"{Around.NearestHealth.Item.Position.X.ToString()}/{Around.NearestHealth.Item.Position.Y.ToString()}/{Around.NearestHealth.Distance}" : "-")} | " +
                                          $"MINE {(Around.NearestMineL != null ? $"{Around.NearestMineL.Item.Position.X.ToString()}/{Around.NearestMineL.Item.Position.Y.ToString()}/{Around.NearestMineL.Distance}" : "-")} | " +
                                          $""));
            // Debug.Draw(new CustomData.Log($"{(MyUnitHasWeapon ? MyWeapon.Value.Typ.ToString() : "NO WEAPON")}"));
        }

        private static double GetDistance(Vec2Double a, Vec2Double b)
        {
            return (a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y);
        }
    }
}