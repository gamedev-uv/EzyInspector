using UnityEditor;
using UnityEngine;

namespace UV.BetterInspector.Editors
{
    /// <summary>
    /// Drawer that force draws interfaces in the inspector
    /// </summary>
    [CustomPropertyDrawer(typeof(ForceInterfaceAttribute))]
    public class ForceInterfaceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var fInterface = attribute as ForceInterfaceAttribute;

            //Show a help box is the attribute is not on the correct type of field
            if(property.propertyType != SerializedPropertyType.ObjectReference)
            {
                EditorGUI.HelpBox(position, "Can only be used with object references!", MessageType.Error);
                return;
            }

            //Draw the object drawer
            EditorGUI.ObjectField(position, property, label);

            //Fetch the current object value
            var objectValue = property.objectReferenceValue;
            if (objectValue == null) return;

            //If the object is a GameObject fetch the interface if it is found on the object and use it 
            if(objectValue is GameObject obj)
                property.objectReferenceValue = obj.GetComponent(fInterface.InterfaceType);

            //If the object does not inherit from the interface set the value to null 
            var currentInterface = objectValue.GetType().GetInterface(fInterface.InterfaceType.FullName);
            if (currentInterface != null) return;
        }
    }
}
