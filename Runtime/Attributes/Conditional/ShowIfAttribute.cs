using System;
using UnityEngine;

namespace UV.EzyInspector
{
    /// <summary>
    /// Only displays a property based on the condition passed 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
    public class ShowIfAttribute : PropertyAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShowIfAttribute"/> class.
        /// Displays the property based on the specified property name and target values.
        /// </summary>
        /// <param name="propertyName">The name of the property which needs to be used.</param>
        /// <param name="targetValues">The target values of the property which determine when the property is shown.</param>
        public ShowIfAttribute(string propertyName, params object[] targetValues)
        {
            PropertyName = propertyName;
            TargetValues = targetValues;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowIfAttribute"/> class.
        /// Displays the property based on the specified property name, target values, and hide mode.
        /// </summary>
        /// <param name="propertyName">The name of the property which needs to be used.</param>
        /// <param name="hideMode">The hide mode of the property when the condition is not met.</param>
        /// <param name="targetValues">The target values of the property which determine when the property is shown.</param>
        public ShowIfAttribute(string propertyName, HideMode hideMode, params object[] targetValues)
        {
            PropertyName = propertyName;
            TargetValues = targetValues;
            HideMode = hideMode;
        }

        /// <summary>
        /// The name of the property which needs to be used 
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// The target value of the property 
        /// </summary>
        public object[] TargetValues { get; private set; }

        /// <summary>
        /// The HideMode of the property 
        /// </summary>
        public HideMode HideMode { get; private set; } = HideMode.Hide;
    }
}
