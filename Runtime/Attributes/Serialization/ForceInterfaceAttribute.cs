using System;
using UnityEngine;

namespace UV.EzyInspector
{
    /// <summary>
    /// Force draws the interface in the inspector 
    /// </summary>
    public class ForceInterfaceAttribute : PropertyAttribute
    {
        /// <summary>
        /// Force draws the interface in the inspector
        /// </summary>
        /// <param name="interfaceType">The type of interface to be drawn</param>
        public ForceInterfaceAttribute(Type interfaceType)
        {
            InterfaceType = interfaceType;
        }

        /// <summary>
        /// The type to draw in the inspector
        /// </summary>
        public Type InterfaceType { get; private set; }
    }
}
