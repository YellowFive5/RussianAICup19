#region Usings

using AiCup2019.Model;

#endregion

namespace AiCup2019
{
    public class MyUnit : CustomUnit
    {
        public Vec2Double Target { get; set; }
        public bool NeedHeel => Health <= 70;
        public bool BestWeaponTaken { get; set; }
        public Vec2Double Aim { get; set; }
        public bool Jump { get; set; }
        public bool JumpDown { get; set; }
        public bool Shoot { get; set; }
        public bool PlantMine { get; set; }
        public bool SwapWeapon { get; set; }
        public bool Reload { get; set; }
        public CustomAction NextAction { get; set; }
    }
}