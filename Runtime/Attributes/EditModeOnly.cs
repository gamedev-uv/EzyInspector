using UnityEngine;

namespace UV.EzyInspector
{
    /// <summary>
    /// Hides the member when not in edit mode 
    /// </summary>
    public class EditModeOnly : PropertyAttribute
    {
        /// <summary>
        /// The hide mode 
        /// </summary>
        public HideMode HideMode { get; private set; }

        /// <summary>
        /// Hides the member when not in edit mode
        /// </summary>
        public EditModeOnly() { }

        /// <summary>
        /// Hides or makes the member readonly when not in edit mode based on the hideMode
        /// </summary>
        /// <param name="hideMode">Whether the member is to be hidden or made readonly</param>
        public EditModeOnly(HideMode hideMode)
        {
            HideMode = hideMode;
        }   
    }
}
