using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

namespace UV.EzyInspector.Editors
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
        protected (MemberInfo, object, string)[] _drawableMembers;

        /// <summary>
        /// All the methods which are to be drawn using buttons
        /// </summary>
        protected Dictionary<MethodInfo, ButtonAttribute> _buttonMethods;

        /// <summary>
        /// All the properties which are disabled / readonly
        /// </summary>
        protected List<SerializedProperty> _disabledProperties;

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

            //Apply modified properties and check if anything was updated 
            serializedObject.ApplyModifiedProperties();
            if (!EditorUtility.IsDirty(target)) return;

            //Find methods which are to be called 
            var onInspectorUpdatedMethods = target.GetMethodsWithAttribute<OnInspectorUpdatedAttribute>();
            if (onInspectorUpdatedMethods == null || onInspectorUpdatedMethods.Count == 0) return;

            //Loop through them and invoke them if the editor play state is correct
            foreach (var pair in onInspectorUpdatedMethods)
            {
                var method = pair.Key;
                var att = pair.Value;
                if (att.IsCorrectEditorPlayerState()) method?.Invoke(target, null);
            }

            //Clear any dirty on the target
            EditorUtility.ClearDirty(target);
        }

        /// <summary>
        /// Draws the default editor view
        /// </summary>
        protected virtual void DrawDefaultUI()
        {
            DrawPropertiesExcluding(serializedObject, _drawableMembers.Select(x => x.Item3).Append("m_Script").ToArray());
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
            _disabledProperties = new();

            var indent = EditorGUI.indentLevel;
            var guiState = GUI.enabled;

            foreach (var (member, memberObj, memberPath) in _drawableMembers)
            {
                //Fetch the property of the member 
                var propertyPath = memberPath.Replace($"{target}.", "");
                var property = serializedObject.FindProperty(propertyPath);
                if (property == null)
                    continue;

                //Go to initial values
                EditorGUI.indentLevel = indent;
                GUI.enabled = guiState;

                //Check if the property has parent(s) 
                if (propertyPath.Contains('.'))
                {
                    //Try to find the instant parent property 
                    var parentPropertyPath = propertyPath[..propertyPath.LastIndexOf('.')];
                    var parentProperty = serializedObject.FindProperty(parentPropertyPath);
                    if (parentProperty != null && !parentProperty.isExpanded) return;

                    //Change indent level and readonly based on the parent 
                    EditorGUI.indentLevel = indent + propertyPath.Count(x => x.Equals('.'));
                    var readOnly = _disabledProperties
                                                    .Where(x => x.name.Equals(parentProperty.name))
                                                    .FirstOrDefault() != null;
                    GUI.enabled = !readOnly;
                }

                //If the member has the EditMode Attribute draw it accordingly 
                if (member.TryGetAttribute(out EditModeOnly editMode) && Application.isPlaying)
                {
                    if (editMode.HideMode == HideMode.Hide) return;
                    DrawReadOnly(property);
                    continue;
                }

                //Check if member has ShowIfAttribute attribute and draw it accordingly 
                if (member.TryGetAttribute(out ShowIfAttribute showIf))
                {
                    if (!CheckShowIfProperty(member, memberObj, showIf))
                    {
                        DrawReadOnly(property);
                        continue;
                    }
                }

                //Draw readonly members
                if (member.HasAttribute<ReadOnlyAttribute>())
                {
                    DrawReadOnly(property);
                    continue;
                }

                //Draw the member normally 
                EditorGUILayout.PropertyField(property, property.isArray);
            }
        }

        /// <summary>
        /// Check whether the show if property is to be drawn or not
        /// </summary>
        /// <param propertyPath="member">The member with the attribute</param>
        /// <param propertyPath="obj">The member with the attribute</param>
        /// <param propertyPath="showIf">The show if attribute</param>
        /// <returns>Returns true or false based on if the property is to be drawn or not</returns>
        protected virtual bool CheckShowIfProperty(MemberInfo member, object obj, ShowIfAttribute showIf)
        {
            //Find the show if property 
            var showIfMemberPair = obj.GetMember(showIf.PropertyName);
            var showIfMember = showIfMemberPair.Item1;
            var showIfMemberObj = showIfMemberPair.Item2;

            //Return true or false based on if the values are equal or not
            var value = showIfMemberObj.GetValue(showIfMember);

            Debug.Log($"{member.Name} => {showIfMember} {value} == {showIf.TargetValue}");

            if (value == null) return showIf.TargetValue == null;
            return value.Equals(showIf.TargetValue);
        }

        /// <summary>
        /// Draws the readonly property drawer for the property
        /// </summary>
        /// <param propertyPath="property">The property to be drawn</param>
        protected virtual void DrawReadOnly(SerializedProperty property)
        {
            if (property == null) return;

            _disabledProperties ??= new();
            _disabledProperties.Add(property);

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(property, property.isArray);
            EditorGUI.EndDisabledGroup();
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draws all the method buttons on the inspector 
        /// </summary>
        /// <param propertyPath="drawSequence">The sequence for which the buttons are to be drawn</param>
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
        /// <param propertyPath="method">The method to be drawn</param>
        /// <param propertyPath="button">The button used to style the ui</param>
        protected virtual void DrawButton(MethodInfo method, ButtonAttribute button)
        {
            string buttonName = button.DisplayName ?? method.Name;
            if (GUILayout.Button(buttonName))
            {
                method?.Invoke(target, null);
                EditorUtility.SetDirty(this);
            }

            GUILayout.Space(5);
        }
    }
}
