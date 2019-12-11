#region Usings

using AiCup2019.Model;

#endregion

namespace AiCup2019
{
    public class CustomAction
    {
        public UnitAction Action { get; set; }
        public string Name { get; }

        public CustomAction(string name)
        {
            Name = name;
            Action = new UnitAction();
        }

        public CustomAction()
        {
            Action = new UnitAction();
            Name = "Empty action";
        }
    }
}