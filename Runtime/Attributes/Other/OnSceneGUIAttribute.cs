using System;

namespace UV.EzyInspector
{
    /// <summary>
    /// Calls the method whenever the SceneGUI is drawn for the editor 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class OnSceneGUIAttribute : Attribute { }
}
