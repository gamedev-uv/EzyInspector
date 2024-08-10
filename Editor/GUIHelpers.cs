using UnityEditor;
using UnityEngine;

namespace UV.EzyInspector.Editors
{
    /// <summary>
    /// GUI helper functions 
    /// </summary>
    public static class GUIHelpers
    {
        /// <summary>
        /// Draws a button on the inspector with the given gui content 
        /// </summary>
        /// <param name="_">The editor under which the button is to be drawn</param>
        /// <param name="buttonGUI">The gui content for the button</param>
        /// <param name="onButtonPressed">The action which is to be called when the button is pressed</param>
        /// <param name="layoutOptions">The GUILayoutOptions for the button</param>
        public static void DrawButton(this Editor _, GUIContent buttonGUI, System.Action onButtonPressed = null, params GUILayoutOption[] layoutOptions)
        {
            if (GUILayout.Button(buttonGUI, layoutOptions))
                onButtonPressed?.Invoke();
        }
    }
}
