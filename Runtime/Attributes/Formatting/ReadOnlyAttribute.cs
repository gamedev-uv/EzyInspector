using System;
using UnityEngine;

namespace UV.EzyInspector
{
    /// <summary>
    /// Makes the member readonly 
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class ReadOnlyAttribute : PropertyAttribute { }
}
