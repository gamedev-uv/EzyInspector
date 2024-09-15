using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace UV.EzyInspector.Editors
{
    /// <summary>
    /// Helper functions used in the EzyInspector package
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// Draws a button in the inspector with the provided GUI content
        /// </summary>
        /// <param name="_">The editor in which the button is drawn</param>
        /// <param name="buttonGUI">The GUI content for the button</param>
        /// <param name="onButtonPressed">The action to invoke when the button is pressed</param>
        /// <param name="layoutOptions">Optional layout options for the button</param>
        public static void DrawButton(this Editor _, GUIContent buttonGUI, System.Action onButtonPressed = null, params GUILayoutOption[] layoutOptions)
        {
            if (GUILayout.Button(buttonGUI, layoutOptions))
                onButtonPressed?.Invoke();
        }

        /// <summary>
        /// Checks if drag-and-drop has occurred within the given area and returns the dropped objects
        /// </summary>
        /// <param name="area">The rectangular area to check for drag-and-drop events</param>
        /// <returns>An array of dropped objects</returns>
        public static Object[] CheckDragAndDrop(this Rect area)
        {
            var objects = Array.Empty<Object>();
            var @event = Event.current;
            var eventType = @event.type;

            if (!(eventType == EventType.DragUpdated || eventType == EventType.DragPerform)) return objects;
            if (!area.Contains(@event.mousePosition)) return objects;

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (eventType == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                return DragAndDrop.objectReferences ?? objects;
            }

            return objects;
        }

        /// <summary>
        /// Checks if drag-and-drop has occurred within the given area and returns objects of the specified type
        /// </summary>
        /// <param name="area">The rectangular area to check for drag-and-drop events</param>
        /// <param name="type">The type of objects to return</param>
        /// <returns>An array of objects cast to the specified type</returns>
        public static Object[] CheckDragAndDrop(this Rect area, Type type)
        {
            var castedObjects = new List<Object>();
            var objects = CheckDragAndDrop(area);
            if (objects.Length == 0) return Array.Empty<Object>();

            foreach (var obj in objects)
            {
                if (obj == null) continue;

                if (obj.GetType() == type)
                {
                    castedObjects.Add(obj);
                }
                else if (obj is GameObject gameObject)
                {
                    if (gameObject.TryGetComponent(type, out Component castedObject))
                        castedObjects.Add(castedObject);
                }
                else if (obj is Component component)
                {
                    if (component.TryGetComponent(type, out Component castedObject))
                        castedObjects.Add(castedObject);
                }
            }

            return castedObjects.ToArray();
        }

        /// <summary>
        /// Checks if drag-and-drop has occurred within the given area and returns objects cast to the specified generic type
        /// </summary>
        /// <typeparam name="T">The type of objects to return</typeparam>
        /// <param name="area">The rectangular area to check for drag-and-drop events</param>
        /// <returns>An array of objects cast to the specified type</returns>
        public static T[] CheckDragAndDrop<T>(this Rect area) where T : Object
        {
            return CheckDragAndDrop(area, typeof(T))
                        .Cast<T>()
                        .ToArray();
        }

        /// <summary>
        /// Finds and returns all the types that inherit from the provided baseType
        /// </summary>
        /// <param name="baseType">The base type</param>
        /// <param name="excludeSystemNamespace">Whether the system namespace is to be excluded from the search</param>
        /// <returns>An array of all the found child types</returns>
        public static Type[] GetAllChildClasses(this Type baseType, bool excludeSystemNamespace = true)
        {
            if (baseType == null) return Array.Empty<Type>();

            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => !excludeSystemNamespace || !assembly.FullName.StartsWith("System"))
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type != null && type.IsClass && type.IsSubclassOf(baseType))
                .ToArray();
        }
    }
}
