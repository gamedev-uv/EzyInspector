using System;
namespace UV.BetterInspector
{
    /// <summary>
    /// Calls the method when the inspector of the object is updated
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class OnInspectorUpdatedAttribute : Attribute { }
}
