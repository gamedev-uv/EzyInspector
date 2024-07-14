using System;

namespace UV.EzyInspector
{
    /// <summary>
    /// Used to draw buttons in the inspector
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ButtonAttribute : SerializeMemberAttribute
    {
        /// <summary>
        /// The display name of the button
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// Draws a button in the inspector
        /// </summary>
        /// <param name="buttonName">The name of button</param>
        public ButtonAttribute(string buttonName = null)
        {
            DisplayName = buttonName;
        }
    }
}
