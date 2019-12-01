#region Usings

using System.Threading.Tasks;
using AiCup2019.Model;

#endregion

namespace AiCup2019
{
    public class MyStrategy
    {
        private Game Game { get; set; }
        private Debug Debug { get; set; }

        #region MyUnit

        private Unit MyUnit { get; set; }
        private int MyHealth { get; set; }
        private bool HasWeapon { get; set; }
        private Weapon? MyWeapon { get; set; }
        private Vec2Double Target { get; set; }
        private Vec2Double Aim { get; set; }
        private bool Jump { get; set; }

        #endregion

        #region Enemy

        private Unit NearestEnemy { get; set; }
        private Mine NearestMine { get; set; }

        #endregion

        #region Loot

        private bool LootIsActual { get; set; }
        private int LootItems { get; set; }
        private LootBox NearestWeapon { get; set; }
        private WeaponType NearestWeaponType { get; set; }
        private LootBox? PistolLoot { get; set; }
        private LootBox? RifleLoot { get; set; }
        private LootBox? RLauncherLoot { get; set; }
        private LootBox? HealthLoot { get; set; }
        private LootBox? MineLoot { get; set; }

        #endregion

        public UnitAction GetAction(Unit unit, Game game, Debug debug)
        {
            NextTick(unit, game, debug);
            ScanLootIfNotActual();
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

            HasWeapon = MyUnit.Weapon.HasValue;
            if (HasWeapon)
            {
                MyWeapon = MyUnit.Weapon;
            }

            MyHealth = MyUnit.Health;

            LootIsActual = LootItems == game.LootBoxes.Length;
        }

        private void ScanLootIfNotActual()
        {
            if (!LootIsActual)
            {
                Parallel.ForEach(Game.LootBoxes,
                                 (lootBox) =>
                                 {
                                     if (lootBox.Item is Item.Weapon weapon)
                                     {
                                         if (weapon.WeaponType == WeaponType.Pistol)
                                         {
                                             if (PistolLoot.HasValue && GetDistance(MyUnit.Position, lootBox.Position) < GetDistance(MyUnit.Position, PistolLoot.Value.Position))
                                             {
                                                 PistolLoot = lootBox;
                                             }
                                         }
                                         else if (weapon.WeaponType == WeaponType.AssaultRifle)
                                         {
                                             if (RifleLoot.HasValue && GetDistance(MyUnit.Position, lootBox.Position) < GetDistance(MyUnit.Position, RifleLoot.Value.Position))
                                             {
                                                 RifleLoot = lootBox;
                                             }
                                         }
                                         else if (weapon.WeaponType == WeaponType.RocketLauncher)
                                         {
                                             if (RLauncherLoot.HasValue && GetDistance(MyUnit.Position, lootBox.Position) < GetDistance(MyUnit.Position, RLauncherLoot.Value.Position))
                                             {
                                                 RLauncherLoot = lootBox;
                                             }
                                         }
                                     }
                                     else if (lootBox.Item is Item.HealthPack)
                                     {
                                         if (HealthLoot.HasValue && GetDistance(MyUnit.Position, lootBox.Position) < GetDistance(MyUnit.Position, HealthLoot.Value.Position))
                                         {
                                             HealthLoot = lootBox;
                                         }
                                     }
                                     else if (lootBox.Item is Item.Mine)
                                     {
                                         if (MineLoot.HasValue && GetDistance(MyUnit.Position, lootBox.Position) < GetDistance(MyUnit.Position, MineLoot.Value.Position))
                                         {
                                             MineLoot = lootBox;
                                         }
                                     }
                                 });

                LootItems = Game.LootBoxes.Length;
            }
        }

        private void ScanNearestEnemy()
        {
            Parallel.ForEach(Game.Units,
                             (other) =>
                             {
                                 if (other.PlayerId != MyUnit.PlayerId)
                                 {
                                     if (GetDistance(MyUnit.Position, other.Position) < GetDistance(MyUnit.Position, NearestEnemy.Position))
                                     {
                                         NearestEnemy = other;
                                         return;
                                     }

                                     NearestEnemy = other;
                                 }
                             });
        }

        private void ScanNearestMine()
        {
            Parallel.ForEach(Game.Mines,
                             (mine) =>
                             {
                                 if (GetDistance(MyUnit.Position, mine.Position) < GetDistance(MyUnit.Position, NearestMine.Position))
                                 {
                                     NearestMine = mine;
                                     return;
                                 }

                                 NearestMine = mine;
                             });
        }

        private void ChooseBehavior()
        {
            if (!HasWeapon)
            {
                var nearestPistolDist = PistolLoot.HasValue
                                            ? GetDistance(MyUnit.Position, PistolLoot.Value.Position)
                                            : 999;
                var nearestRifleDist = RifleLoot.HasValue
                                           ? GetDistance(MyUnit.Position, RifleLoot.Value.Position)
                                           : 999;
                var nearestRLauncherDist = RLauncherLoot.HasValue
                                               ? GetDistance(MyUnit.Position, RLauncherLoot.Value.Position)
                                               : 999;

                if (PistolLoot.HasValue ||
                    RifleLoot.HasValue)
                {
                    NearestWeapon = nearestPistolDist < nearestRifleDist
                                        ? PistolLoot.Value
                                        : RifleLoot.Value;
                    NearestWeaponType = ((Item.Weapon) NearestWeapon.Item).WeaponType;
                }

                SetTarget(NearestWeapon.Position);
            }
            else
            {
                SetTarget(NearestEnemy.Position);
            }

            SetAim(NearestEnemy.Position);
            SetJump();
        }

        private void SetTarget(Vec2Double target)
        {
            Target = target;
        }

        private void SetAim(Vec2Double target)
        {
            Aim = new Vec2Double(NearestEnemy.Position.X - MyUnit.Position.X, NearestEnemy.Position.Y - MyUnit.Position.Y);
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
                                       Aim = Aim,
                                       Shoot = true,
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
            Debug.Draw(new CustomData.Log("Loot items: " + LootItems));
            // Debug.Draw(new CustomData.Log("Loot is actual: " + LootIsActual));
            // Debug.Draw(new CustomData.Log("Health: " + MyHealth));
            Debug.Draw(new CustomData.Log("Nearest weapon: " + NearestWeaponType));
            Debug.Draw(new CustomData.Log($"{(HasWeapon ? MyWeapon.Value.Typ.ToString() : "NO WEAPON")}"));
        }

        private static double GetDistance(Vec2Double a, Vec2Double b)
        {
            return (a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y);
        }
    }
}