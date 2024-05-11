using UnityEngine;

namespace UV.EzyInspector
{
    /// <summary>
    /// Only displays a property based on the condition passed 
    /// </summary>
    public class ShowIf : PropertyAttribute
    {
        public ShowIf(string propertyName)
        {
            PropertyName = propertyName;
            TargetBoolValue = true;
        }

        public ShowIf(string propertyName, bool targetBoolValue) : this(propertyName)
        {
            TargetBoolValue = targetBoolValue;
        }

        /// <summary>
        /// The name of the property which needs to be used 
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// The target boolean value of the property 
        /// </summary>
        public bool TargetBoolValue { get; private set; } = true;
    }
}
