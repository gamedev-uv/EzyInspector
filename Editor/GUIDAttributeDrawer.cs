using UnityEditor;
using UnityEngine;

namespace UV.EzyInspector.Editors
{
    /// <summary>
    /// Custom drawer for GUID Attribute that fetches the Unity GUID and assings it back to the field 
    /// </summary>
    [CustomPropertyDrawer(typeof(GUIDAttribute))]
    public class GUIDDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.HelpBox(position, "Can only be used on strings", MessageType.Error);
                return;
            }

            //Fetch object guid and assign to string 
            Object scriptableObj = property.serializedObject.targetObject;
            string unityManagedGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(scriptableObj));
            property.stringValue = unityManagedGuid;

            //Draw copy button
            var buttonRect = position;
            buttonRect.width = Mathf.Clamp(buttonRect.width * 0.2f, 100, 200);
            if (GUI.Button(buttonRect, $"Copy {property.displayName}"))
                GUIUtility.systemCopyBuffer = property.stringValue;

            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, true);
            GUI.enabled = true;
        }
    }
}
