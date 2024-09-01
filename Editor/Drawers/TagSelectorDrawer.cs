using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

namespace UV.EzyInspector.Editors
{
    /// <summary>
    /// Drawer for the tag attribute 
    /// </summary>
    [CustomPropertyDrawer(typeof(TagSelectorAttribute))]
    public class TagSelectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.HelpBox(position, "[Tag] is only valid on Strings!", MessageType.Error);
                return;
            }

            var allTags = InternalEditorUtility.tags;
            if (allTags == null || allTags.Length == 0)
            {
                Debug.LogWarning("Tags were not found!, Can't draw tag selector");
                return;
            }

            //Check if the current tag is valid 
            var currentValue = property.stringValue;
            int currentIndex = GetCurrentIndex(currentValue, allTags);
            if (currentIndex == -1)
                property.stringValue = allTags[++currentIndex];


            //Draw the dropdown 
            currentIndex = EditorGUI.Popup(position, label.text, currentIndex, allTags);
            property.stringValue = allTags[currentIndex];
        }

        /// <summary>
        /// Returns the index of the current tag value 
        /// </summary>
        /// <param name="value">The current value of the tag</param>
        /// <param name="tags">All the available tags</param>
        /// <returns>Returns the index if the current tag was found else -1</returns>
        private int GetCurrentIndex(string value, string[] tags)
        {
            for (int i = 0; i < tags.Length; i++)
            {
                var tag = tags[i];
                if (tag == null) continue;
                if (tag.Equals(value)) return i;
            }
            return -1;
        }
    }
}
