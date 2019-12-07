#region Usings

using AiCup2019.Model;

#endregion

namespace AiCup2019
{
    public class CustomAction
    {
        public UnitAction Action { get; }
        public string Name { get; }

        public CustomAction(string name, UnitAction action)
        {
            Name = name;
            Action = action;
        }

        public CustomAction()
        {
            Action = new UnitAction();
            Name = "Empty action";
        }
    }
}