using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

namespace UV.BetterInspector.Editors
{
    using UV.Utils;
    using UV.Utils.Editors;

    /// <summary>
    /// A overriden inspector 
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Object), true)]
    public class UV_Inspector : Editor
    {
        /// <summary>
        /// All the members which are to be drawn 
        /// </summary>
        protected Dictionary<string, MemberInfo> _drawableMembers;

        /// <summary>
        /// All the methods which are to be drawn using buttons
        /// </summary>
        protected Dictionary<ButtonAttribute, MethodInfo> _usableMethods;

        protected virtual void OnEnable() => Init();

        /// <summary>
        /// Initializes all the needed variables
        /// </summary>
        protected virtual void Init()
        {
            _drawableMembers = target.GetSerializedMembers();
            _usableMethods = target.GetMethodsWithAttributes<ButtonAttribute>();
        }

        public override void OnInspectorGUI()
        {
            DrawOpenScriptUI();
            DrawPropertiesExcluding(serializedObject, _drawableMembers.Keys.Append("m_Script").ToArray());
            DrawSerializedMembers();
            DrawButtons();

            if(serializedObject.ApplyModifiedProperties())
            {
                var onInspectorUpdate = target.GetMethodWithAttribute<OnInspectorUpdatedAttribute>();
                onInspectorUpdate?.Invoke(target, null);
            }
        }

        /// <summary>
        /// Draws a button to open the script in the specified editor
        /// </summary>
        protected virtual void DrawOpenScriptUI()
        {
            if (target.HasAttribute<HideMonoScriptAttribute>()) return;

            if (GUILayout.Button("Open Script"))
                target.OpenScript();
            GUILayout.Space(10);
        }

        /// <summary>
        /// Draws all the serialized memebers 
        /// </summary>
        protected virtual void DrawSerializedMembers()
        {
            if (_drawableMembers == null || _drawableMembers.Count == 0) return;

            foreach (var member in _drawableMembers)
            {
                //Draw readonly members
                if (member.Value.HasAttribute<ReadOnlyAttribute>())
                {
                    DrawReadOnly(member.Value);
                    continue;
                }

                //Draw member
                var memberObject = serializedObject.FindProperty(member.Key);
                if (memberObject != null)
                    EditorGUILayout.PropertyField(memberObject, true);
            }
        }

        /// <summary>
        /// Draws the readonly property drawer for the member
        /// </summary>
        /// <param name="member">The member to be drawn</param>
        protected virtual void DrawReadOnly(MemberInfo member)
        {
            EditorGUI.BeginDisabledGroup(true);

            var memberObject = serializedObject.FindProperty(member.Name);
            if (memberObject != null)
                EditorGUILayout.PropertyField(memberObject, true);

            EditorGUI.EndDisabledGroup();
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draws all the method buttons on the inspector 
        /// </summary>
        protected virtual void DrawButtons()
        {
            if (_usableMethods == null || _usableMethods.Count == 0) return;

            GUILayout.Space(15);
            foreach (var methodButton in _usableMethods)
            {
                string buttonName = methodButton.Key.Name ?? methodButton.Value.Name;
                if (GUILayout.Button(buttonName))
                    methodButton.Value?.Invoke(target, null);
            }
        }
    }
}
