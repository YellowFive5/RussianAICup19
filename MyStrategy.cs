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
        #region Classes

        public class Constants
        {
            public int MaxHealth { get; } = 100;
            public int MaxVelocityR { get; } = 10;
            public int MaxVelocityL { get; } = -10;
            public int FullPistolAmmo { get; } = 8;
            public int FullRifleAmmo { get; } = 20;
            public int FullRLAmmo { get; } = 1;
        }

        public class MyUnit
        {
            public Unit Unit { get; set; }
            public int Health => Unit.Health;
            public int Mines => Unit.Mines;
            public bool CanPlantMine => Unit.Mines > 0;
            public bool HasWeapon => Weapon != null;
            public Weapon? Weapon => Unit.Weapon;
            public bool SeeRight => Unit.WalkedRight;
            public bool SeeLeft => !Unit.WalkedRight;
            public bool Stand => Unit.Stand;
            public bool OnGround => Unit.OnGround;
            public bool OnLadder => Unit.OnLadder;
            public JumpState JumpState => Unit.JumpState;
            public Vec2Double Target { get; set; }
            public Vec2Double Aim { get; set; }
            public bool Jump { get; set; }
        }

        public class LootItem
        {
            public LootItem(LootBox item, WeaponType? type = null)
            {
                Item = item;
                Distance = GetDistance(Me.Unit.Position, item.Position);
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
            public int Health => Unit.Health;
            public bool HasWeapon => Weapon != null;
            public Weapon? Weapon => Unit.Weapon;

            public EnemyUnit(Unit unit)
            {
                Unit = unit;
                Distance = GetDistance(Me.Unit.Position, unit.Position);
            }
        }

        public class EnemyMine
        {
            public Mine Mine { get; }
            public double Distance { get; }

            public EnemyMine(Mine mine)
            {
                Mine = mine;
                Distance = GetDistance(Me.Unit.Position, mine.Position);
            }
        }

        public class EnemyBullet
        {
            public Bullet Bullet { get; }
            public double Distance { get; }

            public EnemyBullet(Bullet bullet)
            {
                Bullet = bullet;
                Distance = GetDistance(Me.Unit.Position, bullet.Position);
            }
        }

        public class World
        {
            #region Enemy

            public List<EnemyUnit> Enemies { get; set; }
            public EnemyUnit NearestEnemy { get; set; }
            public List<EnemyMine> PlantedMines { get; set; }
            public EnemyMine NearestMine { get; set; }
            public List<EnemyBullet> Bullets { get; set; }
            public EnemyBullet NearestBullet { get; set; }

            #endregion

            #region Loot

            public int LootItems { get; set; }
            public List<LootItem> AllLoot { get; set; }
            public LootItem NearestWeapon { get; set; } = new LootItem(new LootBox());
            public LootItem NearestPistol { get; set; } = new LootItem(new LootBox());
            public LootItem NearestRifle { get; set; } = new LootItem(new LootBox());
            public LootItem NearestRLauncher { get; set; } = new LootItem(new LootBox());
            public LootItem NearestHealth { get; set; } = new LootItem(new LootBox());
            public LootItem NearestMineL { get; set; } = new LootItem(new LootBox());

            #endregion

            public Tile NextTileT { get; set; }
            public Tile NextTileB { get; set; }
            public Tile NextTileR { get; set; }
            public Tile NextTileL { get; set; }
        }

        #endregion

        public MyStrategy()
        {
            Me = new MyUnit();
            Around = new World();
            Const = new Constants();
        }

        private static Game Game { get; set; }
        private static Debug Debug { get; set; }
        private static Constants Const { get; set; }
        private static MyUnit Me { get; set; }
        private World Around { get; }

        public UnitAction GetAction(Unit unit, Game game, Debug debug)
        {
            NextTick(unit, game, debug);
            SeeAround();

            ChooseBehavior();

            DebugWrite();
            return DoAction();
        }

        private void NextTick(Unit unit, Game game, Debug debug)
        {
            Game = game;
            Debug = debug;
            Me.Unit = unit;
        }

        private void SeeAround()
        {
            ScanLoot();
            ScanNearestEnemy();
            ScanNearestMine();
            ScanBullets();
            ScanTiles();
        }

        private void ScanTiles()
        {
            Around.NextTileR = Game.Level.Tiles[(int) (Me.Unit.Position.X - 1)][(int) Me.Unit.Position.Y];
            Around.NextTileL = Game.Level.Tiles[(int) (Me.Unit.Position.X + 1)][(int) Me.Unit.Position.Y];
            Around.NextTileT = Game.Level.Tiles[(int) Me.Unit.Position.X][(int) Me.Unit.Position.Y + 1];
            Around.NextTileB = Game.Level.Tiles[(int) Me.Unit.Position.X][(int) Me.Unit.Position.Y - 1];
        }

        private void ScanLoot()
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
                                             Around.AllLoot.Add(new LootItem(lootBox, WeaponType.Pistol));
                                             break;
                                         case WeaponType.AssaultRifle:
                                             Around.AllLoot.Add(new LootItem(lootBox, WeaponType.AssaultRifle));
                                             break;
                                         case WeaponType.RocketLauncher:
                                             Around.AllLoot.Add(new LootItem(lootBox, WeaponType.RocketLauncher));
                                             break;
                                         default:
                                             throw new ArgumentOutOfRangeException();
                                     }
                                 }
                                 else if (lootBox.Item is Item.HealthPack)
                                 {
                                     Around.AllLoot.Add(new LootItem(lootBox));
                                 }
                                 else if (lootBox.Item is Item.Mine)
                                 {
                                     Around.AllLoot.Add(new LootItem(lootBox));
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

        private void ScanNearestEnemy()
        {
            Around.Enemies = new List<EnemyUnit>();
            Parallel.ForEach(Game.Units,
                             (unit) =>
                             {
                                 if (unit.PlayerId != Me.Unit.PlayerId)
                                 {
                                     Around.Enemies.Add(new EnemyUnit(unit));
                                 }
                             });
            Around.NearestEnemy = Around.Enemies.OrderByDescending(u => u.Distance).LastOrDefault();
        }

        private void ScanNearestMine()
        {
            Around.PlantedMines = new List<EnemyMine>();
            Parallel.ForEach(Game.Mines,
                             (mine) => { Around.PlantedMines.Add(new EnemyMine(mine)); });
            Around.NearestMine = Around.PlantedMines.OrderByDescending(m => m.Distance).LastOrDefault();
        }

        private void ScanBullets()
        {
            Around.Bullets = new List<EnemyBullet>();
            Parallel.ForEach(Game.Bullets,
                             (bullet) =>
                             {
                                 if (bullet.PlayerId != Me.Unit.PlayerId)
                                 {
                                     Around.Bullets.Add(new EnemyBullet(bullet));
                                 }
                             });
            Around.NearestBullet = Around.Bullets.OrderByDescending(b => b.Distance).LastOrDefault();
        }

        private void ChooseBehavior()
        {
            SetTarget(Around.NearestEnemy.Unit.Position);
            SetAim(Around.NearestEnemy.Unit.Position);
            SetJump();
        }

        private void SetTarget(Vec2Double target)
        {
            Me.Target = target;
        }

        private void SetAim(Vec2Double target)
        {
            Me.Aim = new Vec2Double(Around.NearestEnemy.Unit.Position.X - Me.Unit.Position.X, Around.NearestEnemy.Unit.Position.Y - Me.Unit.Position.Y);
            // Aim = target;
        }

        private void SetJump()
        {
            Me.Jump = true;
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
            var action = new UnitAction
                         {
                             Velocity = Const.MaxVelocityR,
                             Jump = Me.Jump,
                             JumpDown = !Me.Jump,
                             Aim = Me.Aim,
                             Shoot = false,
                             SwapWeapon = true,
                             PlantMine = false,
                             Reload = true
                         };
            return action;
        }

        private void DebugWrite()
        {
            Debug.Draw(new CustomData.Log($"" +
                                          //$"BULLETS: {Around.Bullets.Count} | " +
                                          //$"NEAREST BULLET: {(Around.NearestBullet != null ? $"{(int) Around.NearestBullet.Bullet.Position.X}/{(int) Around.NearestBullet.Bullet.Position.Y}/{(int) Around.NearestBullet.Distance}" : "-")} | " +
                                          // $"ENEMY {(Around.NearestEnemy != null ? $"{(int) Around.NearestEnemy.Unit.Position.X}/{(int) Around.NearestEnemy.Unit.Position.Y}/{(int) Around.NearestEnemy.Distance}" : "-")} | " +
                                          // $"E-MINE {(Around.NearestMine != null ? $"{(int) Around.NearestMine.Mine.Position.X}/{(int) Around.NearestMine.Mine.Position.Y}/{(int) Around.NearestMine.Distance}" : "-")} | " +
                                          // $"NEAR WEAPON {(Around.NearestWeapon != null ? Around.NearestWeapon.WeaponType.ToString() : "-")} | " +
                                          // $"POS {(Around.NearestWeapon != null ? $"{(int) Around.NearestWeapon.Item.Position.X}/{(int) Around.NearestWeapon.Item.Position.Y}/{(int) Around.NearestWeapon.Distance}" : "-")} | " +
                                          // $"PISTOL {(Around.NearestPistol != null ? $"{(int) Around.NearestPistol.Item.Position.X}/{(int) Around.NearestPistol.Item.Position.Y}/{(int) Around.NearestPistol.Distance}" : "-")} | " +
                                          // $"RIFLE {(Around.NearestRifle != null ? $"{(int) Around.NearestRifle.Item.Position.X}/{(int) Around.NearestRifle.Item.Position.Y}/{(int) Around.NearestRifle.Distance}" : "-")} | " +
                                          // $"RL {(Around.NearestRLauncher != null ? $"{(int) Around.NearestRLauncher.Item.Position.X}/{(int) Around.NearestRLauncher.Item.Position.Y}/{(int) Around.NearestRLauncher.Distance}" : "-")} | " +
                                          // $"HEALTH {(Around.NearestHealth != null ? $"{(int) Around.NearestHealth.Item.Position.X}/{(int) Around.NearestHealth.Item.Position.Y}/{(int) Around.NearestHealth.Distance}" : "-")} | " +
                                          // $"L-MINE {(Around.NearestMineL != null ? $"{(int) Around.NearestMineL.Item.Position.X}/{(int) Around.NearestMineL.Item.Position.Y}/{(int) Around.NearestMineL.Distance}" : "-")} | " +
                                          // $"ENEMY {(Around.NearestEnemy != null ? $"{(int) Around.NearestEnemy.Unit.Position.X}/{(int) Around.NearestEnemy.Unit.Position.Y}/{(int) Around.NearestEnemy.Distance}" : "-")} | " +
                                          // $"E-MINE {(Around.NearestMine != null ? $"{(int) Around.NearestMine.Mine.Position.X}/{(int) Around.NearestMine.Mine.Position.Y}/{(int) Around.NearestMine.Distance}" : "-")} | " +
                                          // $"ME HAS WEAPON: {Me.HasWeapon} | " +
                                          // $"MY WEAPON TYPE: {(Me.HasWeapon ? $"{Me.Weapon.Value.Typ}" : "-")} | " +
                                          //$"MY HEALTH: {Me.Health} | " +
                                          //$"NENEMY HEALTH: {Around.NearestEnemy.Health} | " +
                                          //$"NENEMY HAS WEAPON: {Around.NearestEnemy.HasWeapon} | " +
                                          //$"NENEMY WEAPON TYPE: {(Around.NearestEnemy.HasWeapon ? $"{Around.NearestEnemy.Weapon.Value.Typ}" : "-")} | " +
                                          // $"AMMO: {(Me.HasWeapon ? $"{Me.Weapon.Value.Magazine}" : "-")} | " +
                                          // $"TILET: {Around.NextTileT} | " +
                                          // $"TILEB: {Around.NextTileB} | " +
                                          // $"TILEL: {Around.NextTileL} | " +
                                          // $"TILER: {Around.NextTileR} | " +
                                          // $"Me.OnGround: {Me.OnGround} | " +
                                          // $"Me.OnLadder: {Me.OnLadder} | " +
                                          // $"Me.Stand: {Me.Stand} | " +
                                          // $"Me.SeeRight: {Me.SeeRight} | " +
                                          // $"Me.SeeLeft: {Me.SeeLeft} | " +
                                          $"Me.Mines: {Me.Mines} | " +
                                          $"Me.CanPlantMine: {Me.CanPlantMine} | " +
                                          ""));
        }

        private static double GetDistance(Vec2Double a, Vec2Double b)
        {
            return (a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y);
        }
    }
}