#region Usings

using AiCup2019.Model;

#endregion

namespace AiCup2019
{
    public class CustomUnit
    {
        public Unit Man { get; set; }
        public Vec2Double Position => Man.Position;
        public double Distance { get; protected set; }
        public Vec2Double Size => Man.Size;
        public int Health => Man.Health;
        public Weapon? Weapon => Man.Weapon;
        public bool HasWeapon => Weapon != null;
        public bool WithoutWeapon => !HasWeapon;
        public bool RLEquiped => HasWeapon && Weapon.Value.Typ == WeaponType.RocketLauncher;
        public bool RifleEquiped => HasWeapon && Weapon.Value.Typ == WeaponType.AssaultRifle;
        public bool PistolEquiped => HasWeapon && Weapon.Value.Typ == WeaponType.Pistol;
        public bool SeeRight => Man.WalkedRight;
        public bool SeeLeft => !Man.WalkedRight;
        public bool Stand => Man.Stand;
        public bool OnGround => Man.OnGround;
        public bool OnLadder => Man.OnLadder;
        public int Mines => Man.Mines;
        public bool CanPlantMine => Man.Mines > 0;
        public JumpState JumpState => Man.JumpState;
        public bool UnderPlatform { get; set; }
    }
}