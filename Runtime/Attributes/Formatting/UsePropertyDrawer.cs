using System;

namespace UV.EzyInspector
{
    /// <summary>
    /// Attribute which makes it so the editor uses the property drawer instead of custom drawing it 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class UsePropertyDrawer : Attribute { }
}