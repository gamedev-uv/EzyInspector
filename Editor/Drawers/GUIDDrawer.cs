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

            //Draw the disabled property 
            Rect drawRect = new(position);
            drawRect.width *= 0.8f;
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.PropertyField(drawRect, property, new GUIContent());
            EditorGUI.EndDisabledGroup();

            //Draw the Copy button
            drawRect.x += drawRect.width;
            drawRect.width = position.width - drawRect.width;
            if (GUI.Button(drawRect, $"Copy {property.displayName}"))
                GUIUtility.systemCopyBuffer = property.stringValue;
        }
    }
}
