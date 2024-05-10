using System;

namespace UV.EzyInspector
{
    /// <summary>
    /// Used to draw buttons in the inspector
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ButtonAttribute : Attribute
    {
        /// <summary>
        /// The display name of the button
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// The sequence to draw the button in
        /// </summary>
        public EditorDrawSequence DrawSequence { get; private set; }

        /// <summary>
        /// Draws a button in the inspector
        /// </summary>
        /// <param name="buttonName">The name of button</param>
        /// <param name="editorDrawSequence">The target draw sequence of the button</param>
        public ButtonAttribute(string buttonName)
        {
            DisplayName = buttonName;
            DrawSequence = EditorDrawSequence.AfterDefaultEditor;
        }

        /// <summary>
        /// Draws a button in the inspector 
        /// </summary>
        /// <param name="editorDrawSequence">The target draw sequence of the button</param>
        public ButtonAttribute(EditorDrawSequence editorDrawSequence = EditorDrawSequence.AfterDefaultEditor)
        {
            DisplayName = null;
            DrawSequence = editorDrawSequence;
        }

        /// <summary>
        /// Draws a button in the inspector
        /// </summary>
        /// <param name="buttonName">The name of button</param>
        /// <param name="editorDrawSequence">The target draw sequence of the button</param>
        public ButtonAttribute(string buttonName, EditorDrawSequence editorDrawSequence)
        {
            DisplayName = buttonName;
            DrawSequence = editorDrawSequence;
        }
    }
}
