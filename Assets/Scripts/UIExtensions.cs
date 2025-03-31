using System.Reflection;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public static class UIExtensions 
    {
        private abstract class Accessor : Selectable
        {
            public const int SelectedEnumValue = (int)SelectionState.Selected;
        }

        private static readonly MethodInfo DoStateTransition = typeof(Selectable).GetMethod("DoStateTransition", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly object[] ArgArray = { Accessor.SelectedEnumValue, false };

        public static void SetSelected(this Selectable button)
        {
            DoStateTransition.Invoke(button, ArgArray);
        }
    }
}
