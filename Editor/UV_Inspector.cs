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
        protected Dictionary<MethodInfo, ButtonAttribute> _buttonMethods;

        protected virtual void OnEnable() => Init();

        /// <summary>
        /// Initializes all the needed variables
        /// </summary>
        protected virtual void Init()
        {
            _drawableMembers = target.GetSerializedMembers();
            _buttonMethods = target.GetMethodsWithAttribute<ButtonAttribute>();
        }

        public override void OnInspectorGUI()
        {
            DrawOpenScriptUI();
            DrawButtons(EditorDrawSequence.BeforeDefaultEditor);
            DrawDefaultUI();
            DrawButtons(EditorDrawSequence.AfterDefaultEditor);

            if(serializedObject.ApplyModifiedProperties())
            {
                var onInspectorUpdate = target.GetMethodWithAttribute(out OnInspectorUpdatedAttribute att);
                if(att != null)
                {
                    if (att.IsCorrectEditorPlayerState())
                        onInspectorUpdate.Invoke(target, null);
                }
            }
        }

        /// <summary>
        /// Draws the default editor view
        /// </summary>
        protected virtual void DrawDefaultUI()
        {
            DrawPropertiesExcluding(serializedObject, _drawableMembers.Keys.Append("m_Script").ToArray());
            DrawSerializedMembers();
        }

        /// <summary>
        /// Draws a button to open the script in the specified editor
        /// </summary>
        protected virtual void DrawOpenScriptUI()
        {
            if (target.HasAttribute<HideMonoScriptAttribute>()) return;

            if (GUILayout.Button("Open Script"))
                target.OpenScript();
            GUILayout.Space(5);
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
        /// <param name="drawSequence">The sequence for which the buttons are to be drawn</param>
        protected virtual void DrawButtons(EditorDrawSequence drawSequence)
        {
            if (_buttonMethods == null || _buttonMethods.Count == 0) return;

            foreach (var methodButton in _buttonMethods)
            {
                var button = methodButton.Value;
                if (!button.DrawSequence.Equals(drawSequence)) continue;
                DrawButton(methodButton.Key, button);
            }
        }

        /// <summary>
        /// Draws a button in the inspector for the given method styled by the button attribute
        /// </summary>
        /// <param name="method">The method to be drawn</param>
        /// <param name="button">The button used to style the ui</param>
        protected virtual void DrawButton(MethodInfo method, ButtonAttribute button)
        {
            string buttonName = button.DisplayName ?? method.Name;
            if (GUILayout.Button(buttonName))
                method?.Invoke(target, null);

            GUILayout.Space(5);
        }
    }
}
