using UnityEditor;
using UnityEngine;

namespace UV.EzyInspector.Editors
{
    /// <summary>
    /// The drawer for the GUID Attribute
    /// </summary>
    [CustomPropertyDrawer(typeof(GUIDAttribute))]
    public class GUIDDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var target = property.serializedObject.targetObject;

            //If it is not under a Scriptable Object draw a help box
            if (target is not ScriptableObject SO)
            {
                EditorGUILayout.HelpBox("[GUID] can only be used on members under a Scriptable Object", MessageType.Error);
                return;
            }

            //If it is not a string draw a help box
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUILayout.HelpBox("[GUID] can only be used on strings!", MessageType.Error);
                return;
            }

            //Fetch the GUID and assign it back to the property 
            property.stringValue = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(SO));

            //Draw the Copy button
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Copy GUID"))
                GUIUtility.systemCopyBuffer = property.stringValue;

            //Draw the disabled property 
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(property, new(""), property.isArray);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }
    }
}
