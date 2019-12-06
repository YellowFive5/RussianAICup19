#region Usings

using AiCup2019.Model;

#endregion

namespace AiCup2019
{
    public class MyUnit : CustomUnit
    {
        public Vec2Double Target { get; set; }
        public Vec2Double Aim { get; set; }
        public bool Jump { get; set; }
    }
}