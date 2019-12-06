#region Usings

using AiCup2019.Model;

#endregion

namespace AiCup2019
{
    public class EnemyBullet
    {
        public Bullet Bullet { get; }
        public double Distance { get; }
        public WeaponType WeaponType => Bullet.WeaponType;
        public int Damage => Bullet.Damage;
        public Vec2Double Velocity => Bullet.Velocity;

        public EnemyBullet(Bullet bullet, MyUnit myUnit)
        {
            Bullet = bullet;
            Distance = Measure.GetDistance(myUnit.Position, bullet.Position);
        }
    }
}