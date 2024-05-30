using UnityEngine;

namespace UV.EzyInspector
{
    /// <summary>
    /// Only displays a property based on the condition passed 
    /// </summary>
    public class ShowIfAttribute : PropertyAttribute
    {
        public ShowIfAttribute(string propertyName, object targetValue) 
        {
            PropertyName = propertyName;
            TargetValue = targetValue;
        }

        /// <summary>
        /// The name of the property which needs to be used 
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// The target value of the property 
        /// </summary>
        public object TargetValue { get; private set; } = true;
    }
}
