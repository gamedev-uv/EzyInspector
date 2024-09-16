using System;

namespace UV.EzyInspector.Editors
{
    /// <summary>
    /// Calls the method whenever the SceneGUI is drawn for the editor 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class OnSceneGUIAttribute : Attribute { }
}
