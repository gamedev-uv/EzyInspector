using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace UV.EzyInspector.Editors
{
    using System.Runtime.Remoting.Messaging;
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
        protected (MemberInfo, object, string, Attribute[])[] _drawableMembers;

        /// <summary>
        /// All the methods which are to be drawn using buttons
        /// </summary>
        protected Dictionary<MethodInfo, ButtonAttribute> _buttonMethods;

        /// <summary>
        /// All the methods which are called when the inspector is updated
        /// </summary>
        protected Dictionary<MethodInfo, OnInspectorUpdatedAttribute> _onInspectorUpdatedMethods;

        /// <summary>
        /// All the methods which are called when the transform is updated
        /// </summary>
        protected Dictionary<MethodInfo, OnTransformUpdatedAttribute> _onTransformUpdated;

        /// <summary>
        /// All the properties which have been successfully found
        /// </summary>
        protected Dictionary<string, SerializedProperty> _foundProperties;

        /// <summary>
        /// All the properties which are disabled / readonly
        /// </summary>
        protected List<SerializedProperty> _disabledProperties;

        protected virtual void OnEnable() => Init();

        /// <summary>
        /// Whether the open mono script button is to be hidden or not
        /// </summary>
        private bool _hideMonoScript;

        /// <summary>
        /// Initializes all the needed variables
        /// </summary>
        protected virtual void Init()
        {
            _drawableMembers = target.GetSerializedMembers();
            _buttonMethods = target.GetMethodsWithAttribute<ButtonAttribute>();
            _onInspectorUpdatedMethods = target.GetMethodsWithAttribute<OnInspectorUpdatedAttribute>();
            _onTransformUpdated = target.GetMethodsWithAttribute<OnTransformUpdatedAttribute>();
            _hideMonoScript = target.HasAttribute<HideMonoScriptAttribute>();
            _foundProperties = new();
        }

        public override void OnInspectorGUI()
        {
            ManagerTransformUpdateMethods();
            DrawOpenScriptUI();
            DrawButtons(EditorDrawSequence.BeforeDefaultEditor);
            DrawDefaultUI();
            DrawButtons(EditorDrawSequence.AfterDefaultEditor);

            //Apply modified properties and check if anything was updated 
            if (!serializedObject.ApplyModifiedProperties()) return;

            //Find methods which are to be called 
            if (_onInspectorUpdatedMethods == null || _onInspectorUpdatedMethods.Count == 0) return;

            //Loop through them and invoke them if the editor play state is correct
            foreach (var pair in _onInspectorUpdatedMethods)
            {
                var method = pair.Key;
                var att = pair.Value;
                if (att.IsCorrectEditorPlayerState()) method?.Invoke(target, null);
            }
        }

        /// <summary>
        /// Calls the methods which are to be called when the transform is updated 
        /// </summary>
        protected virtual void ManagerTransformUpdateMethods()
        {
            var objTarget = target as MonoBehaviour;
            if (target == null || objTarget == null) return;
            if (!objTarget.transform.hasChanged) return;

            //Find methods which are to be called  
            if (_onTransformUpdated == null || _onTransformUpdated.Count == 0) return;

            //Loop through them and invoke them if the editor play state is correct
            foreach (var pair in _onTransformUpdated)
            {
                var method = pair.Key;
                var att = pair.Value;
                if (att.IsCorrectEditorPlayerState()) method?.Invoke(target, null);
            }
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
            if (_hideMonoScript) return;

            if (GUILayout.Button("Open Script"))
                target.OpenScript();
            GUILayout.Space(5);
        }

        /// <summary>
        /// Returns the serialized property with the given propertyPath
        /// </summary>
        /// <param name="propertyPath">The path at which the property is to be found</param>
        /// <returns>returns the serialized property if found else null</returns>
        protected virtual SerializedProperty FindProperty(string propertyPath)
        {
            //Return the property if found in the dictionary
            if (_foundProperties.ContainsKey(propertyPath)) return _foundProperties[propertyPath];

            //Try finding the property under the serialized object 
            var property = serializedObject.FindProperty(propertyPath);
            if (property == null) return null;

            //If the property is found add it to the dictionary
            _foundProperties.Add(propertyPath, property);
            return property;
        }

        /// <summary>
        /// Draws all the serialized memebers 
        /// </summary>
        protected virtual void DrawSerializedMembers()
        {
            _disabledProperties = new();

            var indent = EditorGUI.indentLevel;
            var guiState = GUI.enabled;

            foreach (var memberTuple in _drawableMembers)
            {
                //Try finding the property
                var memberPath = memberTuple.Item3;
                var propertyPath = memberPath.Replace($"{target}.", "");
                SerializedProperty property = FindProperty(propertyPath);

                if (property == null)
                    continue;

                //Access the other variables from the tuple
                var member = memberTuple.Item1;
                var memberObj = memberTuple.Item2;
                var attributes = memberTuple.Item4;

                //Go to initial values
                EditorGUI.indentLevel = indent;
                GUI.enabled = guiState;

                //Check if the property has parent(s) 
                if (propertyPath.Contains('.'))
                {
                    //Try to find the instant parent property 
                    var parentPropertyPath = propertyPath[..propertyPath.LastIndexOf('.')];
                    var parentProperty = FindProperty(parentPropertyPath);
                    if (parentProperty != null && !parentProperty.isExpanded)
                    {
                        property.isExpanded = false;
                        continue;
                    }

                    //Change indent level and readonly based on the parent 
                    EditorGUI.indentLevel = indent + propertyPath.Count(x => x.Equals('.'));
                    var readOnly = _disabledProperties
                                                    .Where(x => x.name.Equals(parentProperty.name))
                                                    .FirstOrDefault() != null;
                    GUI.enabled = !readOnly;
                }

                //Check whether it has the hide in inspector if it does hide it 
                if (memberTuple.HasAttribute<HideInInspector>())
                    continue;

                //If the member has the EditMode Attribute draw it accordingly 
                if (memberTuple.TryGetAttribute(out EditModeOnly editMode) && Application.isPlaying)
                {
                    if (editMode.HideMode == HideMode.Hide) continue;
                    DrawReadOnly(property);
                    continue;
                }

                //Check if member has ShowIfAttribute attribute and draw it accordingly 
                if (memberTuple.TryGetAttribute(out ShowIfAttribute showIf))
                {
                    if (!CheckShowIfProperty(member, memberObj, showIf))
                    {
                        if (showIf.HideMode == HideMode.ReadOnly)
                            DrawReadOnly(property);
                        else
                            property.isExpanded = false;

                        continue;
                    }
                }

                //Draw readonly members
                if (memberTuple.HasAttribute<ReadOnlyAttribute>())
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

            //If the value is null display a help box accordingly 
            if (showIfMember == null || value == null)
            {
                //Extract just the name of the member 
                var memberName = member.Name;
                var startingIndex = memberName.IndexOf('<') + 1;
                var endingIndex = memberName.IndexOf('>');

                if (startingIndex > -1 && endingIndex > -1)
                    memberName = memberName[startingIndex..endingIndex];

                //Draw the help box 
                EditorGUILayout.HelpBox($"\"{showIf.PropertyName}\" not found!\nCan't draw : \"{memberName}\"", MessageType.Error);
                return showIf.TargetValues == null || showIf.TargetValues.Length == 0;
            }

            for (int i = 0; i < showIf.TargetValues.Length; i++)
            {
                var targetValue = showIf.TargetValues[i];
                var targetValueType = targetValue.GetType();

                //Casts the fetched value back into the type value
                var typedValue = value.ChangeType(targetValueType);
                if (typedValue == null) return false;

                //Checks whether the values are the same or not
                if (typedValue.Equals(targetValue))
                    return true;
            }

            return false;
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
