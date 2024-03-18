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
        /// Draws a button in the inspector with the name of the method 
        /// </summary>
        public ButtonAttribute()
        {
            Name = null;
        }

        /// <summary>
        /// Draws a button in the inspector with the given name
        /// </summary>
        /// <param name="buttonName">The name of button</param>
        public ButtonAttribute(string buttonName)
        {
            Name = buttonName;
        }
    }
}
