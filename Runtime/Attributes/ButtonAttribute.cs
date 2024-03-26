using System;

namespace UV.BetterInspector
{
    /// <summary>
    /// Used to draw buttons in the inspector
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ButtonAttribute : Attribute
    {
        /// <summary>
        /// The name of the button
        /// </summary>
        public string Name { get; private set; }

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
            Name = buttonName;
            DrawSequence = EditorDrawSequence.AfterDefaultEditor;
        }

        /// <summary>
        /// Draws a button in the inspector 
        /// </summary>
        /// <param name="editorDrawSequence">The target draw sequence of the button</param>
        public ButtonAttribute(EditorDrawSequence editorDrawSequence = EditorDrawSequence.AfterDefaultEditor)
        {
            Name = null;
            DrawSequence = editorDrawSequence;
        }

        /// <summary>
        /// Draws a button in the inspector
        /// </summary>
        /// <param name="buttonName">The name of button</param>
        /// <param name="editorDrawSequence">The target draw sequence of the button</param>
        public ButtonAttribute(string buttonName, EditorDrawSequence editorDrawSequence)
        {
            Name = buttonName;
            DrawSequence = editorDrawSequence;
        }
    }
}
