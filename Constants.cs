#region Usings

using AiCup2019.Model;

#endregion

namespace AiCup2019
{
    public static class Constants
    {
        public static int MaxHealth { get; } = 100;
        public static int OneShotRLHealth { get; } = 50;
        public static int MaxVelocity { get; } = 10;
        public static int FullPistolAmmo { get; } = 8;
        public static int FullRifleAmmo { get; } = 20;
        public static int FullRLAmmo { get; } = 1;
        public static int PistolDamage { get; } = 20;
        public static int RifleDamage { get; } = 5;
        public static int RLDamage { get; } = 30;
        public static int HealthAid { get; } = 50;
        public static int SafeArea { get; } = 9;
        public static ColorFloat RedColor { get; } = new ColorFloat(255, 0, 0, 255);
        public static ColorFloat GreenColor { get; } = new ColorFloat(0, 255, 0, 255);
        public static ColorFloat BlueColor { get; } = new ColorFloat(0, 0, 255, 255);
        public static int MaxXArrayTile { get; set; } = 39;
        public static int MaxYArrayTile { get; set; } = 29;
        public static double RLMinSpread { get; set; } = 0.1d;
        public static double RLFireSpread { get; set; } = 0.25d;
        public static double RLMaxSpread { get; set; } = 0.48d;
    }
}