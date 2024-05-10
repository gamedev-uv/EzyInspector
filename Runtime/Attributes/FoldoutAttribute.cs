using System;

namespace UV.EzyInspector
{
    /// <summary>
    /// Used to draw foldouts in the inspector 
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class FoldoutAttribute : Attribute
    {
        /// <summary>
        /// The display name of the foldout in the inspector 
        /// </summary>
        public string DisplayName { get; private set; }    

        /// <summary>
        /// Creates a foldout in the inspector with the given display name 
        /// </summary>
        /// <param name="displayName">The display name of the foldout</param>
        public FoldoutAttribute(string displayName)
        {
            DisplayName = displayName;  
        }   
    }
}
