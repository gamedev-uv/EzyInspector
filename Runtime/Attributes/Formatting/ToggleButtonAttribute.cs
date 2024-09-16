using System;
using UnityEngine;

namespace UV.EzyInspector
{
    /// <summary>
    /// Creates a toggle button in the inspector for the given bool
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ToggleButtonAttribute : PropertyAttribute
    {
        public ToggleButtonAttribute(string onLabel, string offLabel)
        {
            OnLabel = onLabel;
            OffLabel = offLabel;
        }
      
        /// <summary>
        /// The label to be used when the button is toggled on
        /// </summary>
        public string OnLabel { get; private set; }

        /// <summary>
        /// The label to be used when the button is toggle off
        /// </summary>
        public string OffLabel { get; private set; }
    }
}
